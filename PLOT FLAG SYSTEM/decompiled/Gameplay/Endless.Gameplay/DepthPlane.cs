using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class DepthPlane : EndlessNetworkBehaviour, IGameEndSubscriber, IUpdateSubscriber, IPersistantStateSubscriber, IStartSubscriber, IBaseType, IComponentBase
{
	public enum DepthPlaneType
	{
		Empty,
		Ocean
	}

	[Serializable]
	private class DepthPlaneInfo
	{
		public DepthPlaneType PlaneType;

		public float KillOffset;

		public GameObject PlaneObject;

		public GameObject DeeperPlane;
	}

	[SerializeField]
	private List<DepthPlaneInfo> visuals = new List<DepthPlaneInfo>();

	[SerializeField]
	private Transform planeParent;

	[SerializeField]
	private Transform offsetTransform;

	private NetworkVariable<float> desiredOffset = new NetworkVariable<float>(0f);

	private NetworkVariable<float> runtimeMovementSpeed = new NetworkVariable<float>(0.1f);

	[SerializeField]
	private float moveSpeed = 0.1f;

	[HideInInspector]
	public EndlessEvent OnWaterLevelReached = new EndlessEvent();

	private DepthPlaneType planeType = DepthPlaneType.Ocean;

	private Context context;

	public DepthPlaneType PlaneType
	{
		get
		{
			return planeType;
		}
		set
		{
			if (planeType != value)
			{
				SetPlaneActive(value: false);
				planeType = value;
				SetPlaneActive(value: true);
			}
		}
	}

	public bool OverrideFallOffHeight => PlaneType != DepthPlaneType.Empty;

	public bool ShouldSaveAndLoad => true;

	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public NavType NavValue => NavType.Intangible;

	private void SetPlaneActive(bool value)
	{
		DepthPlaneInfo planeInfo = GetPlaneInfo();
		if (planeInfo != null)
		{
			planeInfo.PlaneObject.SetActive(value);
			planeInfo.DeeperPlane.SetActive(value);
		}
	}

	private DepthPlaneInfo GetPlaneInfo()
	{
		foreach (DepthPlaneInfo visual in visuals)
		{
			if (visual.PlaneType == planeType)
			{
				return visual;
			}
		}
		return null;
	}

	internal float GetFallOffHeight()
	{
		DepthPlaneInfo planeInfo = GetPlaneInfo();
		return planeParent.transform.position.y + planeInfo.KillOffset;
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			runtimeMovementSpeed.Value = moveSpeed;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance)
		{
			MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(HandleStageReady);
			if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.UnregisterDepthPlane(this);
			}
		}
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			runtimeMovementSpeed.Value = moveSpeed;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RegisterDepthPlane(this);
			}
			else
			{
				MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(HandleStageReady);
			}
		}
	}

	private void HandleStageReady(Stage stage)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty)
		{
			stage.RegisterDepthPlane(this);
		}
	}

	public void EndlessGameEnd()
	{
		if (base.IsServer)
		{
			desiredOffset.Value = 0f;
		}
		offsetTransform.localPosition = UnityEngine.Vector3.zero;
	}

	public void SetMoveSpeed(Context context, float newSpeed)
	{
		if (base.IsServer)
		{
			runtimeMovementSpeed.Value = Mathf.Max(0f, newSpeed);
		}
	}

	public void ModifyMoveSpeed(Context context, float speedDelta)
	{
		if (base.IsServer)
		{
			runtimeMovementSpeed.Value = Mathf.Max(0f, runtimeMovementSpeed.Value + speedDelta);
		}
	}

	public void SetHeight(Context context, float newHeight)
	{
		if (base.IsServer)
		{
			desiredOffset.Value = newHeight;
		}
	}

	public void ModifyHeight(Context context, float delta)
	{
		if (base.IsServer)
		{
			desiredOffset.Value += delta;
		}
	}

	public void EndlessUpdate()
	{
		UnityEngine.Vector3 localPosition = offsetTransform.localPosition;
		if (!Mathf.Approximately(localPosition.y, desiredOffset.Value))
		{
			if (Mathf.Sign(desiredOffset.Value - localPosition.y) > 0f)
			{
				localPosition.y = Mathf.Min(desiredOffset.Value, localPosition.y + runtimeMovementSpeed.Value * Time.deltaTime);
			}
			else
			{
				localPosition.y = Mathf.Max(desiredOffset.Value, localPosition.y - runtimeMovementSpeed.Value * Time.deltaTime);
			}
			offsetTransform.localPosition = localPosition;
			if (base.IsServer && Mathf.Approximately(localPosition.y, desiredOffset.Value))
			{
				OnWaterLevelReached.Invoke(Context);
			}
		}
	}

	private void LateUpdate()
	{
		DepthPlaneInfo planeInfo = GetPlaneInfo();
		if (planeInfo != null)
		{
			float y = Mathf.Min(planeInfo.PlaneObject.transform.position.y - 5f, Camera.main.transform.position.y - 5f);
			planeInfo.DeeperPlane.transform.position = new UnityEngine.Vector3(planeInfo.PlaneObject.transform.position.x, y, planeInfo.PlaneObject.transform.position.z);
		}
	}

	public object GetSaveState()
	{
		return (offsetTransform.localPosition.y, desiredOffset.Value);
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			(float, float) tuple = ((float, float))loadedState;
			desiredOffset.Value = tuple.Item2;
			ForceSyncFloorPosition_ClientRpc(tuple.Item1);
			if (!base.IsClient)
			{
				SetCurrentOffset(tuple.Item1);
			}
		}
	}

	[ClientRpc]
	private void ForceSyncFloorPosition_ClientRpc(float newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1179626712u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in newValue, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 1179626712u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				SetCurrentOffset(newValue);
			}
		}
	}

	private void SetCurrentOffset(float newValue)
	{
		offsetTransform.localPosition = new UnityEngine.Vector3(offsetTransform.localPosition.x, newValue, offsetTransform.localPosition.z);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	protected override void __initializeVariables()
	{
		if (desiredOffset == null)
		{
			throw new Exception("DepthPlane.desiredOffset cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		desiredOffset.Initialize(this);
		__nameNetworkVariable(desiredOffset, "desiredOffset");
		NetworkVariableFields.Add(desiredOffset);
		if (runtimeMovementSpeed == null)
		{
			throw new Exception("DepthPlane.runtimeMovementSpeed cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		runtimeMovementSpeed.Initialize(this);
		__nameNetworkVariable(runtimeMovementSpeed, "runtimeMovementSpeed");
		NetworkVariableFields.Add(runtimeMovementSpeed);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1179626712u, __rpc_handler_1179626712, "ForceSyncFloorPosition_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1179626712(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((DepthPlane)target).ForceSyncFloorPosition_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "DepthPlane";
	}
}
