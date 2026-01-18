using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Endless.Gameplay
{
	// Token: 0x02000314 RID: 788
	public class Filter : EndlessNetworkBehaviour, IGameEndSubscriber, IBaseType, IComponentBase, IAwakeSubscriber
	{
		// Token: 0x1700039F RID: 927
		// (get) Token: 0x0600123F RID: 4671 RVA: 0x0005A76D File Offset: 0x0005896D
		// (set) Token: 0x06001240 RID: 4672 RVA: 0x0005A778 File Offset: 0x00058978
		public Filter.FilterType CurrentFilterType
		{
			get
			{
				return this.serverFilter;
			}
			set
			{
				if (this.serverChanged)
				{
					return;
				}
				if (this.serverFilter != value)
				{
					this.serverFilter = value;
					if (this.visibleInCreator && base.IsClient)
					{
						if (this.appliedFilter != Filter.FilterType.None)
						{
							this.StartTransition(this.appliedFilter, 1f, 0f);
						}
						this.appliedFilter = value;
						if (this.appliedFilter != Filter.FilterType.None)
						{
							this.StartTransition(this.appliedFilter, 0f, 1f);
						}
					}
				}
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06001241 RID: 4673 RVA: 0x0005A7F1 File Offset: 0x000589F1
		// (set) Token: 0x06001242 RID: 4674 RVA: 0x0005A7FC File Offset: 0x000589FC
		public bool VisibleInCreator
		{
			get
			{
				return this.visibleInCreator;
			}
			set
			{
				if (this.serverChanged)
				{
					return;
				}
				if (this.visibleInCreator != value)
				{
					this.visibleInCreator = value;
					if (this.visibleInCreator)
					{
						if (this.serverFilter != Filter.FilterType.None)
						{
							this.appliedFilter = this.serverFilter;
							this.StartTransition(this.serverFilter, 0f, 1f);
							return;
						}
					}
					else if (this.appliedFilter != Filter.FilterType.None)
					{
						this.StartTransition(this.appliedFilter, 1f, 0f);
						this.appliedFilter = Filter.FilterType.None;
					}
				}
			}
		}

		// Token: 0x06001243 RID: 4675 RVA: 0x0005A87C File Offset: 0x00058A7C
		public void SetFilterType(Context context, Filter.FilterType value)
		{
			if (base.IsClient && this.appliedFilter != Filter.FilterType.None)
			{
				this.StartTransition(this.appliedFilter, 1f, 0f);
			}
			if (base.IsServer)
			{
				this.serverFilter = value;
				this.serverChanged = true;
				this.SetServerFilter_ClientRpc(value);
			}
			if (base.IsClient)
			{
				this.appliedFilter = value;
				if (this.appliedFilter != Filter.FilterType.None)
				{
					this.StartTransition(this.appliedFilter, 0f, 1f);
				}
			}
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x0005A8FC File Offset: 0x00058AFC
		[ClientRpc]
		private void SetServerFilter_ClientRpc(Filter.FilterType value)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2009833482U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<Filter.FilterType>(in value, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 2009833482U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.IsServer)
			{
				return;
			}
			this.serverFilter = value;
			this.SetFilterType(this.Context, value);
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x0005AA08 File Offset: 0x00058C08
		public void SetFilterTypeForPlayer(Context context, Filter.FilterType value)
		{
			if (context.IsPlayer())
			{
				PlayerLuaComponent userComponent = context.WorldObject.GetUserComponent<PlayerLuaComponent>();
				ClientRpcParams clientRpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = new ulong[] { userComponent.WorldObject.NetworkObject.OwnerClientId }
					}
				};
				this.SetLocalFilterType_ClientRpc(value, clientRpcParams);
			}
		}

		// Token: 0x06001246 RID: 4678 RVA: 0x0005AA6C File Offset: 0x00058C6C
		[ClientRpc]
		public void SetLocalFilterType_ClientRpc(Filter.FilterType value, ClientRpcParams clientParams)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(4004194627U, clientParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<Filter.FilterType>(in value, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 4004194627U, clientParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.localFilter = value;
			if (this.serverFilter == Filter.FilterType.None)
			{
				if (this.appliedFilter != Filter.FilterType.None)
				{
					this.StartTransition(this.appliedFilter, 1f, 0f);
				}
				this.appliedFilter = value;
				if (this.appliedFilter != Filter.FilterType.None)
				{
					this.StartTransition(this.appliedFilter, 0f, 1f);
				}
			}
		}

		// Token: 0x06001247 RID: 4679 RVA: 0x0005ABAC File Offset: 0x00058DAC
		private void Awake()
		{
			foreach (Filter.FilterTypePair filterTypePair in this.filterTypes)
			{
				this.filterMap.Add(filterTypePair.FilterType, filterTypePair.Volume);
			}
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x0005AC10 File Offset: 0x00058E10
		public void EndlessGameEnd()
		{
			base.StopAllCoroutines();
			this.serverChanged = false;
			this.localFilter = Filter.FilterType.None;
			this.serverFilter = Filter.FilterType.None;
			if (this.appliedFilter != Filter.FilterType.None)
			{
				this.filterMap[this.appliedFilter].weight = 0f;
				this.appliedFilter = Filter.FilterType.None;
			}
		}

		// Token: 0x06001249 RID: 4681 RVA: 0x0005AC64 File Offset: 0x00058E64
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			serializer.SerializeValue<Filter.FilterType>(ref this.serverFilter, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue<bool>(ref this.serverChanged, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader && this.serverFilter != Filter.FilterType.None)
			{
				this.appliedFilter = this.serverFilter;
				this.filterMap[this.serverFilter].weight = 1f;
			}
		}

		// Token: 0x0600124A RID: 4682 RVA: 0x0005ACD9 File Offset: 0x00058ED9
		public void SetTransitionTime(Context context, float newTransitionTime)
		{
			this.transitionTime = newTransitionTime;
		}

		// Token: 0x0600124B RID: 4683 RVA: 0x0005ACE4 File Offset: 0x00058EE4
		private void StartTransition(Filter.FilterType filterType, float start, float end)
		{
			if (this.liveTransitions.ContainsKey(filterType))
			{
				base.StopCoroutine(this.liveTransitions[filterType]);
				this.liveTransitions[filterType] = base.StartCoroutine(this.LerpVolume(filterType, this.filterMap[filterType], start, end));
				return;
			}
			this.liveTransitions.Add(filterType, base.StartCoroutine(this.LerpVolume(filterType, this.filterMap[filterType], start, end)));
		}

		// Token: 0x0600124C RID: 4684 RVA: 0x0005AD60 File Offset: 0x00058F60
		private IEnumerator LerpVolume(Filter.FilterType filterType, Volume volume, float start, float end)
		{
			yield return null;
			float num = Mathf.InverseLerp(start, end, volume.weight);
			for (float elapsedTime = num * this.transitionTime; elapsedTime < this.transitionTime; elapsedTime += Time.deltaTime)
			{
				volume.weight = Mathf.Lerp(start, end, Mathf.Clamp01(elapsedTime / this.transitionTime));
				yield return null;
			}
			volume.weight = end;
			this.liveTransitions.Remove(filterType);
			yield break;
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x0600124D RID: 4685 RVA: 0x0005AD8C File Offset: 0x00058F8C
		// (set) Token: 0x0600124E RID: 4686 RVA: 0x0005AD94 File Offset: 0x00058F94
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x0600124F RID: 4687 RVA: 0x0005ADA0 File Offset: 0x00058FA0
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

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06001250 RID: 4688 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x0005ADCB File Offset: 0x00058FCB
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x0005ADD4 File Offset: 0x00058FD4
		public void EndlessAwake()
		{
			if (this.serverFilter != Filter.FilterType.None)
			{
				this.appliedFilter = this.serverFilter;
				this.filterMap[this.appliedFilter].weight = 1f;
			}
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x0005AE40 File Offset: 0x00059040
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x0005AE58 File Offset: 0x00059058
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2009833482U, new NetworkBehaviour.RpcReceiveHandler(Filter.__rpc_handler_2009833482), "SetServerFilter_ClientRpc");
			base.__registerRpc(4004194627U, new NetworkBehaviour.RpcReceiveHandler(Filter.__rpc_handler_4004194627), "SetLocalFilterType_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x0005AEA8 File Offset: 0x000590A8
		private static void __rpc_handler_2009833482(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Filter.FilterType filterType;
			reader.ReadValueSafe<Filter.FilterType>(out filterType, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Filter)target).SetServerFilter_ClientRpc(filterType);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x0005AF18 File Offset: 0x00059118
		private static void __rpc_handler_4004194627(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Filter.FilterType filterType;
			reader.ReadValueSafe<Filter.FilterType>(out filterType, default(FastBufferWriter.ForEnums));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Filter)target).SetLocalFilterType_ClientRpc(filterType, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001258 RID: 4696 RVA: 0x0005AF96 File Offset: 0x00059196
		protected internal override string __getTypeName()
		{
			return "Filter";
		}

		// Token: 0x04000F9A RID: 3994
		[Tooltip("This is expected to contain an entry for each filter type except none!")]
		[SerializeField]
		private List<Filter.FilterTypePair> filterTypes = new List<Filter.FilterTypePair>();

		// Token: 0x04000F9B RID: 3995
		[SerializeField]
		private float transitionTime = 2f;

		// Token: 0x04000F9C RID: 3996
		[SerializeField]
		private bool visibleInCreator = true;

		// Token: 0x04000F9D RID: 3997
		[FormerlySerializedAs("currentFilter")]
		[SerializeField]
		private Filter.FilterType serverFilter;

		// Token: 0x04000F9E RID: 3998
		private Dictionary<Filter.FilterType, Volume> filterMap = new Dictionary<Filter.FilterType, Volume>();

		// Token: 0x04000F9F RID: 3999
		private Dictionary<Filter.FilterType, Coroutine> liveTransitions = new Dictionary<Filter.FilterType, Coroutine>();

		// Token: 0x04000FA0 RID: 4000
		private Filter.FilterType localFilter;

		// Token: 0x04000FA1 RID: 4001
		private Filter.FilterType appliedFilter;

		// Token: 0x04000FA2 RID: 4002
		private bool serverChanged;

		// Token: 0x04000FA3 RID: 4003
		private Context context;

		// Token: 0x02000315 RID: 789
		public enum FilterType
		{
			// Token: 0x04000FA6 RID: 4006
			None,
			// Token: 0x04000FA7 RID: 4007
			FilmNoir,
			// Token: 0x04000FA8 RID: 4008
			Sepia,
			// Token: 0x04000FA9 RID: 4009
			Blurred,
			// Token: 0x04000FAA RID: 4010
			Neon,
			// Token: 0x04000FAB RID: 4011
			Warm,
			// Token: 0x04000FAC RID: 4012
			Curse,
			// Token: 0x04000FAD RID: 4013
			Meadow,
			// Token: 0x04000FAE RID: 4014
			Curved
		}

		// Token: 0x02000316 RID: 790
		[Serializable]
		public class FilterTypePair
		{
			// Token: 0x04000FAF RID: 4015
			public Volume Volume;

			// Token: 0x04000FB0 RID: 4016
			public Filter.FilterType FilterType;
		}
	}
}
