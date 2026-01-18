using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class NetworkRigidbodyAppearanceController : EndlessBehaviour, IStartSubscriber
{
	protected InterpolationRingBuffer<RigidbodyState> stateRingBuffer = new InterpolationRingBuffer<RigidbodyState>(30);

	[SerializeField]
	protected GameObject appearance;

	[SerializeField]
	protected bool startVisible;

	private uint startFrame;

	private bool visible;

	private bool teleporting;

	private TeleportType activeTeleportType;

	public WorldObject WorldObject { get; protected set; }

	public void InitAppearance(WorldObject worldObject, GameObject appearanceObject)
	{
		WorldObject = worldObject;
		appearance = appearanceObject;
		appearanceObject.transform.SetParent(base.transform);
		appearanceObject.transform.localEulerAngles = Vector3.zero;
		appearanceObject.transform.localPosition = Vector3.zero;
	}

	void IStartSubscriber.EndlessStart()
	{
		if (!startVisible && !visible)
		{
			appearance.SetActive(value: false);
		}
		else
		{
			visible = true;
		}
	}

	public void AddState(RigidbodyState state)
	{
		if (startFrame == 0)
		{
			startFrame = state.NetFrame;
		}
		stateRingBuffer.UpdateValue(ref state);
	}

	private void Update()
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			return;
		}
		stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
		if ((bool)appearance && !visible && startFrame != 0 && stateRingBuffer.PastInterpolationState.NetFrame >= startFrame)
		{
			appearance.SetActive(value: true);
			visible = true;
		}
		if (stateRingBuffer.PastInterpolationState.Teleporting && !stateRingBuffer.NextInterpolationState.Teleporting)
		{
			base.transform.position = stateRingBuffer.NextInterpolationState.Position;
		}
		else
		{
			base.transform.position = Vector3.Lerp(stateRingBuffer.PastInterpolationState.Position, stateRingBuffer.NextInterpolationState.Position, stateRingBuffer.ActiveStateLerpTime);
		}
		base.transform.rotation = Quaternion.Lerp(Quaternion.Euler(stateRingBuffer.PastInterpolationState.Angles), Quaternion.Euler(stateRingBuffer.NextInterpolationState.Angles), stateRingBuffer.ActiveStateLerpTime);
		if (teleporting && !stateRingBuffer.NextInterpolationState.Teleporting)
		{
			teleporting = false;
			RuntimeDatabase.GetTeleportInfo(activeTeleportType).TeleportEnd(WorldObject.EndlessVisuals, null, base.transform.position);
		}
		else if (!teleporting && stateRingBuffer.NextInterpolationState.Teleporting)
		{
			teleporting = true;
			activeTeleportType = stateRingBuffer.NextInterpolationState.TeleportType;
			RuntimeDatabase.GetTeleportInfo(activeTeleportType).TeleportStart(WorldObject.EndlessVisuals, null, base.transform.position);
		}
		AfterUpdate();
	}

	protected virtual void AfterUpdate()
	{
	}
}
