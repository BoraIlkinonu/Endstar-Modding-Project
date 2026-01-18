using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class PhysicsCubeController : WorldUsableController, IStartSubscriber, IGameEndSubscriber
{
	public enum PushDirection : byte
	{
		N,
		S,
		W,
		E
	}

	public enum PushState : byte
	{
		Idle,
		Push,
		Pull
	}

	private enum GroundState
	{
		FlatGround,
		SlopeGround,
		NotGrounded
	}

	public struct StateChangeInfo : INetworkSerializable
	{
		public uint frame;

		public NetworkObjectReference interactor;

		public int motorFrame;

		public PushState pushState;

		public PushDirection pushDirection;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref frame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref interactor, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref pushState, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref pushDirection, default(FastBufferWriter.ForEnums));
		}
	}

	[Serializable]
	public struct GroundingSide
	{
		public Transform side;

		public List<Transform> corners;
	}

	private const float PLAYER_COLLISION_BLOCKER_DISTANCE = -0.8f;

	private const float PUSH_SIDE_RAYCAST_OFFSET = 0.475f;

	private const float PUSH_SIDE_RAYCAST_DISTANCE = 1f;

	private const float PUSH_OVER_LEDGE_ROTATION_CORRECTION_SPEED = 8f;

	[SerializeField]
	private Rigidbody targetRigidbody;

	[SerializeField]
	private Collider targetCollider;

	[SerializeField]
	private WorldUsableDefinition worldUsableDefinition;

	[SerializeField]
	private NetworkRigidbodyController networkRigidbodyController;

	[SerializeField]
	private bool AllowRidingDownSlope;

	[SerializeField]
	private float pushSpeed;

	[SerializeField]
	private float pullSpeed;

	[SerializeField]
	private AnimationCurve speedCurve;

	[SerializeField]
	private uint speedCurveFrames;

	[SerializeField]
	private float verticalForce = 0.48f;

	[SerializeField]
	private float interactionTargetSideDistance = 0.5f;

	[SerializeField]
	private float interactorSideOffsetDistance = 0.25f;

	[SerializeField]
	private float interactionTargetVerticalDistance = -0.5f;

	[SerializeField]
	private ForceMode forceMode;

	[SerializeField]
	private float rotationCorrectionSpeed;

	private NetworkVariable<StateChangeInfo> networkedInteraction = new NetworkVariable<StateChangeInfo>();

	[SerializeField]
	private LayerMask groundedLayerMask;

	[SerializeField]
	private float groudFixOffset = 0.505f;

	[SerializeField]
	private float slopeHeightGrabTargetOffset = 0.2f;

	[SerializeField]
	private List<GroundingSide> groundingSides;

	[Header("Not Grounded")]
	[SerializeField]
	private float notGroundedDrag = 3f;

	[SerializeField]
	private float notGroundedAngularDrag = 3f;

	[Header("Flat Grounded")]
	[SerializeField]
	private float groundedDrag = 4f;

	[SerializeField]
	private float groundedAngularDrag = 12f;

	[Header("Slope Grounded")]
	[SerializeField]
	private float slopeGroundedDrag = 4f;

	[SerializeField]
	private float slopeGroundedAngularDrag = 18f;

	[Header("Player Collision")]
	[SerializeField]
	private BoxCollider playerCollisionBlocker;

	private GroundState currentGroundState;

	protected static RaycastHit[] RaycastHitCache = new RaycastHit[2];

	[SerializeField]
	private WorldUsableInteractable interactable;

	public override WorldUsableDefinition WorldUsableDefinition => worldUsableDefinition;

	public override void CancelInteraction(InteractorBase interactor)
	{
		if (IsInteractingWith(interactor))
		{
			ClearInteraction();
		}
	}

	public override bool IsInteractingWith(InteractorBase interactor)
	{
		if ((bool)interactor)
		{
			return (NetworkObject)networkedInteraction.Value.interactor == interactor.NetworkObject;
		}
		return false;
	}

	public override bool AttemptInteract(InteractorBase interactorBase, int colliderIndex)
	{
		if ((NetworkObject)networkedInteraction.Value.interactor == null)
		{
			if (base.IsServer)
			{
				StateChangeInfo value = networkedInteraction.Value;
				value.frame = NetClock.CurrentFrame;
				value.interactor = interactorBase.NetworkObject;
				float angle = PlayerController.DirectionToAngle(targetRigidbody.position - interactorBase.transform.position);
				value.pushDirection = AngleToPushDirection(angle);
				networkedInteraction.Value = value;
			}
			return true;
		}
		return false;
	}

	private void ClearInteraction()
	{
		if (base.IsServer)
		{
			StateChangeInfo value = networkedInteraction.Value;
			value.interactor = default(NetworkObjectReference);
			value.pushState = PushState.Idle;
			networkedInteraction.Value = value;
		}
	}

	public void EndlessStart()
	{
	}

	public void EndlessGameEnd()
	{
	}

	private void CheckGrounding(bool forceUnground)
	{
		if (!forceUnground)
		{
			for (int i = 0; i < groundingSides.Count; i++)
			{
				int num = Physics.RaycastNonAlloc(groundingSides[i].side.position, groundingSides[i].side.up, RaycastHitCache, 0.07f, groundedLayerMask);
				if (num <= 0)
				{
					continue;
				}
				for (int j = 0; j < num; j++)
				{
					RaycastHit raycastHit = RaycastHitCache[j];
					if (raycastHit.collider != targetCollider)
					{
						float num2 = Vector3.Angle(raycastHit.normal, Vector3.up);
						if (Mathf.Approximately(0f, num2))
						{
							targetRigidbody.position = new Vector3(targetRigidbody.position.x, raycastHit.point.y + groudFixOffset, targetRigidbody.position.z);
							targetRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
							base.transform.position = new Vector3(targetRigidbody.position.x, raycastHit.point.y + groudFixOffset, targetRigidbody.position.z);
							targetRigidbody.angularDrag = groundedAngularDrag;
							targetRigidbody.drag = groundedDrag;
							currentGroundState = GroundState.FlatGround;
							return;
						}
						if (Mathf.Abs(45f - num2) < 10f)
						{
							targetRigidbody.position = raycastHit.point + groundingSides[i].side.up * -1f * groudFixOffset;
							targetRigidbody.constraints = RigidbodyConstraints.None;
							targetRigidbody.angularDrag = slopeGroundedAngularDrag;
							targetRigidbody.drag = slopeGroundedDrag;
							currentGroundState = GroundState.SlopeGround;
							return;
						}
					}
				}
			}
		}
		targetRigidbody.constraints = RigidbodyConstraints.None;
		targetRigidbody.angularDrag = notGroundedAngularDrag;
		targetRigidbody.drag = notGroundedDrag;
		currentGroundState = GroundState.NotGrounded;
	}

	public void EquipmentInteractFrame(PushState pushState)
	{
		if (networkedInteraction.Value.pushState != pushState && base.IsServer)
		{
			StateChangeInfo value = networkedInteraction.Value;
			value.frame = NetClock.CurrentFrame;
			value.pushState = pushState;
			networkedInteraction.Value = value;
		}
	}

	public PushState EquipmentInteractFrame(int verticalInput, float cameraRotation)
	{
		PushState pushState = PushState.Idle;
		if (verticalInput != 0)
		{
			float current = PlayerController.MotionInputToRotation(0, verticalInput) + cameraRotation;
			float pushDirectionFloat = GetPushDirectionFloat();
			float num = Mathf.Abs(Mathf.DeltaAngle(current, pushDirectionFloat));
			pushState = ((!Mathf.Approximately(num, 90f)) ? ((num < 90f) ? PushState.Push : PushState.Pull) : ((verticalInput > 0) ? PushState.Push : PushState.Pull));
		}
		EquipmentInteractFrame(pushState);
		return pushState;
	}

	private void OnDrawGizmos()
	{
		if (base.IsSpawned && (NetworkObject)networkedInteraction.Value.interactor != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(GetInteractorTargetPosition(), 0.1f);
		}
	}

	public Vector3 GetInteractorTargetPosition()
	{
		Vector3 pushDirectionV = GetPushDirectionV3(networkedInteraction.Value.pushDirection);
		Vector3 vector = targetRigidbody.position + new Vector3(0f, slopeHeightGrabTargetOffset, 0f);
		Vector3 vector2 = vector + pushDirectionV * (interactionTargetSideDistance + interactorSideOffsetDistance) * -1f;
		Vector3 vector3 = vector2 + new Vector3(0f, interactionTargetVerticalDistance, 0f);
		if (!AllowRidingDownSlope && currentGroundState == GroundState.SlopeGround)
		{
			return vector3 + Vector3.down * 10f;
		}
		int num = Physics.RaycastNonAlloc(vector, pushDirectionV * -1f, RaycastHitCache, interactionTargetSideDistance + interactorSideOffsetDistance, groundedLayerMask);
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				if (RaycastHitCache[i].rigidbody != targetRigidbody)
				{
					if (Mathf.Approximately(90f, Vector3.Angle(RaycastHitCache[i].normal, Vector3.up)))
					{
						return vector3;
					}
					return RaycastHitCache[i].point;
				}
			}
		}
		num = Physics.RaycastNonAlloc(vector2, Vector3.down, RaycastHitCache, 0f - interactionTargetVerticalDistance, groundedLayerMask);
		if (num > 0)
		{
			for (int j = 0; j < num; j++)
			{
				if (RaycastHitCache[j].rigidbody != targetRigidbody)
				{
					return RaycastHitCache[j].point;
				}
			}
		}
		return vector3;
	}

	public void Frame(uint frame, bool forceUnground)
	{
		CheckGrounding(forceUnground);
		if (!base.IsServer && frame != NetClock.CurrentFrame)
		{
			return;
		}
		if (networkedInteraction.Value.interactor.TryGet(out var _) && currentGroundState != GroundState.SlopeGround && CheckIfLowestSideIsSemiGrounded())
		{
			Vector3 forward = NearestWorldAxis(targetRigidbody.rotation * Vector3.forward);
			Vector3 upwards = NearestWorldAxis(targetRigidbody.rotation * Vector3.up);
			Quaternion rot = Quaternion.RotateTowards(targetRigidbody.rotation, Quaternion.LookRotation(forward, upwards), 8f);
			targetRigidbody.MoveRotation(rot);
		}
		int difference = (int)(base.IsServer ? 1 : (frame - networkedInteraction.Value.frame));
		int motorFrame = GetMotorFrame(networkedInteraction.Value.pushState, networkedInteraction.Value.pushDirection, networkedInteraction.Value.motorFrame, difference);
		if (base.IsServer && motorFrame != networkedInteraction.Value.motorFrame)
		{
			StateChangeInfo value = networkedInteraction.Value;
			value.motorFrame = motorFrame;
			networkedInteraction.Value = value;
		}
		if (motorFrame != 0)
		{
			Vector3 vector = GetPushDirectionV3(networkedInteraction.Value.pushDirection) * interactionTargetSideDistance * ((motorFrame > 0) ? 1 : (-1));
			Vector3 force = GetPushDirectionV3(networkedInteraction.Value.pushDirection) * (GetPushPullForce(networkedInteraction.Value.pushState) * speedCurve.Evaluate((float)Mathf.Abs(motorFrame) / (float)speedCurveFrames));
			if (currentGroundState == GroundState.FlatGround)
			{
				force.y = verticalForce;
			}
			else
			{
				force *= 0.6f;
			}
			targetRigidbody.AddForceAtPosition(force, targetRigidbody.position + vector, forceMode);
			Vector3 eulerAngles = targetRigidbody.rotation.eulerAngles;
			Quaternion rot2 = Quaternion.RotateTowards(to: Quaternion.Euler(new Vector3(eulerAngles.x, Mathf.Round(eulerAngles.y / 90f) * 90f, eulerAngles.z)), from: targetRigidbody.rotation, maxDegreesDelta: rotationCorrectionSpeed);
			targetRigidbody.MoveRotation(rot2);
			playerCollisionBlocker.transform.position = targetRigidbody.position + GetPushDirectionV3(networkedInteraction.Value.pushDirection) * -0.8f;
		}
		else
		{
			playerCollisionBlocker.transform.localPosition = Vector3.zero;
		}
	}

	private static Vector3 NearestWorldAxis(Vector3 v)
	{
		if (Mathf.Abs(v.x) < Mathf.Abs(v.y))
		{
			v.x = 0f;
			if (Mathf.Abs(v.y) < Mathf.Abs(v.z))
			{
				v.y = 0f;
			}
			else
			{
				v.z = 0f;
			}
		}
		else
		{
			v.y = 0f;
			if (Mathf.Abs(v.x) < Mathf.Abs(v.z))
			{
				v.x = 0f;
			}
			else
			{
				v.z = 0f;
			}
		}
		return v;
	}

	private bool CheckIfLowestSideIsSemiGrounded()
	{
		GroundingSide side = groundingSides[0];
		for (int i = 1; i < groundingSides.Count; i++)
		{
			if (groundingSides[i].side.position.y < side.side.position.y)
			{
				side = groundingSides[i];
			}
		}
		if (CheckSideSemiGroundDirection(side, Vector3.forward) || CheckSideSemiGroundDirection(side, Vector3.back) || CheckSideSemiGroundDirection(side, Vector3.left) || CheckSideSemiGroundDirection(side, Vector3.right))
		{
			return true;
		}
		return false;
	}

	private bool CheckSideSemiGroundDirection(GroundingSide side, Vector3 direction)
	{
		int num = Physics.RaycastNonAlloc(side.side.position + direction * 0.475f, Vector3.down, RaycastHitCache, 1f, groundedLayerMask);
		for (int i = 0; i < num; i++)
		{
			if (RaycastHitCache[i].normal.Approximately(Vector3.up))
			{
				return true;
			}
		}
		return false;
	}

	private int GetMotorFrame(PushState pushState, PushDirection pushDirection, int motorFrame, int difference)
	{
		switch (pushState)
		{
		case PushState.Idle:
			if (motorFrame == 0)
			{
				return 0;
			}
			if (motorFrame > 0)
			{
				return Mathf.Max(motorFrame - difference, 0);
			}
			return Mathf.Min(motorFrame + difference, 0);
		case PushState.Push:
			return Mathf.Min(motorFrame + difference, (int)speedCurveFrames);
		default:
			return Mathf.Max(motorFrame - difference, (int)(0L - (long)speedCurveFrames));
		}
	}

	private float GetPushPullForce(PushState state)
	{
		return state switch
		{
			PushState.Push => pushSpeed, 
			PushState.Pull => 0f - pullSpeed, 
			_ => 0f, 
		};
	}

	public Vector3 GetPushDirectionV3(PushDirection dir)
	{
		return dir switch
		{
			PushDirection.E => Vector3.right, 
			PushDirection.S => Vector3.back, 
			PushDirection.W => Vector3.left, 
			_ => Vector3.forward, 
		};
	}

	public float GetPushDirectionFloat()
	{
		if (networkedInteraction.Value.pushDirection == PushDirection.E)
		{
			return 90f;
		}
		if (networkedInteraction.Value.pushDirection == PushDirection.S)
		{
			return 180f;
		}
		if (networkedInteraction.Value.pushDirection == PushDirection.W)
		{
			return 270f;
		}
		return 0f;
	}

	public PushDirection AngleToPushDirection(float angle)
	{
		angle = Mathf.Repeat(angle, 360f);
		if (angle < 45f)
		{
			return PushDirection.N;
		}
		if (angle < 135f)
		{
			return PushDirection.E;
		}
		if (angle < 225f)
		{
			return PushDirection.S;
		}
		if (angle < 315f)
		{
			return PushDirection.W;
		}
		return PushDirection.N;
	}

	public void TriggerTeleport(Vector3 position, TeleportType teleportType)
	{
		ClearInteraction();
		networkRigidbodyController.TriggerTeleport(position, teleportType);
	}

	protected override void __initializeVariables()
	{
		if (networkedInteraction == null)
		{
			throw new Exception("PhysicsCubeController.networkedInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		networkedInteraction.Initialize(this);
		__nameNetworkVariable(networkedInteraction, "networkedInteraction");
		NetworkVariableFields.Add(networkedInteraction);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "PhysicsCubeController";
	}
}
