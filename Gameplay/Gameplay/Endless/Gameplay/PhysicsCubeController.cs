using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200030E RID: 782
	public class PhysicsCubeController : WorldUsableController, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x1700039E RID: 926
		// (get) Token: 0x06001223 RID: 4643 RVA: 0x000599BD File Offset: 0x00057BBD
		public override WorldUsableDefinition WorldUsableDefinition
		{
			get
			{
				return this.worldUsableDefinition;
			}
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x000599C5 File Offset: 0x00057BC5
		public override void CancelInteraction(InteractorBase interactor)
		{
			if (this.IsInteractingWith(interactor))
			{
				this.ClearInteraction();
			}
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x000599D6 File Offset: 0x00057BD6
		public override bool IsInteractingWith(InteractorBase interactor)
		{
			return interactor && this.networkedInteraction.Value.interactor == interactor.NetworkObject;
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x00059A04 File Offset: 0x00057C04
		public override bool AttemptInteract(InteractorBase interactorBase, int colliderIndex)
		{
			if (this.networkedInteraction.Value.interactor == null)
			{
				if (base.IsServer)
				{
					PhysicsCubeController.StateChangeInfo value = this.networkedInteraction.Value;
					value.frame = NetClock.CurrentFrame;
					value.interactor = interactorBase.NetworkObject;
					float num = PlayerController.DirectionToAngle(this.targetRigidbody.position - interactorBase.transform.position);
					value.pushDirection = this.AngleToPushDirection(num);
					this.networkedInteraction.Value = value;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x00059AA0 File Offset: 0x00057CA0
		private void ClearInteraction()
		{
			if (base.IsServer)
			{
				PhysicsCubeController.StateChangeInfo value = this.networkedInteraction.Value;
				value.interactor = default(NetworkObjectReference);
				value.pushState = PhysicsCubeController.PushState.Idle;
				this.networkedInteraction.Value = value;
			}
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void EndlessStart()
		{
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void EndlessGameEnd()
		{
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x00059AE4 File Offset: 0x00057CE4
		private void CheckGrounding(bool forceUnground)
		{
			if (!forceUnground)
			{
				for (int i = 0; i < this.groundingSides.Count; i++)
				{
					int num = Physics.RaycastNonAlloc(this.groundingSides[i].side.position, this.groundingSides[i].side.up, PhysicsCubeController.RaycastHitCache, 0.07f, this.groundedLayerMask);
					if (num > 0)
					{
						for (int j = 0; j < num; j++)
						{
							RaycastHit raycastHit = PhysicsCubeController.RaycastHitCache[j];
							if (raycastHit.collider != this.targetCollider)
							{
								float num2 = Vector3.Angle(raycastHit.normal, Vector3.up);
								if (Mathf.Approximately(0f, num2))
								{
									this.targetRigidbody.position = new Vector3(this.targetRigidbody.position.x, raycastHit.point.y + this.groudFixOffset, this.targetRigidbody.position.z);
									this.targetRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
									base.transform.position = new Vector3(this.targetRigidbody.position.x, raycastHit.point.y + this.groudFixOffset, this.targetRigidbody.position.z);
									this.targetRigidbody.angularDrag = this.groundedAngularDrag;
									this.targetRigidbody.drag = this.groundedDrag;
									this.currentGroundState = PhysicsCubeController.GroundState.FlatGround;
									return;
								}
								if (Mathf.Abs(45f - num2) < 10f)
								{
									this.targetRigidbody.position = raycastHit.point + this.groundingSides[i].side.up * -1f * this.groudFixOffset;
									this.targetRigidbody.constraints = RigidbodyConstraints.None;
									this.targetRigidbody.angularDrag = this.slopeGroundedAngularDrag;
									this.targetRigidbody.drag = this.slopeGroundedDrag;
									this.currentGroundState = PhysicsCubeController.GroundState.SlopeGround;
									return;
								}
							}
						}
					}
				}
			}
			this.targetRigidbody.constraints = RigidbodyConstraints.None;
			this.targetRigidbody.angularDrag = this.notGroundedAngularDrag;
			this.targetRigidbody.drag = this.notGroundedDrag;
			this.currentGroundState = PhysicsCubeController.GroundState.NotGrounded;
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x00059D38 File Offset: 0x00057F38
		public void EquipmentInteractFrame(PhysicsCubeController.PushState pushState)
		{
			if (this.networkedInteraction.Value.pushState != pushState && base.IsServer)
			{
				PhysicsCubeController.StateChangeInfo value = this.networkedInteraction.Value;
				value.frame = NetClock.CurrentFrame;
				value.pushState = pushState;
				this.networkedInteraction.Value = value;
			}
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x00059D8C File Offset: 0x00057F8C
		public PhysicsCubeController.PushState EquipmentInteractFrame(int verticalInput, float cameraRotation)
		{
			PhysicsCubeController.PushState pushState = PhysicsCubeController.PushState.Idle;
			if (verticalInput != 0)
			{
				float num = PlayerController.MotionInputToRotation(0, verticalInput) + cameraRotation;
				float pushDirectionFloat = this.GetPushDirectionFloat();
				float num2 = Mathf.Abs(Mathf.DeltaAngle(num, pushDirectionFloat));
				if (Mathf.Approximately(num2, 90f))
				{
					pushState = ((verticalInput > 0) ? PhysicsCubeController.PushState.Push : PhysicsCubeController.PushState.Pull);
				}
				else
				{
					pushState = ((num2 < 90f) ? PhysicsCubeController.PushState.Push : PhysicsCubeController.PushState.Pull);
				}
			}
			this.EquipmentInteractFrame(pushState);
			return pushState;
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x00059DE8 File Offset: 0x00057FE8
		private void OnDrawGizmos()
		{
			if (!base.IsSpawned)
			{
				return;
			}
			if (this.networkedInteraction.Value.interactor != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(this.GetInteractorTargetPosition(), 0.1f);
			}
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x00059E38 File Offset: 0x00058038
		public Vector3 GetInteractorTargetPosition()
		{
			Vector3 pushDirectionV = this.GetPushDirectionV3(this.networkedInteraction.Value.pushDirection);
			Vector3 vector = this.targetRigidbody.position + new Vector3(0f, this.slopeHeightGrabTargetOffset, 0f);
			Vector3 vector2 = vector + pushDirectionV * (this.interactionTargetSideDistance + this.interactorSideOffsetDistance) * -1f;
			Vector3 vector3 = vector2 + new Vector3(0f, this.interactionTargetVerticalDistance, 0f);
			if (!this.AllowRidingDownSlope && this.currentGroundState == PhysicsCubeController.GroundState.SlopeGround)
			{
				return vector3 + Vector3.down * 10f;
			}
			int num = Physics.RaycastNonAlloc(vector, pushDirectionV * -1f, PhysicsCubeController.RaycastHitCache, this.interactionTargetSideDistance + this.interactorSideOffsetDistance, this.groundedLayerMask);
			if (num > 0)
			{
				int i = 0;
				while (i < num)
				{
					if (PhysicsCubeController.RaycastHitCache[i].rigidbody != this.targetRigidbody)
					{
						if (Mathf.Approximately(90f, Vector3.Angle(PhysicsCubeController.RaycastHitCache[i].normal, Vector3.up)))
						{
							return vector3;
						}
						return PhysicsCubeController.RaycastHitCache[i].point;
					}
					else
					{
						i++;
					}
				}
			}
			num = Physics.RaycastNonAlloc(vector2, Vector3.down, PhysicsCubeController.RaycastHitCache, -this.interactionTargetVerticalDistance, this.groundedLayerMask);
			if (num > 0)
			{
				for (int j = 0; j < num; j++)
				{
					if (PhysicsCubeController.RaycastHitCache[j].rigidbody != this.targetRigidbody)
					{
						return PhysicsCubeController.RaycastHitCache[j].point;
					}
				}
			}
			return vector3;
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x00059FF4 File Offset: 0x000581F4
		public void Frame(uint frame, bool forceUnground)
		{
			this.CheckGrounding(forceUnground);
			if (!base.IsServer && frame != NetClock.CurrentFrame)
			{
				return;
			}
			NetworkObject networkObject;
			if (this.networkedInteraction.Value.interactor.TryGet(out networkObject, null) && this.currentGroundState != PhysicsCubeController.GroundState.SlopeGround && this.CheckIfLowestSideIsSemiGrounded())
			{
				Vector3 vector = PhysicsCubeController.NearestWorldAxis(this.targetRigidbody.rotation * Vector3.forward);
				Vector3 vector2 = PhysicsCubeController.NearestWorldAxis(this.targetRigidbody.rotation * Vector3.up);
				Quaternion quaternion = Quaternion.RotateTowards(this.targetRigidbody.rotation, Quaternion.LookRotation(vector, vector2), 8f);
				this.targetRigidbody.MoveRotation(quaternion);
			}
			int num = (int)(base.IsServer ? 1U : (frame - this.networkedInteraction.Value.frame));
			int motorFrame = this.GetMotorFrame(this.networkedInteraction.Value.pushState, this.networkedInteraction.Value.pushDirection, this.networkedInteraction.Value.motorFrame, num);
			if (base.IsServer && motorFrame != this.networkedInteraction.Value.motorFrame)
			{
				PhysicsCubeController.StateChangeInfo value = this.networkedInteraction.Value;
				value.motorFrame = motorFrame;
				this.networkedInteraction.Value = value;
			}
			if (motorFrame != 0)
			{
				Vector3 vector3 = this.GetPushDirectionV3(this.networkedInteraction.Value.pushDirection) * this.interactionTargetSideDistance * (float)((motorFrame > 0) ? 1 : (-1));
				Vector3 vector4 = this.GetPushDirectionV3(this.networkedInteraction.Value.pushDirection) * (this.GetPushPullForce(this.networkedInteraction.Value.pushState) * this.speedCurve.Evaluate((float)Mathf.Abs(motorFrame) / this.speedCurveFrames));
				if (this.currentGroundState == PhysicsCubeController.GroundState.FlatGround)
				{
					vector4.y = this.verticalForce;
				}
				else
				{
					vector4 *= 0.6f;
				}
				this.targetRigidbody.AddForceAtPosition(vector4, this.targetRigidbody.position + vector3, this.forceMode);
				Vector3 eulerAngles = this.targetRigidbody.rotation.eulerAngles;
				Vector3 vector5 = new Vector3(eulerAngles.x, Mathf.Round(eulerAngles.y / 90f) * 90f, eulerAngles.z);
				Quaternion quaternion2 = Quaternion.RotateTowards(this.targetRigidbody.rotation, Quaternion.Euler(vector5), this.rotationCorrectionSpeed);
				this.targetRigidbody.MoveRotation(quaternion2);
				this.playerCollisionBlocker.transform.position = this.targetRigidbody.position + this.GetPushDirectionV3(this.networkedInteraction.Value.pushDirection) * -0.8f;
				return;
			}
			this.playerCollisionBlocker.transform.localPosition = Vector3.zero;
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x0005A2D4 File Offset: 0x000584D4
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

		// Token: 0x06001231 RID: 4657 RVA: 0x0005A378 File Offset: 0x00058578
		private bool CheckIfLowestSideIsSemiGrounded()
		{
			PhysicsCubeController.GroundingSide groundingSide = this.groundingSides[0];
			for (int i = 1; i < this.groundingSides.Count; i++)
			{
				if (this.groundingSides[i].side.position.y < groundingSide.side.position.y)
				{
					groundingSide = this.groundingSides[i];
				}
			}
			return this.CheckSideSemiGroundDirection(groundingSide, Vector3.forward) || this.CheckSideSemiGroundDirection(groundingSide, Vector3.back) || this.CheckSideSemiGroundDirection(groundingSide, Vector3.left) || this.CheckSideSemiGroundDirection(groundingSide, Vector3.right);
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x0005A420 File Offset: 0x00058620
		private bool CheckSideSemiGroundDirection(PhysicsCubeController.GroundingSide side, Vector3 direction)
		{
			int num = Physics.RaycastNonAlloc(side.side.position + direction * 0.475f, Vector3.down, PhysicsCubeController.RaycastHitCache, 1f, this.groundedLayerMask);
			for (int i = 0; i < num; i++)
			{
				if (PhysicsCubeController.RaycastHitCache[i].normal.Approximately(Vector3.up))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x0005A494 File Offset: 0x00058694
		private int GetMotorFrame(PhysicsCubeController.PushState pushState, PhysicsCubeController.PushDirection pushDirection, int motorFrame, int difference)
		{
			if (pushState == PhysicsCubeController.PushState.Idle)
			{
				if (motorFrame == 0)
				{
					return 0;
				}
				if (motorFrame > 0)
				{
					return Mathf.Max(motorFrame - difference, 0);
				}
				return Mathf.Min(motorFrame + difference, 0);
			}
			else
			{
				if (pushState == PhysicsCubeController.PushState.Push)
				{
					return Mathf.Min(motorFrame + difference, (int)this.speedCurveFrames);
				}
				return Mathf.Max(motorFrame - difference, (int)(-(int)((ulong)this.speedCurveFrames)));
			}
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x0005A4E9 File Offset: 0x000586E9
		private float GetPushPullForce(PhysicsCubeController.PushState state)
		{
			if (state == PhysicsCubeController.PushState.Push)
			{
				return this.pushSpeed;
			}
			if (state == PhysicsCubeController.PushState.Pull)
			{
				return -this.pullSpeed;
			}
			return 0f;
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x0005A507 File Offset: 0x00058707
		public Vector3 GetPushDirectionV3(PhysicsCubeController.PushDirection dir)
		{
			if (dir == PhysicsCubeController.PushDirection.E)
			{
				return Vector3.right;
			}
			if (dir == PhysicsCubeController.PushDirection.S)
			{
				return Vector3.back;
			}
			if (dir == PhysicsCubeController.PushDirection.W)
			{
				return Vector3.left;
			}
			return Vector3.forward;
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x0005A52C File Offset: 0x0005872C
		public float GetPushDirectionFloat()
		{
			if (this.networkedInteraction.Value.pushDirection == PhysicsCubeController.PushDirection.E)
			{
				return 90f;
			}
			if (this.networkedInteraction.Value.pushDirection == PhysicsCubeController.PushDirection.S)
			{
				return 180f;
			}
			if (this.networkedInteraction.Value.pushDirection == PhysicsCubeController.PushDirection.W)
			{
				return 270f;
			}
			return 0f;
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x0005A589 File Offset: 0x00058789
		public PhysicsCubeController.PushDirection AngleToPushDirection(float angle)
		{
			angle = Mathf.Repeat(angle, 360f);
			if (angle < 45f)
			{
				return PhysicsCubeController.PushDirection.N;
			}
			if (angle < 135f)
			{
				return PhysicsCubeController.PushDirection.E;
			}
			if (angle < 225f)
			{
				return PhysicsCubeController.PushDirection.S;
			}
			if (angle < 315f)
			{
				return PhysicsCubeController.PushDirection.W;
			}
			return PhysicsCubeController.PushDirection.N;
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x0005A5C1 File Offset: 0x000587C1
		public void TriggerTeleport(Vector3 position, TeleportType teleportType)
		{
			this.ClearInteraction();
			this.networkRigidbodyController.TriggerTeleport(position, teleportType);
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x0005A694 File Offset: 0x00058894
		protected override void __initializeVariables()
		{
			bool flag = this.networkedInteraction == null;
			if (flag)
			{
				throw new Exception("PhysicsCubeController.networkedInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.networkedInteraction.Initialize(this);
			base.__nameNetworkVariable(this.networkedInteraction, "networkedInteraction");
			this.NetworkVariableFields.Add(this.networkedInteraction);
			base.__initializeVariables();
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x0005A6F7 File Offset: 0x000588F7
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x0005A701 File Offset: 0x00058901
		protected internal override string __getTypeName()
		{
			return "PhysicsCubeController";
		}

		// Token: 0x04000F64 RID: 3940
		private const float PLAYER_COLLISION_BLOCKER_DISTANCE = -0.8f;

		// Token: 0x04000F65 RID: 3941
		private const float PUSH_SIDE_RAYCAST_OFFSET = 0.475f;

		// Token: 0x04000F66 RID: 3942
		private const float PUSH_SIDE_RAYCAST_DISTANCE = 1f;

		// Token: 0x04000F67 RID: 3943
		private const float PUSH_OVER_LEDGE_ROTATION_CORRECTION_SPEED = 8f;

		// Token: 0x04000F68 RID: 3944
		[SerializeField]
		private Rigidbody targetRigidbody;

		// Token: 0x04000F69 RID: 3945
		[SerializeField]
		private Collider targetCollider;

		// Token: 0x04000F6A RID: 3946
		[SerializeField]
		private WorldUsableDefinition worldUsableDefinition;

		// Token: 0x04000F6B RID: 3947
		[SerializeField]
		private NetworkRigidbodyController networkRigidbodyController;

		// Token: 0x04000F6C RID: 3948
		[SerializeField]
		private bool AllowRidingDownSlope;

		// Token: 0x04000F6D RID: 3949
		[SerializeField]
		private float pushSpeed;

		// Token: 0x04000F6E RID: 3950
		[SerializeField]
		private float pullSpeed;

		// Token: 0x04000F6F RID: 3951
		[SerializeField]
		private AnimationCurve speedCurve;

		// Token: 0x04000F70 RID: 3952
		[SerializeField]
		private uint speedCurveFrames;

		// Token: 0x04000F71 RID: 3953
		[SerializeField]
		private float verticalForce = 0.48f;

		// Token: 0x04000F72 RID: 3954
		[SerializeField]
		private float interactionTargetSideDistance = 0.5f;

		// Token: 0x04000F73 RID: 3955
		[SerializeField]
		private float interactorSideOffsetDistance = 0.25f;

		// Token: 0x04000F74 RID: 3956
		[SerializeField]
		private float interactionTargetVerticalDistance = -0.5f;

		// Token: 0x04000F75 RID: 3957
		[SerializeField]
		private ForceMode forceMode;

		// Token: 0x04000F76 RID: 3958
		[SerializeField]
		private float rotationCorrectionSpeed;

		// Token: 0x04000F77 RID: 3959
		private NetworkVariable<PhysicsCubeController.StateChangeInfo> networkedInteraction = new NetworkVariable<PhysicsCubeController.StateChangeInfo>(default(PhysicsCubeController.StateChangeInfo), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F78 RID: 3960
		[SerializeField]
		private LayerMask groundedLayerMask;

		// Token: 0x04000F79 RID: 3961
		[SerializeField]
		private float groudFixOffset = 0.505f;

		// Token: 0x04000F7A RID: 3962
		[SerializeField]
		private float slopeHeightGrabTargetOffset = 0.2f;

		// Token: 0x04000F7B RID: 3963
		[SerializeField]
		private List<PhysicsCubeController.GroundingSide> groundingSides;

		// Token: 0x04000F7C RID: 3964
		[Header("Not Grounded")]
		[SerializeField]
		private float notGroundedDrag = 3f;

		// Token: 0x04000F7D RID: 3965
		[SerializeField]
		private float notGroundedAngularDrag = 3f;

		// Token: 0x04000F7E RID: 3966
		[Header("Flat Grounded")]
		[SerializeField]
		private float groundedDrag = 4f;

		// Token: 0x04000F7F RID: 3967
		[SerializeField]
		private float groundedAngularDrag = 12f;

		// Token: 0x04000F80 RID: 3968
		[Header("Slope Grounded")]
		[SerializeField]
		private float slopeGroundedDrag = 4f;

		// Token: 0x04000F81 RID: 3969
		[SerializeField]
		private float slopeGroundedAngularDrag = 18f;

		// Token: 0x04000F82 RID: 3970
		[Header("Player Collision")]
		[SerializeField]
		private BoxCollider playerCollisionBlocker;

		// Token: 0x04000F83 RID: 3971
		private PhysicsCubeController.GroundState currentGroundState;

		// Token: 0x04000F84 RID: 3972
		protected static RaycastHit[] RaycastHitCache = new RaycastHit[2];

		// Token: 0x04000F85 RID: 3973
		[SerializeField]
		private WorldUsableInteractable interactable;

		// Token: 0x0200030F RID: 783
		public enum PushDirection : byte
		{
			// Token: 0x04000F87 RID: 3975
			N,
			// Token: 0x04000F88 RID: 3976
			S,
			// Token: 0x04000F89 RID: 3977
			W,
			// Token: 0x04000F8A RID: 3978
			E
		}

		// Token: 0x02000310 RID: 784
		public enum PushState : byte
		{
			// Token: 0x04000F8C RID: 3980
			Idle,
			// Token: 0x04000F8D RID: 3981
			Push,
			// Token: 0x04000F8E RID: 3982
			Pull
		}

		// Token: 0x02000311 RID: 785
		private enum GroundState
		{
			// Token: 0x04000F90 RID: 3984
			FlatGround,
			// Token: 0x04000F91 RID: 3985
			SlopeGround,
			// Token: 0x04000F92 RID: 3986
			NotGrounded
		}

		// Token: 0x02000312 RID: 786
		public struct StateChangeInfo : INetworkSerializable
		{
			// Token: 0x0600123E RID: 4670 RVA: 0x0005A708 File Offset: 0x00058908
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<uint>(ref this.frame, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<NetworkObjectReference>(ref this.interactor, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<PhysicsCubeController.PushState>(ref this.pushState, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<PhysicsCubeController.PushDirection>(ref this.pushDirection, default(FastBufferWriter.ForEnums));
			}

			// Token: 0x04000F93 RID: 3987
			public uint frame;

			// Token: 0x04000F94 RID: 3988
			public NetworkObjectReference interactor;

			// Token: 0x04000F95 RID: 3989
			public int motorFrame;

			// Token: 0x04000F96 RID: 3990
			public PhysicsCubeController.PushState pushState;

			// Token: 0x04000F97 RID: 3991
			public PhysicsCubeController.PushDirection pushDirection;
		}

		// Token: 0x02000313 RID: 787
		[Serializable]
		public struct GroundingSide
		{
			// Token: 0x04000F98 RID: 3992
			public Transform side;

			// Token: 0x04000F99 RID: 3993
			public List<Transform> corners;
		}
	}
}
