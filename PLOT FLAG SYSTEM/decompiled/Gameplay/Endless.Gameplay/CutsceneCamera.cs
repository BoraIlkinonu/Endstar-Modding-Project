using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class CutsceneCamera : EndlessNetworkBehaviour, IBaseType, IComponentBase, IScriptInjector
{
	public struct TargetInfo : INetworkSerializable
	{
		private bool hasTarget;

		private ulong networkId;

		private SerializableGuid instanceId;

		public WorldObject TargetObject { get; private set; }

		public bool HasTarget => hasTarget;

		public TargetInfo(WorldObject worldObject)
		{
			if (worldObject == null)
			{
				TargetObject = null;
				hasTarget = false;
				networkId = 0uL;
				instanceId = SerializableGuid.Empty;
				return;
			}
			TargetObject = worldObject;
			hasTarget = true;
			if (worldObject.NetworkObject != null)
			{
				networkId = worldObject.NetworkObject.NetworkObjectId;
				instanceId = SerializableGuid.Empty;
			}
			else
			{
				networkId = 0uL;
				instanceId = worldObject.InstanceId;
			}
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref hasTarget, default(FastBufferWriter.ForPrimitives));
			if (!hasTarget)
			{
				return;
			}
			serializer.SerializeValue(ref networkId, default(FastBufferWriter.ForPrimitives));
			if (networkId == 0L)
			{
				serializer.SerializeValue(ref instanceId, default(FastBufferWriter.ForNetworkSerializable));
			}
			if (serializer.IsReader)
			{
				GameObject gameObject = null;
				if (networkId != 0L)
				{
					gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].gameObject;
				}
				else if (instanceId != SerializableGuid.Empty)
				{
					gameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(instanceId);
				}
				if (gameObject != null)
				{
					TargetObject = gameObject.GetComponent<WorldObject>();
				}
			}
		}
	}

	public struct MoveToInfo : INetworkSerializable
	{
		public bool HasValue;

		public UnityEngine.Vector3 MoveToPosition;

		public float MoveToPitch;

		public float MoveToYaw;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref HasValue, default(FastBufferWriter.ForPrimitives));
			if (HasValue)
			{
				serializer.SerializeValue(ref MoveToPosition);
				serializer.SerializeValue(ref MoveToPitch, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue(ref MoveToYaw, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	public struct TransitionInfo : INetworkSerializable
	{
		public CameraTransition Type;

		public float Duration;

		public TransitionInfo(CameraTransition type, float duration)
		{
			Type = type;
			Duration = Mathf.Max(0f, duration);
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Type, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref Duration, default(FastBufferWriter.ForPrimitives));
		}
	}

	public EndlessEvent OnShotStarted = new EndlessEvent();

	public EndlessEvent OnShotFinished = new EndlessEvent();

	public EndlessEvent OnShotInterrupted = new EndlessEvent();

	[SerializeField]
	private CinemachineCamera cinemachineCamera;

	[SerializeField]
	private CinemachineThirdPersonFollow cinemachineThirdPersonFollow;

	[SerializeField]
	private CinemachineRotationComposer cinemachineRotationComposer;

	[SerializeField]
	private InputSettings InputAllowedDuringShot;

	[SerializeField]
	private bool InvulnerablePlayerDuringShot;

	private NetworkVariable<float> pitch = new NetworkVariable<float>(0f);

	private NetworkVariable<float> yaw = new NetworkVariable<float>(0f);

	private NetworkVariable<float> fieldOfView = new NetworkVariable<float>(60f);

	private NetworkVariable<UnityEngine.Vector3> followOffset = new NetworkVariable<UnityEngine.Vector3>();

	private NetworkVariable<UnityEngine.Vector3> lookOffset = new NetworkVariable<UnityEngine.Vector3>();

	private NetworkVariable<float> followDampening = new NetworkVariable<float>(2f);

	private NetworkVariable<float> lookDampening = new NetworkVariable<float>(0f);

	public NetworkVariable<UnityEngine.Vector3> positionOffset = new NetworkVariable<UnityEngine.Vector3>();

	private float baseShotDuration;

	private float secondaryShotDuration;

	private bool locallyActive;

	private TargetInfo currentFollowInfo_Server;

	private TargetInfo currentLookAtInfo_Server;

	private MoveToInfo moveToInfo_server;

	private TargetInfo localUseFollowInfo;

	private TargetInfo localUseLookAtInfo;

	private MoveToInfo localUseMoveToInfo;

	private float localStartTime;

	private float localUseShotDuration;

	private Context context;

	private Endless.Gameplay.LuaInterfaces.Camera luaInterface;

	private EndlessScriptComponent scriptComponent;

	public InputSettings InputAllowed => InputAllowedDuringShot;

	public bool InvulnerablePlayer => InvulnerablePlayerDuringShot;

	public float BaseShotDuration => baseShotDuration;

	public float SecondaryShotDuration => secondaryShotDuration;

	public float TotalShotDuration => baseShotDuration + secondaryShotDuration;

	public TargetInfo CurrentFollowInfo_Server => currentFollowInfo_Server;

	public TargetInfo CurrentLookAtInfo_Server => currentLookAtInfo_Server;

	public TransitionInfo TransitionIn_Server { get; private set; } = new TransitionInfo(CameraTransition.Ease, 3f);

	public TransitionInfo TransitionOut_Server { get; private set; } = new TransitionInfo(CameraTransition.Ease, 3f);

	public MoveToInfo MoveToInfo_Server => moveToInfo_server;

	public CinemachineCamera CinemachineCamera => cinemachineCamera;

	public float Pitch
	{
		get
		{
			return pitch.Value;
		}
		set
		{
			if (base.IsServer)
			{
				pitch.Value = value;
			}
		}
	}

	public float Yaw
	{
		get
		{
			return yaw.Value;
		}
		set
		{
			if (base.IsServer)
			{
				yaw.Value = value;
			}
		}
	}

	public float FieldOfView
	{
		get
		{
			return fieldOfView.Value;
		}
		set
		{
			if (base.IsServer)
			{
				fieldOfView.Value = value;
			}
		}
	}

	public UnityEngine.Vector3 PositionOffset
	{
		get
		{
			return positionOffset.Value;
		}
		set
		{
			if (base.IsServer)
			{
				positionOffset.Value = value;
			}
		}
	}

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public ReferenceFilter Filter => ReferenceFilter.NonStatic;

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Endless.Gameplay.LuaInterfaces.Camera(this);
			}
			return luaInterface;
		}
	}

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.Camera);

	private new void Start()
	{
		base.Start();
		NetworkVariable<float> networkVariable = followDampening;
		networkVariable.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(HandleFollowDampeningChanged));
		NetworkVariable<float> networkVariable2 = lookDampening;
		networkVariable2.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(HandleLookDampeningChanged));
	}

	private void UpdatePreviewData()
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			CinemachineCamera.transform.localEulerAngles = new UnityEngine.Vector3(pitch.Value, yaw.Value, 0f);
			CinemachineCamera.Lens.FieldOfView = fieldOfView.Value;
			CinemachineCamera.transform.localPosition = PositionOffset;
		}
	}

	public void SetPitch(float value)
	{
		if (base.IsServer)
		{
			pitch.Value = 0f - value;
		}
	}

	public void SetYaw(float value)
	{
		if (base.IsServer)
		{
			yaw.Value = value;
		}
	}

	public void SetFieldOfView(float value)
	{
		if (base.IsServer)
		{
			fieldOfView.Value = value;
		}
	}

	public void SetPositionOffset(UnityEngine.Vector3 value)
	{
		if (base.IsServer)
		{
			positionOffset.Value = value;
		}
	}

	public void SetFollowTarget(Context target)
	{
		if (base.IsServer)
		{
			if (target != null)
			{
				currentFollowInfo_Server = new TargetInfo(target.WorldObject);
			}
			else
			{
				currentFollowInfo_Server = new TargetInfo(null);
			}
		}
	}

	public void SetLookAtTarget(Context target)
	{
		if (base.IsServer)
		{
			if (target != null)
			{
				currentLookAtInfo_Server = new TargetInfo(target.WorldObject);
			}
			else
			{
				currentLookAtInfo_Server = new TargetInfo(null);
			}
		}
	}

	public void SetFollowOffset(UnityEngine.Vector3 offset)
	{
		if (base.IsServer)
		{
			followOffset.Value = offset;
		}
	}

	public void SetLookOffset(UnityEngine.Vector3 offset)
	{
		if (base.IsServer)
		{
			lookOffset.Value = offset;
		}
	}

	public void SetFollowDampening(float value)
	{
		if (base.IsServer)
		{
			followDampening.Value = Mathf.Clamp(value, 0f, 20f);
		}
	}

	public void SetLookDampening(float value)
	{
		if (base.IsServer)
		{
			lookDampening.Value = Mathf.Clamp(value, 0f, 20f);
		}
	}

	public void SetDuration(float baseDuration, float secondaryDuration = 0f)
	{
		if (base.IsServer)
		{
			baseShotDuration = baseDuration;
			secondaryShotDuration = secondaryDuration;
		}
	}

	public void SetMoveToPosition(UnityEngine.Vector3 position)
	{
		if (base.IsServer)
		{
			moveToInfo_server.MoveToPosition = position;
			moveToInfo_server.HasValue = true;
		}
	}

	public void SetMoveToRotation(float pitch, float yaw)
	{
		if (base.IsServer)
		{
			moveToInfo_server.MoveToPitch = pitch;
			moveToInfo_server.MoveToYaw = yaw;
			moveToInfo_server.HasValue = true;
		}
	}

	public void SetTransitionIn(CameraTransition type, float duration)
	{
		if (base.IsServer)
		{
			TransitionIn_Server = new TransitionInfo(type, duration);
		}
	}

	public void SetTransitionOut(CameraTransition type, float duration)
	{
		if (base.IsServer)
		{
			TransitionOut_Server = new TransitionInfo(type, duration);
		}
	}

	public void JoinInProgress(CutsceneManager.InProgressState inProgressState)
	{
		localUseFollowInfo = inProgressState.FollowInfo;
		localUseMoveToInfo = inProgressState.MoveToInfo;
		localUseShotDuration = inProgressState.ShotDuration;
		float num = ((TransitionIn_Server.Type != CameraTransition.Cut) ? TransitionIn_Server.Duration : 0f);
		if (inProgressState.LateJoin)
		{
			localStartTime = num + Time.realtimeSinceStartup - (float)(base.NetworkManager.ServerTime.Time - inProgressState.StartTime);
		}
		else
		{
			localStartTime = num + Time.realtimeSinceStartup;
		}
	}

	public void EnabledLocally(TargetInfo followInfo, TargetInfo lookAtInfo)
	{
		locallyActive = true;
		localUseFollowInfo = followInfo;
		localUseLookAtInfo = lookAtInfo;
		InitializeShot();
	}

	public void DisabledLocally()
	{
		locallyActive = false;
	}

	private void InitializeShot()
	{
		CinemachineCamera.transform.localPosition = PositionOffset;
		CinemachineCamera.transform.localEulerAngles = new UnityEngine.Vector3(pitch.Value, yaw.Value, 0f);
		CinemachineCamera.Lens.FieldOfView = fieldOfView.Value;
		if (localUseFollowInfo.HasTarget)
		{
			Transform follow = ((!(localUseFollowInfo.TargetObject.BaseType is PlayerLuaComponent)) ? localUseFollowInfo.TargetObject.transform.GetChild(0) : localUseFollowInfo.TargetObject.GetComponent<PlayerReferenceManager>().ApperanceController.transform);
			cinemachineThirdPersonFollow.ShoulderOffset = followOffset.Value;
			CinemachineCamera.Follow = follow;
		}
		else
		{
			cinemachineThirdPersonFollow.ShoulderOffset = UnityEngine.Vector3.zero;
			CinemachineCamera.Follow = null;
		}
		if (localUseLookAtInfo.HasTarget)
		{
			Transform lookAt = ((!(localUseLookAtInfo.TargetObject.BaseType is PlayerLuaComponent)) ? localUseLookAtInfo.TargetObject.transform.GetChild(0) : localUseLookAtInfo.TargetObject.GetComponent<PlayerReferenceManager>().ApperanceController.transform);
			CinemachineCamera.LookAt = lookAt;
			cinemachineRotationComposer.TargetOffset = lookOffset.Value;
		}
		else
		{
			CinemachineCamera.LookAt = null;
			cinemachineRotationComposer.TargetOffset = UnityEngine.Vector3.zero;
		}
	}

	private void HandleFollowDampeningChanged(float previousValue, float newValue)
	{
		cinemachineThirdPersonFollow.Damping = new UnityEngine.Vector3(newValue, newValue, newValue);
	}

	private void HandleLookDampeningChanged(float previousValue, float newValue)
	{
		cinemachineRotationComposer.Damping.x = newValue;
		cinemachineRotationComposer.Damping.y = newValue;
	}

	private void Update()
	{
		if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
		{
			UpdatePreviewData();
		}
		if (locallyActive && !localUseFollowInfo.HasTarget && localUseMoveToInfo.HasValue)
		{
			float t = (Time.realtimeSinceStartup - localStartTime) / localUseShotDuration;
			UnityEngine.Vector3 position = UnityEngine.Vector3.Lerp(base.transform.position + PositionOffset, localUseMoveToInfo.MoveToPosition, t);
			Quaternion localRotation = Quaternion.Lerp(Quaternion.Euler(pitch.Value, yaw.Value, 0f), Quaternion.Euler(localUseMoveToInfo.MoveToPitch, localUseMoveToInfo.MoveToYaw, 0f), t);
			CinemachineCamera.transform.position = position;
			CinemachineCamera.transform.localRotation = localRotation;
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (pitch == null)
		{
			throw new Exception("CutsceneCamera.pitch cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		pitch.Initialize(this);
		__nameNetworkVariable(pitch, "pitch");
		NetworkVariableFields.Add(pitch);
		if (yaw == null)
		{
			throw new Exception("CutsceneCamera.yaw cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		yaw.Initialize(this);
		__nameNetworkVariable(yaw, "yaw");
		NetworkVariableFields.Add(yaw);
		if (fieldOfView == null)
		{
			throw new Exception("CutsceneCamera.fieldOfView cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		fieldOfView.Initialize(this);
		__nameNetworkVariable(fieldOfView, "fieldOfView");
		NetworkVariableFields.Add(fieldOfView);
		if (followOffset == null)
		{
			throw new Exception("CutsceneCamera.followOffset cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		followOffset.Initialize(this);
		__nameNetworkVariable(followOffset, "followOffset");
		NetworkVariableFields.Add(followOffset);
		if (lookOffset == null)
		{
			throw new Exception("CutsceneCamera.lookOffset cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		lookOffset.Initialize(this);
		__nameNetworkVariable(lookOffset, "lookOffset");
		NetworkVariableFields.Add(lookOffset);
		if (followDampening == null)
		{
			throw new Exception("CutsceneCamera.followDampening cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		followDampening.Initialize(this);
		__nameNetworkVariable(followDampening, "followDampening");
		NetworkVariableFields.Add(followDampening);
		if (lookDampening == null)
		{
			throw new Exception("CutsceneCamera.lookDampening cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		lookDampening.Initialize(this);
		__nameNetworkVariable(lookDampening, "lookDampening");
		NetworkVariableFields.Add(lookDampening);
		if (positionOffset == null)
		{
			throw new Exception("CutsceneCamera.positionOffset cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		positionOffset.Initialize(this);
		__nameNetworkVariable(positionOffset, "positionOffset");
		NetworkVariableFields.Add(positionOffset);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "CutsceneCamera";
	}
}
