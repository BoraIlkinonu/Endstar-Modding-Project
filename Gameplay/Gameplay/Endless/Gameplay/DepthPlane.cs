using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000303 RID: 771
	public class DepthPlane : EndlessNetworkBehaviour, IGameEndSubscriber, IUpdateSubscriber, IPersistantStateSubscriber, IStartSubscriber, IBaseType, IComponentBase
	{
		// Token: 0x17000386 RID: 902
		// (get) Token: 0x060011BA RID: 4538 RVA: 0x00057E5D File Offset: 0x0005605D
		// (set) Token: 0x060011BB RID: 4539 RVA: 0x00057E65 File Offset: 0x00056065
		public DepthPlane.DepthPlaneType PlaneType
		{
			get
			{
				return this.planeType;
			}
			set
			{
				if (this.planeType != value)
				{
					this.SetPlaneActive(false);
					this.planeType = value;
					this.SetPlaneActive(true);
				}
			}
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x060011BC RID: 4540 RVA: 0x00057E85 File Offset: 0x00056085
		public bool OverrideFallOffHeight
		{
			get
			{
				return this.PlaneType > DepthPlane.DepthPlaneType.Empty;
			}
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x060011BD RID: 4541 RVA: 0x00017586 File Offset: 0x00015786
		public bool ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x00057E90 File Offset: 0x00056090
		private void SetPlaneActive(bool value)
		{
			DepthPlane.DepthPlaneInfo planeInfo = this.GetPlaneInfo();
			if (planeInfo != null)
			{
				planeInfo.PlaneObject.SetActive(value);
				planeInfo.DeeperPlane.SetActive(value);
			}
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x00057EC0 File Offset: 0x000560C0
		private DepthPlane.DepthPlaneInfo GetPlaneInfo()
		{
			foreach (DepthPlane.DepthPlaneInfo depthPlaneInfo in this.visuals)
			{
				if (depthPlaneInfo.PlaneType == this.planeType)
				{
					return depthPlaneInfo;
				}
			}
			return null;
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00057F24 File Offset: 0x00056124
		internal float GetFallOffHeight()
		{
			DepthPlane.DepthPlaneInfo planeInfo = this.GetPlaneInfo();
			return this.planeParent.transform.position.y + planeInfo.KillOffset;
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x00057F54 File Offset: 0x00056154
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				this.runtimeMovementSpeed.Value = this.moveSpeed;
			}
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x00057F78 File Offset: 0x00056178
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<StageManager>.Instance)
			{
				MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(new UnityAction<Stage>(this.HandleStageReady));
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.UnregisterDepthPlane(this);
				}
			}
		}

		// Token: 0x060011C3 RID: 4547 RVA: 0x00057FD3 File Offset: 0x000561D3
		public void EndlessStart()
		{
			if (base.IsServer)
			{
				this.runtimeMovementSpeed.Value = this.moveSpeed;
			}
		}

		// Token: 0x060011C4 RID: 4548 RVA: 0x00057FF0 File Offset: 0x000561F0
		protected override void Start()
		{
			base.Start();
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty)
			{
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RegisterDepthPlane(this);
					return;
				}
				MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(new UnityAction<Stage>(this.HandleStageReady));
			}
		}

		// Token: 0x060011C5 RID: 4549 RVA: 0x0005806C File Offset: 0x0005626C
		private void HandleStageReady(Stage stage)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty)
			{
				stage.RegisterDepthPlane(this);
			}
		}

		// Token: 0x060011C6 RID: 4550 RVA: 0x000580A0 File Offset: 0x000562A0
		public void EndlessGameEnd()
		{
			if (base.IsServer)
			{
				this.desiredOffset.Value = 0f;
			}
			this.offsetTransform.localPosition = global::UnityEngine.Vector3.zero;
		}

		// Token: 0x060011C7 RID: 4551 RVA: 0x000580CA File Offset: 0x000562CA
		public void SetMoveSpeed(Context context, float newSpeed)
		{
			if (base.IsServer)
			{
				this.runtimeMovementSpeed.Value = Mathf.Max(0f, newSpeed);
			}
		}

		// Token: 0x060011C8 RID: 4552 RVA: 0x000580EA File Offset: 0x000562EA
		public void ModifyMoveSpeed(Context context, float speedDelta)
		{
			if (base.IsServer)
			{
				this.runtimeMovementSpeed.Value = Mathf.Max(0f, this.runtimeMovementSpeed.Value + speedDelta);
			}
		}

		// Token: 0x060011C9 RID: 4553 RVA: 0x00058116 File Offset: 0x00056316
		public void SetHeight(Context context, float newHeight)
		{
			if (base.IsServer)
			{
				this.desiredOffset.Value = newHeight;
			}
		}

		// Token: 0x060011CA RID: 4554 RVA: 0x0005812C File Offset: 0x0005632C
		public void ModifyHeight(Context context, float delta)
		{
			if (base.IsServer)
			{
				this.desiredOffset.Value += delta;
			}
		}

		// Token: 0x060011CB RID: 4555 RVA: 0x0005814C File Offset: 0x0005634C
		public void EndlessUpdate()
		{
			global::UnityEngine.Vector3 localPosition = this.offsetTransform.localPosition;
			if (!Mathf.Approximately(localPosition.y, this.desiredOffset.Value))
			{
				if (Mathf.Sign(this.desiredOffset.Value - localPosition.y) > 0f)
				{
					localPosition.y = Mathf.Min(this.desiredOffset.Value, localPosition.y + this.runtimeMovementSpeed.Value * Time.deltaTime);
				}
				else
				{
					localPosition.y = Mathf.Max(this.desiredOffset.Value, localPosition.y - this.runtimeMovementSpeed.Value * Time.deltaTime);
				}
				this.offsetTransform.localPosition = localPosition;
				if (base.IsServer && Mathf.Approximately(localPosition.y, this.desiredOffset.Value))
				{
					this.OnWaterLevelReached.Invoke(this.Context);
				}
			}
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x0005823C File Offset: 0x0005643C
		private void LateUpdate()
		{
			DepthPlane.DepthPlaneInfo planeInfo = this.GetPlaneInfo();
			if (planeInfo != null)
			{
				float num = Mathf.Min(planeInfo.PlaneObject.transform.position.y - 5f, Camera.main.transform.position.y - 5f);
				planeInfo.DeeperPlane.transform.position = new global::UnityEngine.Vector3(planeInfo.PlaneObject.transform.position.x, num, planeInfo.PlaneObject.transform.position.z);
			}
		}

		// Token: 0x060011CD RID: 4557 RVA: 0x000582CE File Offset: 0x000564CE
		public object GetSaveState()
		{
			return new ValueTuple<float, float>(this.offsetTransform.localPosition.y, this.desiredOffset.Value);
		}

		// Token: 0x060011CE RID: 4558 RVA: 0x000582F8 File Offset: 0x000564F8
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				ValueTuple<float, float> valueTuple = (ValueTuple<float, float>)loadedState;
				this.desiredOffset.Value = valueTuple.Item2;
				this.ForceSyncFloorPosition_ClientRpc(valueTuple.Item1);
				if (!base.IsClient)
				{
					this.SetCurrentOffset(valueTuple.Item1);
				}
			}
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x00058348 File Offset: 0x00056548
		[ClientRpc]
		private void ForceSyncFloorPosition_ClientRpc(float newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1179626712U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in newValue, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 1179626712U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.SetCurrentOffset(newValue);
		}

		// Token: 0x060011D0 RID: 4560 RVA: 0x0005843D File Offset: 0x0005663D
		private void SetCurrentOffset(float newValue)
		{
			this.offsetTransform.localPosition = new global::UnityEngine.Vector3(this.offsetTransform.localPosition.x, newValue, this.offsetTransform.localPosition.z);
		}

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x060011D1 RID: 4561 RVA: 0x00058470 File Offset: 0x00056670
		// (set) Token: 0x060011D2 RID: 4562 RVA: 0x00058478 File Offset: 0x00056678
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x060011D3 RID: 4563 RVA: 0x00058484 File Offset: 0x00056684
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x060011D4 RID: 4564 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x060011D5 RID: 4565 RVA: 0x000584AF File Offset: 0x000566AF
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x060011D7 RID: 4567 RVA: 0x00058518 File Offset: 0x00056718
		protected override void __initializeVariables()
		{
			bool flag = this.desiredOffset == null;
			if (flag)
			{
				throw new Exception("DepthPlane.desiredOffset cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.desiredOffset.Initialize(this);
			base.__nameNetworkVariable(this.desiredOffset, "desiredOffset");
			this.NetworkVariableFields.Add(this.desiredOffset);
			flag = this.runtimeMovementSpeed == null;
			if (flag)
			{
				throw new Exception("DepthPlane.runtimeMovementSpeed cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.runtimeMovementSpeed.Initialize(this);
			base.__nameNetworkVariable(this.runtimeMovementSpeed, "runtimeMovementSpeed");
			this.NetworkVariableFields.Add(this.runtimeMovementSpeed);
			base.__initializeVariables();
		}

		// Token: 0x060011D8 RID: 4568 RVA: 0x000585C8 File Offset: 0x000567C8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1179626712U, new NetworkBehaviour.RpcReceiveHandler(DepthPlane.__rpc_handler_1179626712), "ForceSyncFloorPosition_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060011D9 RID: 4569 RVA: 0x000585F0 File Offset: 0x000567F0
		private static void __rpc_handler_1179626712(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((DepthPlane)target).ForceSyncFloorPosition_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060011DA RID: 4570 RVA: 0x00058660 File Offset: 0x00056860
		protected internal override string __getTypeName()
		{
			return "DepthPlane";
		}

		// Token: 0x04000F25 RID: 3877
		[SerializeField]
		private List<DepthPlane.DepthPlaneInfo> visuals = new List<DepthPlane.DepthPlaneInfo>();

		// Token: 0x04000F26 RID: 3878
		[SerializeField]
		private Transform planeParent;

		// Token: 0x04000F27 RID: 3879
		[SerializeField]
		private Transform offsetTransform;

		// Token: 0x04000F28 RID: 3880
		private NetworkVariable<float> desiredOffset = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F29 RID: 3881
		private NetworkVariable<float> runtimeMovementSpeed = new NetworkVariable<float>(0.1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000F2A RID: 3882
		[SerializeField]
		private float moveSpeed = 0.1f;

		// Token: 0x04000F2B RID: 3883
		[HideInInspector]
		public EndlessEvent OnWaterLevelReached = new EndlessEvent();

		// Token: 0x04000F2C RID: 3884
		private DepthPlane.DepthPlaneType planeType = DepthPlane.DepthPlaneType.Ocean;

		// Token: 0x04000F2D RID: 3885
		private Context context;

		// Token: 0x02000304 RID: 772
		public enum DepthPlaneType
		{
			// Token: 0x04000F30 RID: 3888
			Empty,
			// Token: 0x04000F31 RID: 3889
			Ocean
		}

		// Token: 0x02000305 RID: 773
		[Serializable]
		private class DepthPlaneInfo
		{
			// Token: 0x04000F32 RID: 3890
			public DepthPlane.DepthPlaneType PlaneType;

			// Token: 0x04000F33 RID: 3891
			public float KillOffset;

			// Token: 0x04000F34 RID: 3892
			public GameObject PlaneObject;

			// Token: 0x04000F35 RID: 3893
			public GameObject DeeperPlane;
		}
	}
}
