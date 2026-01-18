using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class TeamComponent : NetworkBehaviour, IComponentBase, IScriptInjector
{
	[SerializeField]
	private Team team;

	[SerializeField]
	private bool teamLocked;

	private Endless.Gameplay.LuaInterfaces.TeamComponent luaInterface;

	public Team Team
	{
		get
		{
			return team;
		}
		set
		{
			if (!teamLocked)
			{
				team = value;
				if (base.IsServer)
				{
					TeamChanged_ClientRpc(value);
				}
			}
		}
	}

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public object LuaObject => luaInterface ?? new Endless.Gameplay.LuaInterfaces.TeamComponent(this);

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.TeamComponent);

	[ClientRpc]
	private void TeamChanged_ClientRpc(Team newTeam)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2007699018u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in newTeam, default(FastBufferWriter.ForEnums));
				__endSendClientRpc(ref bufferWriter, 2007699018u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				team = newTeam;
			}
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		serializer.SerializeValue(ref team, default(FastBufferWriter.ForEnums));
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2007699018u, __rpc_handler_2007699018, "TeamChanged_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2007699018(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Team value, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((TeamComponent)target).TeamChanged_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "TeamComponent";
	}
}
