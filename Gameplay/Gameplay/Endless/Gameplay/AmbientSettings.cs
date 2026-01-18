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
	// Token: 0x020002F3 RID: 755
	public class AmbientSettings : EndlessNetworkBehaviour, IRegisteredSubscriber, IBaseType, IComponentBase, IAwakeSubscriber, IPropPlacedSubscriber
	{
		// Token: 0x17000358 RID: 856
		// (get) Token: 0x060010F2 RID: 4338 RVA: 0x00055455 File Offset: 0x00053655
		private SerializableGuid InstanceId
		{
			get
			{
				if (this.instanceId == SerializableGuid.Empty)
				{
					this.instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject);
				}
				return this.instanceId;
			}
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x060010F3 RID: 4339 RVA: 0x00055494 File Offset: 0x00053694
		// (set) Token: 0x060010F4 RID: 4340 RVA: 0x000554D4 File Offset: 0x000536D4
		[EndlessNonSerialized]
		public bool IsDefault
		{
			get
			{
				return MonoBehaviourSingleton<StageManager>.Instance != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.IsDefaultEnvironment(this.InstanceId);
			}
			set
			{
				if (value)
				{
					if (this.activeSkybox.Value == (AmbientSettings.Skybox)(-1))
					{
						MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(this.InstanceId);
						this.CurrentSkybox = this.CurrentSkybox;
						return;
					}
				}
				else if (this.IsDefault)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ClearDefaultEnvironment();
					MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(null);
				}
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x060010F5 RID: 4341 RVA: 0x00055536 File Offset: 0x00053736
		// (set) Token: 0x060010F6 RID: 4342 RVA: 0x00055540 File Offset: 0x00053740
		public AmbientSettings.Skybox CurrentSkybox
		{
			get
			{
				return this.skybox;
			}
			set
			{
				this.skybox = value;
				if (this.activeSkybox.Value == (AmbientSettings.Skybox)(-1) && (this.IsDefault || MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying))
				{
					MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(this.AmbientEntryMap[this.skybox]);
				}
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x060010F7 RID: 4343 RVA: 0x00055594 File Offset: 0x00053794
		public Dictionary<AmbientSettings.Skybox, SimpleAmbientEntry> AmbientEntryMap
		{
			get
			{
				if (this.ambientEntryMap == null)
				{
					this.ambientEntryMap = new Dictionary<AmbientSettings.Skybox, SimpleAmbientEntry>();
					foreach (AmbientSettings.AmbientPair ambientPair in this.ambientPairs)
					{
						this.ambientEntryMap.Add(ambientPair.Skybox, ambientPair.AmbientEntry);
					}
				}
				return this.ambientEntryMap;
			}
		}

		// Token: 0x060010F8 RID: 4344 RVA: 0x00055610 File Offset: 0x00053810
		private void Awake()
		{
			foreach (AmbientSettings.AmbientPair ambientPair in this.ambientPairs)
			{
				ambientPair.Initialize();
				ambientPair.OnDeactivated.AddListener(new UnityAction<AmbientSettings.Skybox>(this.HandleAmbientEntryDeactivated));
			}
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x00055678 File Offset: 0x00053878
		private void HandleAmbientEntryDeactivated(AmbientSettings.Skybox deactivatedValue)
		{
			if (base.IsServer && this.activeSkybox.Value == deactivatedValue)
			{
				this.activeSkybox.Value = (AmbientSettings.Skybox)(-1);
			}
		}

		// Token: 0x060010FA RID: 4346 RVA: 0x0005569C File Offset: 0x0005389C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsClient)
			{
				if (this.activeSkybox.Value != (AmbientSettings.Skybox)(-1))
				{
					this.ApplySkybox(this.activeSkybox.Value);
				}
				NetworkVariable<AmbientSettings.Skybox> networkVariable = this.activeSkybox;
				networkVariable.OnValueChanged = (NetworkVariable<AmbientSettings.Skybox>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<AmbientSettings.Skybox>.OnValueChangedDelegate(this.HandleActiveSkyboxChanged));
			}
		}

		// Token: 0x060010FB RID: 4347 RVA: 0x000556FD File Offset: 0x000538FD
		private void HandleActiveSkyboxChanged(AmbientSettings.Skybox previousValue, AmbientSettings.Skybox newValue)
		{
			if (newValue != (AmbientSettings.Skybox)(-1))
			{
				this.ApplySkybox(newValue);
			}
		}

		// Token: 0x060010FC RID: 4348 RVA: 0x0005570A File Offset: 0x0005390A
		public void ChangeSkybox(Context context, AmbientSettings.Skybox newSkybox)
		{
			if (base.IsServer)
			{
				this.activeSkybox.Value = newSkybox;
			}
		}

		// Token: 0x060010FD RID: 4349 RVA: 0x00055720 File Offset: 0x00053920
		public void ApplySkybox(AmbientSettings.Skybox newSkybox)
		{
			MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(this.AmbientEntryMap[newSkybox]);
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x00055738 File Offset: 0x00053938
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<StageManager>.Instance && this.IsDefault)
			{
				if (MonoBehaviourSingleton<AmbientManager>.Instance)
				{
					MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(null);
				}
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ClearDefaultEnvironment();
			}
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x00055785 File Offset: 0x00053985
		public void EndlessRegistered()
		{
			if (base.IsClient && !MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && this.IsDefault && this.activeSkybox.Value == (AmbientSettings.Skybox)(-1))
			{
				this.ApplySkybox(this.skybox);
			}
		}

		// Token: 0x06001100 RID: 4352 RVA: 0x000557C0 File Offset: 0x000539C0
		[ClientRpc]
		private void SetDefaultEnvironment_ClientRpc(SerializableGuid instanceId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3934989725U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<SerializableGuid>(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 3934989725U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
			if (this.activeSkybox.Value == (AmbientSettings.Skybox)(-1))
			{
				this.ApplySkybox(this.CurrentSkybox);
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06001101 RID: 4353 RVA: 0x000558D8 File Offset: 0x00053AD8
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

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06001102 RID: 4354 RVA: 0x00055903 File Offset: 0x00053B03
		// (set) Token: 0x06001103 RID: 4355 RVA: 0x0005590B File Offset: 0x00053B0B
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06001104 RID: 4356 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x06001105 RID: 4357 RVA: 0x00055914 File Offset: 0x00053B14
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001106 RID: 4358 RVA: 0x0005591D File Offset: 0x00053B1D
		public void EndlessAwake()
		{
			if (base.IsServer && this.IsDefault)
			{
				this.activeSkybox.Value = this.CurrentSkybox;
			}
		}

		// Token: 0x06001107 RID: 4359 RVA: 0x00055940 File Offset: 0x00053B40
		public void PropPlaced(SerializableGuid instanceId, bool isCopy)
		{
			if (!isCopy && !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.HasDefaultEnvironmentSet)
			{
				this.SetDefaultEnvironment_ClientRpc(instanceId);
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.NetworkSetDefaultEnvironment(instanceId);
				if (!base.IsClient)
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
				}
			}
		}

		// Token: 0x06001109 RID: 4361 RVA: 0x000559C4 File Offset: 0x00053BC4
		protected override void __initializeVariables()
		{
			bool flag = this.activeSkybox == null;
			if (flag)
			{
				throw new Exception("AmbientSettings.activeSkybox cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.activeSkybox.Initialize(this);
			base.__nameNetworkVariable(this.activeSkybox, "activeSkybox");
			this.NetworkVariableFields.Add(this.activeSkybox);
			base.__initializeVariables();
		}

		// Token: 0x0600110A RID: 4362 RVA: 0x00055A27 File Offset: 0x00053C27
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3934989725U, new NetworkBehaviour.RpcReceiveHandler(AmbientSettings.__rpc_handler_3934989725), "SetDefaultEnvironment_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600110B RID: 4363 RVA: 0x00055A50 File Offset: 0x00053C50
		private static void __rpc_handler_3934989725(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			SerializableGuid serializableGuid;
			reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((AmbientSettings)target).SetDefaultEnvironment_ClientRpc(serializableGuid);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600110C RID: 4364 RVA: 0x00055AC0 File Offset: 0x00053CC0
		protected internal override string __getTypeName()
		{
			return "AmbientSettings";
		}

		// Token: 0x04000EA6 RID: 3750
		[SerializeField]
		private List<AmbientSettings.AmbientPair> ambientPairs = new List<AmbientSettings.AmbientPair>();

		// Token: 0x04000EA7 RID: 3751
		private NetworkVariable<AmbientSettings.Skybox> activeSkybox = new NetworkVariable<AmbientSettings.Skybox>((AmbientSettings.Skybox)(-1), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000EA8 RID: 3752
		private SerializableGuid instanceId = SerializableGuid.Empty;

		// Token: 0x04000EA9 RID: 3753
		private AmbientSettings.Skybox skybox;

		// Token: 0x04000EAA RID: 3754
		private Dictionary<AmbientSettings.Skybox, SimpleAmbientEntry> ambientEntryMap;

		// Token: 0x04000EAB RID: 3755
		private Context context;

		// Token: 0x020002F4 RID: 756
		public enum Skybox
		{
			// Token: 0x04000EAE RID: 3758
			Sunrise,
			// Token: 0x04000EAF RID: 3759
			EarlyMidMorning,
			// Token: 0x04000EB0 RID: 3760
			HighNoon,
			// Token: 0x04000EB1 RID: 3761
			Evening,
			// Token: 0x04000EB2 RID: 3762
			SpookyNight,
			// Token: 0x04000EB3 RID: 3763
			Sunrise2,
			// Token: 0x04000EB4 RID: 3764
			ClearNight,
			// Token: 0x04000EB5 RID: 3765
			DustStormDay,
			// Token: 0x04000EB6 RID: 3766
			Darkness,
			// Token: 0x04000EB7 RID: 3767
			Overcast,
			// Token: 0x04000EB8 RID: 3768
			Plague,
			// Token: 0x04000EB9 RID: 3769
			EndstarAcretion,
			// Token: 0x04000EBA RID: 3770
			SnowyDay,
			// Token: 0x04000EBB RID: 3771
			SnowyFog,
			// Token: 0x04000EBC RID: 3772
			Volcanic,
			// Token: 0x04000EBD RID: 3773
			Heavenly,
			// Token: 0x04000EBE RID: 3774
			Rainy,
			// Token: 0x04000EBF RID: 3775
			Haze,
			// Token: 0x04000EC0 RID: 3776
			Aurora,
			// Token: 0x04000EC1 RID: 3777
			Storm,
			// Token: 0x04000EC2 RID: 3778
			LightningStorm,
			// Token: 0x04000EC3 RID: 3779
			ClearMorning
		}

		// Token: 0x020002F5 RID: 757
		[Serializable]
		public class AmbientPair
		{
			// Token: 0x0600110D RID: 4365 RVA: 0x00055AC7 File Offset: 0x00053CC7
			public void Initialize()
			{
				this.AmbientEntry.OnDeactivated.AddListener(new UnityAction<AmbientEntry>(this.HandleDeactivated));
			}

			// Token: 0x0600110E RID: 4366 RVA: 0x00055AE5 File Offset: 0x00053CE5
			private void HandleDeactivated(AmbientEntry _)
			{
				this.OnDeactivated.Invoke(this.Skybox);
			}

			// Token: 0x04000EC4 RID: 3780
			[HideInInspector]
			public UnityEvent<AmbientSettings.Skybox> OnDeactivated = new UnityEvent<AmbientSettings.Skybox>();

			// Token: 0x04000EC5 RID: 3781
			public AmbientSettings.Skybox Skybox;

			// Token: 0x04000EC6 RID: 3782
			public SimpleAmbientEntry AmbientEntry;
		}
	}
}
