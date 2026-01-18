using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002EE RID: 750
	public class PhysicsCubeUsableDefinition : WorldUsableDefinition
	{
		// Token: 0x060010DB RID: 4315 RVA: 0x0005518C File Offset: 0x0005338C
		public override UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
		{
			WorldUsableInteractable worldUsableInteractable = playerReference.PlayerInteractor.MostRecentInteraction as WorldUsableInteractable;
			WorldUsableController worldUsableController = (worldUsableInteractable ? worldUsableInteractable.WorldUsableController : null);
			if (worldUsableController == null)
			{
				Debug.LogWarning("Invalid interaction.");
				return null;
			}
			return new PhysicsCubeUsableDefinition.PhysicsCubeUseState
			{
				CubeObjectReference = worldUsableController.NetworkObject,
				EquipmentGuid = guid
			};
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x000551F0 File Offset: 0x000533F0
		public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
		{
			PhysicsCubeUsableDefinition.PhysicsCubeUseState physicsCubeUseState = (PhysicsCubeUsableDefinition.PhysicsCubeUseState)equipmentUseState;
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
			state.CalculatedMotion = new Vector3(vector.x, 0f, vector.z) * this.characterPositionFixSpeed;
			float pushDirectionFloat = componentInChildren.GetPushDirectionFloat();
			PlayerController.Rotate(ref state, pushDirectionFloat, this.charRotationSpeed, false);
			return true;
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00055319 File Offset: 0x00053519
		public override UsableDefinition.UseState GetNewUseState()
		{
			return new PhysicsCubeUsableDefinition.PhysicsCubeUseState();
		}

		// Token: 0x04000E9B RID: 3739
		[SerializeField]
		private float charRotationSpeed = 450f;

		// Token: 0x04000E9C RID: 3740
		[SerializeField]
		private float characterPositionFixSpeed = 7.4f;

		// Token: 0x020002EF RID: 751
		public class PhysicsCubeUseState : UsableDefinition.UseState
		{
			// Token: 0x060010DF RID: 4319 RVA: 0x00055340 File Offset: 0x00053540
			public override void Serialize<T>(BufferSerializer<T> serializer)
			{
				serializer.SerializeValue<NetworkObjectReference>(ref this.CubeObjectReference, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<int>(ref this.verticalInput, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<PhysicsCubeController.PushState>(ref this.pushState, default(FastBufferWriter.ForEnums));
			}

			// Token: 0x04000E9D RID: 3741
			public NetworkObjectReference CubeObjectReference;

			// Token: 0x04000E9E RID: 3742
			public int verticalInput;

			// Token: 0x04000E9F RID: 3743
			public PhysicsCubeController.PushState pushState;
		}
	}
}
