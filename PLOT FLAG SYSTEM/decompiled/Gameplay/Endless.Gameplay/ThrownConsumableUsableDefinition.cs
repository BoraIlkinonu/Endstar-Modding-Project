using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ThrownConsumableUsableDefinition : ConsumableUsableDefinition
{
	private struct ThrowInfo
	{
		public Vector3 position;

		public Vector3 direction;

		public float forceMultiplier;
	}

	public class ThrownConsumableEquipmentUseState : UseState
	{
		public uint ThrowCooldownFrames;

		public bool ADS;

		public uint HoldFrames;

		public bool ThrownThisFrame;

		public override void Serialize<T>(BufferSerializer<T> serializer)
		{
			base.Serialize(serializer);
			if (serializer.IsWriter)
			{
				Compression.SerializeUInt(serializer, ThrowCooldownFrames);
				Compression.SerializeUInt(serializer, HoldFrames);
				Compression.SerializeBoolsToByte(serializer, ADS, ThrownThisFrame);
			}
			else
			{
				ThrowCooldownFrames = Compression.DeserializeUInt(serializer);
				HoldFrames = Compression.DeserializeUInt(serializer);
				Compression.DeserializeBoolsFromByte(serializer, ref ADS, ref ThrownThisFrame);
			}
		}
	}

	[SerializeField]
	private NetworkRigidbodyController thrownObjectPrefab;

	[SerializeField]
	private uint throwCooldownFrames = 30u;

	[SerializeField]
	private uint adsRotationSpeed;

	[Header("Quick throw")]
	[SerializeField]
	private float quickThrownForce;

	[SerializeField]
	private float quickThrowPitchShift;

	[Header("ADS throw")]
	[SerializeField]
	private float adsThrownForce;

	[SerializeField]
	private float adsThrowPitchShift;

	[SerializeField]
	private AnimationCurve adsThrownForceAngleCurve;

	[SerializeField]
	private float throwPositionVerticalOffset;

	[SerializeField]
	private float throwPositionForwardOffset;

	[Header("ADS timing")]
	[SerializeField]
	private uint adsStartFrame = 6u;

	[SerializeField]
	private uint adsKeepFrames = 6u;

	public override UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item)
	{
		ThrownConsumableEquipmentUseState obj = UseState.GetStateFromPool(guid, item) as ThrownConsumableEquipmentUseState;
		obj.HoldFrames = 0u;
		obj.ThrowCooldownFrames = 0u;
		obj.ADS = false;
		obj.ThrownThisFrame = false;
		return obj;
	}

	public override bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState equipmentUseState, PlayerReferenceManager playerReference, bool equipped, bool pressed)
	{
		if (state.IsStunned || !equipped)
		{
			return false;
		}
		ThrownConsumableEquipmentUseState thrownConsumableEquipmentUseState = equipmentUseState as ThrownConsumableEquipmentUseState;
		if (thrownConsumableEquipmentUseState.ThrownThisFrame && NetworkManager.Singleton.IsServer)
		{
			StackableItem stackableItem = (StackableItem)equipmentUseState.Item;
			if ((bool)stackableItem)
			{
				stackableItem.ChangeStackCount(-1);
			}
		}
		thrownConsumableEquipmentUseState.ThrownThisFrame = false;
		if (thrownConsumableEquipmentUseState.ThrowCooldownFrames != 0)
		{
			thrownConsumableEquipmentUseState.ThrowCooldownFrames--;
		}
		if (pressed)
		{
			thrownConsumableEquipmentUseState.HoldFrames++;
			thrownConsumableEquipmentUseState.ADS = thrownConsumableEquipmentUseState.ADS || thrownConsumableEquipmentUseState.HoldFrames >= adsStartFrame;
			state.AimState = (thrownConsumableEquipmentUseState.ADS ? CameraController.CameraType.FullADS : CameraController.CameraType.Normal);
			if (thrownConsumableEquipmentUseState.ADS)
			{
				PlayerController.Rotate(ref state, input.MotionRotation, adsRotationSpeed);
				state.BlockRotation = true;
			}
			state.BlockItemInput = true;
		}
		else if (thrownConsumableEquipmentUseState.HoldFrames != 0 && thrownConsumableEquipmentUseState.ThrowCooldownFrames == 0)
		{
			state.AimState = (thrownConsumableEquipmentUseState.ADS ? CameraController.CameraType.FullADS : CameraController.CameraType.Normal);
			thrownConsumableEquipmentUseState.HoldFrames = 0u;
			thrownConsumableEquipmentUseState.ThrowCooldownFrames = throwCooldownFrames;
			thrownConsumableEquipmentUseState.ThrownThisFrame = true;
			if (NetworkManager.Singleton.IsServer)
			{
				ThrowInfo throwInfo = GetThrowInfo(state, input);
				NetworkRigidbodyController networkRigidbodyController = Object.Instantiate(thrownObjectPrefab, throwInfo.position, Quaternion.identity);
				networkRigidbodyController.NetworkObject.Spawn();
				networkRigidbodyController.GetComponent<IThrowable>()?.InitiateThrow(thrownConsumableEquipmentUseState.ADS ? (adsThrownForce * throwInfo.forceMultiplier) : quickThrownForce, throwInfo.direction, input.NetFrame, playerReference.NetworkObject, equipmentUseState.Item);
			}
			thrownConsumableEquipmentUseState.ThrowCooldownFrames = throwCooldownFrames;
		}
		if (thrownConsumableEquipmentUseState.HoldFrames == 0)
		{
			if (thrownConsumableEquipmentUseState.ADS && thrownConsumableEquipmentUseState.ThrowCooldownFrames < throwCooldownFrames - adsKeepFrames)
			{
				thrownConsumableEquipmentUseState.ADS = true;
				state.AimState = CameraController.CameraType.FullADS;
			}
			else
			{
				thrownConsumableEquipmentUseState.ADS = false;
				state.AimState = CameraController.CameraType.Normal;
			}
		}
		if (playerReference.IsOwner)
		{
			if (thrownConsumableEquipmentUseState.ADS && pressed)
			{
				MonoBehaviourSingleton<TrajectoryDrawer>.Instance.Enabled = true;
				ThrowInfo throwInfo2 = GetThrowInfo(state, input);
				Vector3 force = throwInfo2.direction * adsThrownForce * throwInfo2.forceMultiplier;
				MonoBehaviourSingleton<TrajectoryDrawer>.Instance.SetTrajectoryDrawerInfo(thrownObjectPrefab.GetComponent<Rigidbody>(), throwInfo2.position, force, thrownConsumableEquipmentUseState.ThrowCooldownFrames != 0, state.NetFrame);
			}
			else
			{
				MonoBehaviourSingleton<TrajectoryDrawer>.Instance.Enabled = false;
			}
		}
		if (!pressed && thrownConsumableEquipmentUseState.ThrowCooldownFrames == 0 && thrownConsumableEquipmentUseState.HoldFrames == 0)
		{
			return false;
		}
		return true;
	}

	private ThrowInfo GetThrowInfo(NetState state, NetInput input)
	{
		float num = input.AimPitch + adsThrowPitchShift;
		bool num2 = state.AimState != CameraController.CameraType.Normal;
		Vector3 direction = Quaternion.Euler(num2 ? new Vector3(num, input.AimYaw, 0f) : new Vector3(quickThrowPitchShift, input.MotionRotation, 0f)) * Vector3.forward;
		float forceMultiplier = 1f;
		if (num2)
		{
			float time = Mathf.Abs(num - -40f);
			forceMultiplier = adsThrownForceAngleCurve.Evaluate(time);
		}
		Vector3 position = new Vector3(0f, throwPositionVerticalOffset, 0f) + state.Position;
		return new ThrowInfo
		{
			position = position,
			direction = direction,
			forceMultiplier = forceMultiplier
		};
	}

	public override void GetEventData(NetState state, UseState eus, double appearanceTime, ref EventData data)
	{
		base.GetEventData(state, eus, appearanceTime, ref data);
		if (eus != null)
		{
			_ = (ThrownConsumableEquipmentUseState)eus;
			data.Available = false;
			data.InUse = true;
		}
		else
		{
			data.Available = true;
		}
	}

	public override EquipmentShowPriority GetShowPriority(UseState eus)
	{
		if (eus != null)
		{
			return EquipmentShowPriority.MinorInUse;
		}
		return EquipmentShowPriority.MinorOutOfUse;
	}

	public override UseState GetNewUseState()
	{
		return new ThrownConsumableEquipmentUseState();
	}

	public override string GetAnimationTrigger(UseState eus, uint currentVisualFrame)
	{
		if ((eus as ThrownConsumableEquipmentUseState).ThrownThisFrame)
		{
			return "UseTool";
		}
		return string.Empty;
	}
}
