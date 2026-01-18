using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UISaveStatusManager : NetworkBehaviourSingleton<UISaveStatusManager>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI gameText;

	[SerializeField]
	private Image savingIndicator;

	private string levelString = string.Empty;

	private string gameString = string.Empty;

	private bool saveInProgress;

	private void Start()
	{
		bool canvasVisibility = PlayerPrefs.GetInt("Save Status Visible", 0) == 1;
		SetCanvasVisibility(canvasVisibility);
	}

	public void SetCanvasVisibility(bool isVisible)
	{
		Debug.Log(string.Format("{0} ( {1}: {2} )", "SetCanvasVisibility", "isVisible", isVisible), this);
		canvas.enabled = isVisible;
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		serializer.SerializeValue(ref levelString);
		serializer.SerializeValue(ref gameString);
		serializer.SerializeValue(ref saveInProgress, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			UpdateLevelString();
			UpdateGameString();
			UpdateSaveImageVisibility();
		}
	}

	[ClientRpc]
	private void UpdateSaveImageVisibility_ClientRpc(bool newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(947958136u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in newValue, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 947958136u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				saveInProgress = newValue;
				UpdateSaveImageVisibility();
			}
		}
	}

	private void UpdateSaveImageVisibility()
	{
		savingIndicator.enabled = saveInProgress;
	}

	[ClientRpc]
	private void UpdateGameString_ClientRpc(string newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2155308795u, clientRpcParams, RpcDelivery.Reliable);
			bool value = newValue != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newValue);
			}
			__endSendClientRpc(ref bufferWriter, 2155308795u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			gameString = newValue;
			UpdateGameString();
		}
	}

	private void UpdateGameString()
	{
		gameText.SetText(gameString);
	}

	[ClientRpc]
	private void UpdateLevelString_ClientRpc(string newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(83145972u, clientRpcParams, RpcDelivery.Reliable);
			bool value = newValue != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(newValue);
			}
			__endSendClientRpc(ref bufferWriter, 83145972u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			levelString = newValue;
			UpdateLevelString();
		}
	}

	private void UpdateLevelString()
	{
		levelText.SetText(levelString);
	}

	public void UpdateLevelVersion(string assetVersion, bool isVersionDirty)
	{
		if (SemanticVersion.TryParse(assetVersion, out var value))
		{
			if (isVersionDirty)
			{
				levelString = $"v{value.Patch}*";
			}
			else
			{
				levelString = $"v{value.Patch}";
			}
		}
		UpdateLevelString_ClientRpc(levelString);
	}

	public void UpdateGameVersion(string assetVersion, bool isVersionDirty)
	{
		if (SemanticVersion.TryParse(assetVersion, out var value))
		{
			if (isVersionDirty)
			{
				gameString = $"v{value.Patch}*";
			}
			else
			{
				gameString = $"v{value.Patch}";
			}
		}
		UpdateGameString_ClientRpc(gameString);
	}

	public void SetSaveIndicator(bool newValue)
	{
		saveInProgress = newValue;
		UpdateSaveImageVisibility_ClientRpc(newValue);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(947958136u, __rpc_handler_947958136, "UpdateSaveImageVisibility_ClientRpc");
		__registerRpc(2155308795u, __rpc_handler_2155308795, "UpdateGameString_ClientRpc");
		__registerRpc(83145972u, __rpc_handler_83145972, "UpdateLevelString_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_947958136(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateSaveImageVisibility_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2155308795(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateGameString_ClientRpc(s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_83145972(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, oneByteChars: false);
			}
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateLevelString_ClientRpc(s);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "UISaveStatusManager";
	}
}
