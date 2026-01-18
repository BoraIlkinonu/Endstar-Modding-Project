using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000DD RID: 221
	public class UISaveStatusManager : NetworkBehaviourSingleton<UISaveStatusManager>
	{
		// Token: 0x060003A4 RID: 932 RVA: 0x00017914 File Offset: 0x00015B14
		private void Start()
		{
			bool flag = PlayerPrefs.GetInt("Save Status Visible", 0) == 1;
			this.SetCanvasVisibility(flag);
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00017937 File Offset: 0x00015B37
		public void SetCanvasVisibility(bool isVisible)
		{
			Debug.Log(string.Format("{0} ( {1}: {2} )", "SetCanvasVisibility", "isVisible", isVisible), this);
			this.canvas.enabled = isVisible;
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x00017968 File Offset: 0x00015B68
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			serializer.SerializeValue(ref this.levelString, false);
			serializer.SerializeValue(ref this.gameString, false);
			serializer.SerializeValue<bool>(ref this.saveInProgress, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.UpdateLevelString();
				this.UpdateGameString();
				this.UpdateSaveImageVisibility();
			}
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x000179C8 File Offset: 0x00015BC8
		[ClientRpc]
		private void UpdateSaveImageVisibility_ClientRpc(bool newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(947958136U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in newValue, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 947958136U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.saveInProgress = newValue;
			this.UpdateSaveImageVisibility();
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x00017AC3 File Offset: 0x00015CC3
		private void UpdateSaveImageVisibility()
		{
			this.savingIndicator.enabled = this.saveInProgress;
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x00017AD8 File Offset: 0x00015CD8
		[ClientRpc]
		private void UpdateGameString_ClientRpc(string newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2155308795U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = newValue != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newValue, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 2155308795U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.gameString = newValue;
			this.UpdateGameString();
		}

		// Token: 0x060003AA RID: 938 RVA: 0x00017BF6 File Offset: 0x00015DF6
		private void UpdateGameString()
		{
			this.gameText.SetText(this.gameString, true);
		}

		// Token: 0x060003AB RID: 939 RVA: 0x00017C0C File Offset: 0x00015E0C
		[ClientRpc]
		private void UpdateLevelString_ClientRpc(string newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(83145972U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = newValue != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(newValue, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 83145972U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.levelString = newValue;
			this.UpdateLevelString();
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00017D2A File Offset: 0x00015F2A
		private void UpdateLevelString()
		{
			this.levelText.SetText(this.levelString, true);
		}

		// Token: 0x060003AD RID: 941 RVA: 0x00017D40 File Offset: 0x00015F40
		public void UpdateLevelVersion(string assetVersion, bool isVersionDirty)
		{
			SemanticVersion semanticVersion;
			if (SemanticVersion.TryParse(assetVersion, out semanticVersion))
			{
				if (isVersionDirty)
				{
					this.levelString = string.Format("v{0}*", semanticVersion.Patch);
				}
				else
				{
					this.levelString = string.Format("v{0}", semanticVersion.Patch);
				}
			}
			this.UpdateLevelString_ClientRpc(this.levelString);
		}

		// Token: 0x060003AE RID: 942 RVA: 0x00017DA0 File Offset: 0x00015FA0
		public void UpdateGameVersion(string assetVersion, bool isVersionDirty)
		{
			SemanticVersion semanticVersion;
			if (SemanticVersion.TryParse(assetVersion, out semanticVersion))
			{
				if (isVersionDirty)
				{
					this.gameString = string.Format("v{0}*", semanticVersion.Patch);
				}
				else
				{
					this.gameString = string.Format("v{0}", semanticVersion.Patch);
				}
			}
			this.UpdateGameString_ClientRpc(this.gameString);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00017DFE File Offset: 0x00015FFE
		public void SetSaveIndicator(bool newValue)
		{
			this.saveInProgress = newValue;
			this.UpdateSaveImageVisibility_ClientRpc(newValue);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x00017E2C File Offset: 0x0001602C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060003B2 RID: 946 RVA: 0x00017E44 File Offset: 0x00016044
		protected override void __initializeRpcs()
		{
			base.__registerRpc(947958136U, new NetworkBehaviour.RpcReceiveHandler(UISaveStatusManager.__rpc_handler_947958136), "UpdateSaveImageVisibility_ClientRpc");
			base.__registerRpc(2155308795U, new NetworkBehaviour.RpcReceiveHandler(UISaveStatusManager.__rpc_handler_2155308795), "UpdateGameString_ClientRpc");
			base.__registerRpc(83145972U, new NetworkBehaviour.RpcReceiveHandler(UISaveStatusManager.__rpc_handler_83145972), "UpdateLevelString_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00017EB0 File Offset: 0x000160B0
		private static void __rpc_handler_947958136(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateSaveImageVisibility_ClientRpc(flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00017F20 File Offset: 0x00016120
		private static void __rpc_handler_2155308795(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateGameString_ClientRpc(text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003B5 RID: 949 RVA: 0x00017FB0 File Offset: 0x000161B0
		private static void __rpc_handler_83145972(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			string text = null;
			if (flag)
			{
				reader.ReadValueSafe(out text, false);
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((UISaveStatusManager)target).UpdateLevelString_ClientRpc(text);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003B6 RID: 950 RVA: 0x0001803D File Offset: 0x0001623D
		protected internal override string __getTypeName()
		{
			return "UISaveStatusManager";
		}

		// Token: 0x040003D4 RID: 980
		[SerializeField]
		private Canvas canvas;

		// Token: 0x040003D5 RID: 981
		[SerializeField]
		private TextMeshProUGUI levelText;

		// Token: 0x040003D6 RID: 982
		[SerializeField]
		private TextMeshProUGUI gameText;

		// Token: 0x040003D7 RID: 983
		[SerializeField]
		private Image savingIndicator;

		// Token: 0x040003D8 RID: 984
		private string levelString = string.Empty;

		// Token: 0x040003D9 RID: 985
		private string gameString = string.Empty;

		// Token: 0x040003DA RID: 986
		private bool saveInProgress;
	}
}
