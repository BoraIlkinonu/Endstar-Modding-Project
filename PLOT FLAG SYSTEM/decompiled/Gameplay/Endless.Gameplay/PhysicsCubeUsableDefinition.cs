using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class PhysicsCubeUsableDefinition : WorldUsableDefinition
{
	public class PhysicsCubeUseState : UseState
	{
		public NetworkObjectReference CubeObjectReference;

		public int verticalInput;

		public PhysicsCubeController.PushState pushState;

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			serializer.SerializeValue(ref CubeObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref verticalInput, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref pushState, default(FastBufferWriter.ForEnums));
		}
	}

	[SerializeField]
	private float charRotationSpeed = 450f;

	[SerializeField]
	private float characterPositionFixSpeed = 7.4f;

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		WorldUsableInteractable worldUsableInteractable = playerReference.PlayerInteractor.MostRecentInteraction as WorldUsableInteractable;
		WorldUsableController worldUsableController = (worldUsableInteractable ? worldUsableInteractable.WorldUsableController : null);
		if (worldUsableController == null)
		{
			Debug.LogWarning("Invalid interaction.");
			return null;
		}
		return new PhysicsCubeUseState
		{
			CubeObjectReference = worldUsableController.NetworkObject,
			EquipmentGuid = guid
		};
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		PhysicsCubeUseState physicsCubeUseState = (PhysicsCubeUseState)equipmentUseState;
		NetworkObject networkObject = physicsCubeUseState.CubeObjectReference;
		if (networkObject == null)
		{
			return false;
		}
		PhysicsCubeController componentInChildren = networkObject.GetComponentInChildren<PhysicsCubeController>();
		if (!componentInChildren.IsInteractingWith(playerReference.PlayerInteractor))
		{
			return false;
		}
		if (!state.Grounded || state.Downed)
		{
			componentInChildren.CancelInteraction(playerReference.PlayerInteractor);
			return false;
		}
		state.BlockMotionXZ = true;
		state.BlockRotation = true;
		if (input.Vertical != 0)
		{
			if (input.Vertical == physicsCubeUseState.verticalInput && physicsCubeUseState.pushState != PhysicsCubeController.PushState.Idle)
			{
				componentInChildren.EquipmentInteractFrame(physicsCubeUseState.pushState);
			}
			else
			{
				physicsCubeUseState.pushState = componentInChildren.EquipmentInteractFrame(input.Vertical, input.MotionRotation);
			}
		}
		else
		{
			physicsCubeUseState.pushState = PhysicsCubeController.PushState.Idle;
			componentInChildren.EquipmentInteractFrame(PhysicsCubeController.PushState.Idle);
		}
		physicsCubeUseState.verticalInput = input.Vertical;
		Vector3 vector = componentInChildren.GetInteractorTargetPosition() - playerReference.transform.position;
		state.CalculatedMotion = new Vector3(vector.x, 0f, vector.z) * characterPositionFixSpeed;
		float pushDirectionFloat = componentInChildren.GetPushDirectionFloat();
		PlayerController.Rotate(ref state, pushDirectionFloat, charRotationSpeed);
		return true;
	}

	public override UseState GetNewUseState()
	{
		return new PhysicsCubeUseState();
	}
}
