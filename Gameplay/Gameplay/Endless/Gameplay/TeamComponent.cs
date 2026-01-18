using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200035F RID: 863
	public class TeamComponent : NetworkBehaviour, IComponentBase, IScriptInjector
	{
		// Token: 0x060015FB RID: 5627 RVA: 0x00068150 File Offset: 0x00066350
		[ClientRpc]
		private void TeamChanged_ClientRpc(Team newTeam)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2007699018U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<Team>(in newTeam, default(FastBufferWriter.ForEnums));
				base.__endSendClientRpc(ref fastBufferWriter, 2007699018U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.team = newTeam;
		}

		// Token: 0x060015FC RID: 5628 RVA: 0x00068248 File Offset: 0x00066448
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			serializer.SerializeValue<Team>(ref this.team, default(FastBufferWriter.ForEnums));
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x060015FD RID: 5629 RVA: 0x00068271 File Offset: 0x00066471
		// (set) Token: 0x060015FE RID: 5630 RVA: 0x00068279 File Offset: 0x00066479
		public Team Team
		{
			get
			{
				return this.team;
			}
			set
			{
				if (!this.teamLocked)
				{
					this.team = value;
					if (base.IsServer)
					{
						this.TeamChanged_ClientRpc(value);
					}
				}
			}
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x060015FF RID: 5631 RVA: 0x00068299 File Offset: 0x00066499
		// (set) Token: 0x06001600 RID: 5632 RVA: 0x000682A1 File Offset: 0x000664A1
		public WorldObject WorldObject { get; private set; }

		// Token: 0x06001601 RID: 5633 RVA: 0x000682AA File Offset: 0x000664AA
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06001602 RID: 5634 RVA: 0x000682B3 File Offset: 0x000664B3
		public object LuaObject
		{
			get
			{
				return this.luaInterface ?? new TeamComponent(this);
			}
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x06001603 RID: 5635 RVA: 0x000682C5 File Offset: 0x000664C5
		public Type LuaObjectType
		{
			get
			{
				return typeof(TeamComponent);
			}
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x000682D4 File Offset: 0x000664D4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x000682EA File Offset: 0x000664EA
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2007699018U, new NetworkBehaviour.RpcReceiveHandler(TeamComponent.__rpc_handler_2007699018), "TeamChanged_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x00068310 File Offset: 0x00066510
		private static void __rpc_handler_2007699018(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Team team;
			reader.ReadValueSafe<Team>(out team, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((TeamComponent)target).TeamChanged_ClientRpc(team);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x00068380 File Offset: 0x00066580
		protected internal override string __getTypeName()
		{
			return "TeamComponent";
		}

		// Token: 0x040011EF RID: 4591
		[SerializeField]
		private Team team;

		// Token: 0x040011F0 RID: 4592
		[SerializeField]
		private bool teamLocked;

		// Token: 0x040011F2 RID: 4594
		private TeamComponent luaInterface;
	}
}
