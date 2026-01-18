using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.UI;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using NLua;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000341 RID: 833
	public class TextBubble : EndlessNetworkBehaviour, IPersistantStateSubscriber, IAwakeSubscriber, IComponentBase, IScriptInjector
	{
		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x06001418 RID: 5144 RVA: 0x00060DE5 File Offset: 0x0005EFE5
		// (set) Token: 0x06001419 RID: 5145 RVA: 0x00060DED File Offset: 0x0005EFED
		public LocalizedString[] Text
		{
			get
			{
				return this.text;
			}
			set
			{
				this.text = value;
				Action action = this.onSerializedTextsChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x0600141A RID: 5146 RVA: 0x00060E06 File Offset: 0x0005F006
		// (set) Token: 0x0600141B RID: 5147 RVA: 0x00060E13 File Offset: 0x0005F013
		public LocalizedString DisplayName
		{
			get
			{
				return this.runtimeDisplayName.Value;
			}
			set
			{
				this.runtimeDisplayName.Value = value;
			}
		}

		// Token: 0x0600141C RID: 5148 RVA: 0x00060E21 File Offset: 0x0005F021
		private void Awake()
		{
			this.onSerializedTextsChanged = (Action)Delegate.Combine(this.onSerializedTextsChanged, new Action(this.HandleSerializedTextsChanged));
			this.SetText();
		}

		// Token: 0x0600141D RID: 5149 RVA: 0x00060E4B File Offset: 0x0005F04B
		private void HandleSerializedTextsChanged()
		{
			if (base.IsHost)
			{
				this.UpdateText(this.text);
			}
		}

		// Token: 0x0600141E RID: 5150 RVA: 0x00060E68 File Offset: 0x0005F068
		private void ValidateStringLengths(IList<string> text)
		{
			for (int i = 0; i < text.Count; i++)
			{
				if (text[i].Length > this.maxCharacters)
				{
					text[i] = text[i].Substring(0, this.maxCharacters);
				}
			}
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x00060EB4 File Offset: 0x0005F0B4
		internal void Display(int textIndex, bool showProgress = true, bool showInteract = true)
		{
			if (textIndex >= this.runtimeText.Length)
			{
				this.Close();
				return;
			}
			this.ReadClientRpc(this.runtimeText.stringPairs[textIndex], textIndex, this.runtimeText.Length, this.fontSize, showProgress, showInteract, default(ClientRpcParams));
		}

		// Token: 0x06001420 RID: 5152 RVA: 0x00060F08 File Offset: 0x0005F108
		internal void DisplayForTarget(Context target, int textIndex, bool showProgress = true, bool showInteract = true)
		{
			if (target == null || !target.IsPlayer())
			{
				return;
			}
			if (textIndex >= this.runtimeText.Length)
			{
				this.CloseForTarget(target);
				return;
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { target.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			this.ReadClientRpc(this.runtimeText.stringPairs[textIndex], textIndex, this.runtimeText.Length, this.fontSize, showProgress, showInteract, clientRpcParams);
		}

		// Token: 0x06001421 RID: 5153 RVA: 0x00060F9C File Offset: 0x0005F19C
		internal void DisplayWithDuration(int textIndex, float duration, bool showProgress = true, bool showInteract = true)
		{
			this.Display(textIndex, showProgress, showInteract);
			base.StartCoroutine(this.WaitToCloseRoutine(duration));
		}

		// Token: 0x06001422 RID: 5154 RVA: 0x00060FB8 File Offset: 0x0005F1B8
		internal void DisplayForTargetWithDuration(Context target, int textIndex, float duration, bool showProgress = true, bool showInteract = true)
		{
			if (target == null || !target.IsPlayer())
			{
				return;
			}
			if (textIndex >= this.runtimeText.Length)
			{
				this.CloseForTarget(target);
				return;
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { target.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			this.ReadClientRpc(this.runtimeText.stringPairs[textIndex], textIndex, this.runtimeText.Length, this.fontSize, showProgress, showInteract, clientRpcParams);
			base.StartCoroutine(this.WaitToCloseRoutineForTarget(target, duration));
		}

		// Token: 0x06001423 RID: 5155 RVA: 0x0006105C File Offset: 0x0005F25C
		private IEnumerator WaitToCloseRoutine(float duration)
		{
			yield return new WaitForSeconds(duration);
			this.Close();
			yield break;
		}

		// Token: 0x06001424 RID: 5156 RVA: 0x00061072 File Offset: 0x0005F272
		private IEnumerator WaitToCloseRoutineForTarget(Context target, float duration)
		{
			yield return new WaitForSeconds(duration);
			this.CloseForTarget(target);
			yield break;
		}

		// Token: 0x06001425 RID: 5157 RVA: 0x00061090 File Offset: 0x0005F290
		[ClientRpc]
		private void ReadClientRpc(LocalizedString textToDisplay, int dialogueIndex, int maximumDialogueIndex, float currentFontSize, bool showIndex = true, bool showInteract = true, ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(601407714U, rpcParams, RpcDelivery.Reliable);
				bool flag = textToDisplay != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe<LocalizedString>(in textToDisplay, default(FastBufferWriter.ForNetworkSerializable));
				}
				BytePacker.WriteValueBitPacked(fastBufferWriter, dialogueIndex);
				BytePacker.WriteValueBitPacked(fastBufferWriter, maximumDialogueIndex);
				fastBufferWriter.WriteValueSafe<float>(in currentFontSize, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<bool>(in showIndex, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<bool>(in showInteract, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 601407714U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			string localizedString = textToDisplay.GetLocalizedString();
			this.ValidateStringLengths(new string[] { localizedString });
			if (this.dialogueBubble == null)
			{
				string text = ((this.DisplayName != null) ? this.DisplayName.GetLocalizedString() : "");
				this.dialogueBubble = UIDialogueBubbleAnchor.CreateInstance(this.dialogueBubbleAnchorSource, this.references.Anchor, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, text, new global::UnityEngine.Vector3?(this.dialogueBubbleOffset));
			}
			bool flag2 = dialogueIndex == 0;
			this.dialogueBubble.DisplayText(localizedString, dialogueIndex, maximumDialogueIndex, flag2, currentFontSize, showIndex, showInteract);
		}

		// Token: 0x06001426 RID: 5158 RVA: 0x000612A8 File Offset: 0x0005F4A8
		[ClientRpc]
		private void Shake_ClientRpc(ClientRpcParams rpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1686333811U, rpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1686333811U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.dialogueBubble)
			{
				this.dialogueBubble.Shake();
			}
		}

		// Token: 0x06001427 RID: 5159 RVA: 0x00061394 File Offset: 0x0005F594
		internal void Close()
		{
			this.CloseClientRpc(default(ClientRpcParams));
		}

		// Token: 0x06001428 RID: 5160 RVA: 0x000613B0 File Offset: 0x0005F5B0
		internal void CloseForTarget(Context context)
		{
			if (context == null || !context.IsPlayer())
			{
				return;
			}
			if (!context.WorldObject.NetworkObject)
			{
				return;
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { context.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			this.CloseClientRpc(clientRpcParams);
		}

		// Token: 0x06001429 RID: 5161 RVA: 0x0006141F File Offset: 0x0005F61F
		internal int GetTextLength()
		{
			return this.runtimeText.Length;
		}

		// Token: 0x0600142A RID: 5162 RVA: 0x0006142C File Offset: 0x0005F62C
		public void UpdateText(LocalizedStringCollection newText)
		{
			this.runtimeText = newText;
		}

		// Token: 0x0600142B RID: 5163 RVA: 0x00061438 File Offset: 0x0005F638
		public void UpdateText(string[] strings)
		{
			LocalizedString[] array = new LocalizedString[strings.Length];
			for (int i = 0; i < strings.Length; i++)
			{
				array[i] = new LocalizedString();
				array[i].SetStringValue(strings[i], LocalizedString.ActiveLanguage);
			}
			this.UpdateText(new LocalizedStringCollection
			{
				stringPairs = array
			});
		}

		// Token: 0x0600142C RID: 5164 RVA: 0x0006148C File Offset: 0x0005F68C
		public void UpdateText(LuaTable newText)
		{
			LocalizedString[] array = new LocalizedString[newText.Keys.Count];
			int num = 0;
			foreach (object obj in newText.Values)
			{
				Type type = obj.GetType();
				if (type == typeof(LocalizedString))
				{
					array[num] = (LocalizedString)obj;
				}
				else
				{
					if (!(type == typeof(LuaTable)))
					{
						this.scriptComponent.LogError("Inner value must be a lua table or localized string");
						return;
					}
					LocalizedString localizedString = new LocalizedString();
					LuaTable luaTable = (LuaTable)obj;
					foreach (object obj2 in luaTable.Keys)
					{
						if (obj2.GetType() != typeof(long))
						{
							this.scriptComponent.LogError("Inner indexes must be a language");
							return;
						}
						Language language = (Language)((long)obj2);
						object obj3 = luaTable[obj2];
						if (obj3.GetType() != typeof(string))
						{
							this.scriptComponent.LogError("Inner values must be a string");
							return;
						}
						string text = (string)obj3;
						localizedString.SetStringValue(text, language);
					}
					array[num] = localizedString;
				}
				num++;
			}
			this.UpdateText(array);
		}

		// Token: 0x0600142D RID: 5165 RVA: 0x00061654 File Offset: 0x0005F854
		[ClientRpc]
		private void CloseClientRpc(ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1732833465U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1732833465U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.dialogueBubble)
			{
				this.dialogueBubble.Close();
			}
			this.dialogueBubble = null;
		}

		// Token: 0x0600142E RID: 5166 RVA: 0x00061746 File Offset: 0x0005F946
		public void SetDisplayName(Context _, LocalizedString newName)
		{
			this.DisplayName = newName;
		}

		// Token: 0x0600142F RID: 5167 RVA: 0x0006174F File Offset: 0x0005F94F
		public void SetText(Context _, string[] newText)
		{
			this.ResetFontSize();
			this.UpdateText(newText);
		}

		// Token: 0x06001430 RID: 5168 RVA: 0x0006175E File Offset: 0x0005F95E
		public void SetLocalizedText(Context _, LocalizedString[] text)
		{
			this.ResetFontSize();
			this.UpdateText(text);
		}

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x06001431 RID: 5169 RVA: 0x00061772 File Offset: 0x0005F972
		// (set) Token: 0x06001432 RID: 5170 RVA: 0x0006177A File Offset: 0x0005F97A
		public bool ShouldSaveAndLoad { get; set; } = true;

		// Token: 0x1700041F RID: 1055
		// (get) Token: 0x06001433 RID: 5171 RVA: 0x00061783 File Offset: 0x0005F983
		public float ShakeDuration
		{
			get
			{
				return 0.5f;
			}
		}

		// Token: 0x06001434 RID: 5172 RVA: 0x0006178A File Offset: 0x0005F98A
		public object GetSaveState()
		{
			return new object[]
			{
				this.runtimeDisplayName.Value,
				this.runtimeText
			};
		}

		// Token: 0x06001435 RID: 5173 RVA: 0x000617B0 File Offset: 0x0005F9B0
		public void LoadState(object loadedState)
		{
			if (loadedState == null)
			{
				return;
			}
			object[] array = (object[])loadedState;
			this.runtimeDisplayName.Value = (LocalizedString)array[0];
			this.UpdateText((LocalizedStringCollection)array[1]);
		}

		// Token: 0x06001436 RID: 5174 RVA: 0x000617E9 File Offset: 0x0005F9E9
		public void SetFontSize(float newFontSize)
		{
			this.fontSize = newFontSize;
		}

		// Token: 0x06001437 RID: 5175 RVA: 0x000617F2 File Offset: 0x0005F9F2
		public void ResetFontSize()
		{
			this.fontSize = 22f;
		}

		// Token: 0x06001438 RID: 5176 RVA: 0x00061800 File Offset: 0x0005FA00
		public void ShakeForTarget(Context target)
		{
			ClientRpcParams clientRpcParams = TextBubble.GetClientRpcParams(target);
			this.Shake_ClientRpc(clientRpcParams);
		}

		// Token: 0x06001439 RID: 5177 RVA: 0x0006181C File Offset: 0x0005FA1C
		private static ClientRpcParams GetClientRpcParams(Context target)
		{
			return new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { target.WorldObject.NetworkObject.OwnerClientId }
				}
			};
		}

		// Token: 0x0600143A RID: 5178 RVA: 0x00061864 File Offset: 0x0005FA64
		public void ShakeForAll()
		{
			this.Shake_ClientRpc(default(ClientRpcParams));
		}

		// Token: 0x0600143B RID: 5179 RVA: 0x00061880 File Offset: 0x0005FA80
		public void ShowAlert(string displayText)
		{
			this.ShowAlert_ClientRpc(displayText, default(ClientRpcParams));
		}

		// Token: 0x0600143C RID: 5180 RVA: 0x000618A0 File Offset: 0x0005FAA0
		public void ShowAlertForTarget(Context target, string displayText)
		{
			ClientRpcParams clientRpcParams = TextBubble.GetClientRpcParams(target);
			this.ShowAlert_ClientRpc(displayText, clientRpcParams);
		}

		// Token: 0x0600143D RID: 5181 RVA: 0x000618BC File Offset: 0x0005FABC
		[ClientRpc]
		private void ShowAlert_ClientRpc(string displayText, ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2234065250U, clientRpcParams, RpcDelivery.Reliable);
				bool flag = displayText != null;
				fastBufferWriter.WriteValueSafe<bool>(in flag, default(FastBufferWriter.ForPrimitives));
				if (flag)
				{
					fastBufferWriter.WriteValueSafe(displayText, false);
				}
				base.__endSendClientRpc(ref fastBufferWriter, 2234065250U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.dialogueBubble)
			{
				this.dialogueBubble.ShowAlert(displayText);
			}
		}

		// Token: 0x0600143E RID: 5182 RVA: 0x00060E4B File Offset: 0x0005F04B
		public void EndlessAwake()
		{
			if (base.IsHost)
			{
				this.UpdateText(this.text);
			}
		}

		// Token: 0x17000420 RID: 1056
		// (get) Token: 0x0600143F RID: 5183 RVA: 0x000619E6 File Offset: 0x0005FBE6
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(TextBubbleReferences);
			}
		}

		// Token: 0x17000421 RID: 1057
		// (get) Token: 0x06001440 RID: 5184 RVA: 0x000619F2 File Offset: 0x0005FBF2
		// (set) Token: 0x06001441 RID: 5185 RVA: 0x000619FA File Offset: 0x0005FBFA
		public WorldObject WorldObject { get; private set; }

		// Token: 0x06001442 RID: 5186 RVA: 0x00061A03 File Offset: 0x0005FC03
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001443 RID: 5187 RVA: 0x00061A0C File Offset: 0x0005FC0C
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (TextBubbleReferences)referenceBase;
		}

		// Token: 0x06001444 RID: 5188 RVA: 0x00061A1C File Offset: 0x0005FC1C
		private void SetText()
		{
			this.text = new LocalizedString[this.text.Length];
			for (int i = 0; i < this.defaultText.Length; i++)
			{
				this.text[i] = new LocalizedString();
				this.text[i].SetStringValue(this.defaultText[i], LocalizedString.ActiveLanguage);
			}
		}

		// Token: 0x17000422 RID: 1058
		// (get) Token: 0x06001445 RID: 5189 RVA: 0x00061A78 File Offset: 0x0005FC78
		public object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new TextBubble(this));
				}
				return obj;
			}
		}

		// Token: 0x17000423 RID: 1059
		// (get) Token: 0x06001446 RID: 5190 RVA: 0x00061A9E File Offset: 0x0005FC9E
		public Type LuaObjectType
		{
			get
			{
				return typeof(TextBubble);
			}
		}

		// Token: 0x06001447 RID: 5191 RVA: 0x00061AAA File Offset: 0x0005FCAA
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001449 RID: 5193 RVA: 0x00061ADC File Offset: 0x0005FCDC
		protected override void __initializeVariables()
		{
			bool flag = this.runtimeDisplayName == null;
			if (flag)
			{
				throw new Exception("TextBubble.runtimeDisplayName cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.runtimeDisplayName.Initialize(this);
			base.__nameNetworkVariable(this.runtimeDisplayName, "runtimeDisplayName");
			this.NetworkVariableFields.Add(this.runtimeDisplayName);
			base.__initializeVariables();
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x00061B40 File Offset: 0x0005FD40
		protected override void __initializeRpcs()
		{
			base.__registerRpc(601407714U, new NetworkBehaviour.RpcReceiveHandler(TextBubble.__rpc_handler_601407714), "ReadClientRpc");
			base.__registerRpc(1686333811U, new NetworkBehaviour.RpcReceiveHandler(TextBubble.__rpc_handler_1686333811), "Shake_ClientRpc");
			base.__registerRpc(1732833465U, new NetworkBehaviour.RpcReceiveHandler(TextBubble.__rpc_handler_1732833465), "CloseClientRpc");
			base.__registerRpc(2234065250U, new NetworkBehaviour.RpcReceiveHandler(TextBubble.__rpc_handler_2234065250), "ShowAlert_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600144B RID: 5195 RVA: 0x00061BC8 File Offset: 0x0005FDC8
		private static void __rpc_handler_601407714(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			LocalizedString localizedString = null;
			if (flag)
			{
				reader.ReadValueSafe<LocalizedString>(out localizedString, default(FastBufferWriter.ForNetworkSerializable));
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			float num3;
			reader.ReadValueSafe<float>(out num3, default(FastBufferWriter.ForPrimitives));
			bool flag2;
			reader.ReadValueSafe<bool>(out flag2, default(FastBufferWriter.ForPrimitives));
			bool flag3;
			reader.ReadValueSafe<bool>(out flag3, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((TextBubble)target).ReadClientRpc(localizedString, num, num2, num3, flag2, flag3, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600144C RID: 5196 RVA: 0x00061CF0 File Offset: 0x0005FEF0
		private static void __rpc_handler_1686333811(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((TextBubble)target).Shake_ClientRpc(client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600144D RID: 5197 RVA: 0x00061D50 File Offset: 0x0005FF50
		private static void __rpc_handler_1732833465(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((TextBubble)target).CloseClientRpc(client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600144E RID: 5198 RVA: 0x00061DB0 File Offset: 0x0005FFB0
		private static void __rpc_handler_2234065250(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((TextBubble)target).ShowAlert_ClientRpc(text, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600144F RID: 5199 RVA: 0x00061E4B File Offset: 0x0006004B
		protected internal override string __getTypeName()
		{
			return "TextBubble";
		}

		// Token: 0x040010D4 RID: 4308
		private const float DEFAULT_FONT_SIZE = 22f;

		// Token: 0x040010D5 RID: 4309
		[SerializeField]
		private LocalizedString[] text;

		// Token: 0x040010D6 RID: 4310
		[SerializeField]
		private string[] defaultText;

		// Token: 0x040010D7 RID: 4311
		[SerializeField]
		private int maxCharacters;

		// Token: 0x040010D8 RID: 4312
		[SerializeField]
		private UIDialogueBubbleAnchor dialogueBubbleAnchorSource;

		// Token: 0x040010D9 RID: 4313
		internal LocalizedStringCollection runtimeText;

		// Token: 0x040010DA RID: 4314
		private readonly NetworkVariable<LocalizedString> runtimeDisplayName = new NetworkVariable<LocalizedString>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040010DB RID: 4315
		private Action onSerializedTextsChanged;

		// Token: 0x040010DC RID: 4316
		private UIDialogueBubbleAnchor dialogueBubble;

		// Token: 0x040010DD RID: 4317
		private global::UnityEngine.Vector3 dialogueBubbleOffset;

		// Token: 0x040010DE RID: 4318
		private float fontSize = 22f;

		// Token: 0x040010E0 RID: 4320
		[SerializeField]
		[HideInInspector]
		private TextBubbleReferences references;

		// Token: 0x040010E2 RID: 4322
		private EndlessScriptComponent scriptComponent;

		// Token: 0x040010E3 RID: 4323
		private object luaObject;
	}
}
