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

namespace Endless.Gameplay;

public class TextBubble : EndlessNetworkBehaviour, IPersistantStateSubscriber, IAwakeSubscriber, IComponentBase, IScriptInjector
{
	private const float DEFAULT_FONT_SIZE = 22f;

	[SerializeField]
	private LocalizedString[] text;

	[SerializeField]
	private string[] defaultText;

	[SerializeField]
	private int maxCharacters;

	[SerializeField]
	private UIDialogueBubbleAnchor dialogueBubbleAnchorSource;

	internal LocalizedStringCollection runtimeText;

	private readonly NetworkVariable<LocalizedString> runtimeDisplayName = new NetworkVariable<LocalizedString>();

	private Action onSerializedTextsChanged;

	private UIDialogueBubbleAnchor dialogueBubble;

	private UnityEngine.Vector3 dialogueBubbleOffset;

	private float fontSize = 22f;

	[SerializeField]
	[HideInInspector]
	private TextBubbleReferences references;

	private EndlessScriptComponent scriptComponent;

	private object luaObject;

	public LocalizedString[] Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
			onSerializedTextsChanged?.Invoke();
		}
	}

	public LocalizedString DisplayName
	{
		get
		{
			return runtimeDisplayName.Value;
		}
		set
		{
			runtimeDisplayName.Value = value;
		}
	}

	public bool ShouldSaveAndLoad { get; set; } = true;

	public float ShakeDuration => 0.5f;

	public Type ComponentReferenceType => typeof(TextBubbleReferences);

	public WorldObject WorldObject { get; private set; }

	public object LuaObject => luaObject ?? (luaObject = new Endless.Gameplay.LuaInterfaces.TextBubble(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.TextBubble);

	private void Awake()
	{
		onSerializedTextsChanged = (Action)Delegate.Combine(onSerializedTextsChanged, new Action(HandleSerializedTextsChanged));
		SetText();
	}

	private void HandleSerializedTextsChanged()
	{
		if (base.IsHost)
		{
			UpdateText(text);
		}
	}

	private void ValidateStringLengths(IList<string> text)
	{
		for (int i = 0; i < text.Count; i++)
		{
			if (text[i].Length > maxCharacters)
			{
				text[i] = text[i].Substring(0, maxCharacters);
			}
		}
	}

	internal void Display(int textIndex, bool showProgress = true, bool showInteract = true)
	{
		if (textIndex >= runtimeText.Length)
		{
			Close();
		}
		else
		{
			ReadClientRpc(runtimeText.stringPairs[textIndex], textIndex, runtimeText.Length, fontSize, showProgress, showInteract);
		}
	}

	internal void DisplayForTarget(Context target, int textIndex, bool showProgress = true, bool showInteract = true)
	{
		if (target != null && target.IsPlayer())
		{
			if (textIndex >= runtimeText.Length)
			{
				CloseForTarget(target);
				return;
			}
			ClientRpcParams rpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { target.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			ReadClientRpc(runtimeText.stringPairs[textIndex], textIndex, runtimeText.Length, fontSize, showProgress, showInteract, rpcParams);
		}
	}

	internal void DisplayWithDuration(int textIndex, float duration, bool showProgress = true, bool showInteract = true)
	{
		Display(textIndex, showProgress, showInteract);
		StartCoroutine(WaitToCloseRoutine(duration));
	}

	internal void DisplayForTargetWithDuration(Context target, int textIndex, float duration, bool showProgress = true, bool showInteract = true)
	{
		if (target != null && target.IsPlayer())
		{
			if (textIndex >= runtimeText.Length)
			{
				CloseForTarget(target);
				return;
			}
			ClientRpcParams rpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { target.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			ReadClientRpc(runtimeText.stringPairs[textIndex], textIndex, runtimeText.Length, fontSize, showProgress, showInteract, rpcParams);
			StartCoroutine(WaitToCloseRoutineForTarget(target, duration));
		}
	}

	private IEnumerator WaitToCloseRoutine(float duration)
	{
		yield return new WaitForSeconds(duration);
		Close();
	}

	private IEnumerator WaitToCloseRoutineForTarget(Context target, float duration)
	{
		yield return new WaitForSeconds(duration);
		CloseForTarget(target);
	}

	[ClientRpc]
	private void ReadClientRpc(LocalizedString textToDisplay, int dialogueIndex, int maximumDialogueIndex, float currentFontSize, bool showIndex = true, bool showInteract = true, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(601407714u, rpcParams, RpcDelivery.Reliable);
			bool value = textToDisplay != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in textToDisplay, default(FastBufferWriter.ForNetworkSerializable));
			}
			BytePacker.WriteValueBitPacked(bufferWriter, dialogueIndex);
			BytePacker.WriteValueBitPacked(bufferWriter, maximumDialogueIndex);
			bufferWriter.WriteValueSafe(in currentFontSize, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in showIndex, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in showInteract, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 601407714u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			string localizedString = textToDisplay.GetLocalizedString();
			ValidateStringLengths(new string[1] { localizedString });
			if (dialogueBubble == null)
			{
				string displayName = ((DisplayName != null) ? DisplayName.GetLocalizedString() : "");
				dialogueBubble = UIDialogueBubbleAnchor.CreateInstance(dialogueBubbleAnchorSource, references.Anchor, MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer, displayName, dialogueBubbleOffset);
			}
			bool skipOnNextTextTween = dialogueIndex == 0;
			dialogueBubble.DisplayText(localizedString, dialogueIndex, maximumDialogueIndex, skipOnNextTextTween, currentFontSize, showIndex, showInteract);
		}
	}

	[ClientRpc]
	private void Shake_ClientRpc(ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(1686333811u, rpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 1686333811u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((bool)dialogueBubble)
			{
				dialogueBubble.Shake();
			}
		}
	}

	internal void Close()
	{
		CloseClientRpc();
	}

	internal void CloseForTarget(Context context)
	{
		if (context != null && context.IsPlayer() && (bool)context.WorldObject.NetworkObject)
		{
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new List<ulong> { context.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			CloseClientRpc(clientRpcParams);
		}
	}

	internal int GetTextLength()
	{
		return runtimeText.Length;
	}

	public void UpdateText(LocalizedStringCollection newText)
	{
		runtimeText = newText;
	}

	public void UpdateText(string[] strings)
	{
		LocalizedString[] array = new LocalizedString[strings.Length];
		for (int i = 0; i < strings.Length; i++)
		{
			array[i] = new LocalizedString();
			array[i].SetStringValue(strings[i], LocalizedString.ActiveLanguage);
		}
		UpdateText(new LocalizedStringCollection
		{
			stringPairs = array
		});
	}

	public void UpdateText(LuaTable newText)
	{
		LocalizedString[] array = new LocalizedString[newText.Keys.Count];
		int num = 0;
		foreach (object value in newText.Values)
		{
			Type type = value.GetType();
			if (type == typeof(LocalizedString))
			{
				array[num] = (LocalizedString)value;
			}
			else
			{
				if (!(type == typeof(LuaTable)))
				{
					scriptComponent.LogError("Inner value must be a lua table or localized string");
					return;
				}
				LocalizedString localizedString = new LocalizedString();
				LuaTable luaTable = (LuaTable)value;
				foreach (object key in luaTable.Keys)
				{
					if (key.GetType() != typeof(long))
					{
						scriptComponent.LogError("Inner indexes must be a language");
						return;
					}
					Language language = (Language)(long)key;
					object obj = luaTable[key];
					if (obj.GetType() != typeof(string))
					{
						scriptComponent.LogError("Inner values must be a string");
						return;
					}
					string newValue = (string)obj;
					localizedString.SetStringValue(newValue, language);
				}
				array[num] = localizedString;
			}
			num++;
		}
		UpdateText(array);
	}

	[ClientRpc]
	private void CloseClientRpc(ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(1732833465u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 1732833465u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((bool)dialogueBubble)
			{
				dialogueBubble.Close();
			}
			dialogueBubble = null;
		}
	}

	public void SetDisplayName(Context _, LocalizedString newName)
	{
		DisplayName = newName;
	}

	public void SetText(Context _, string[] newText)
	{
		ResetFontSize();
		UpdateText(newText);
	}

	public void SetLocalizedText(Context _, LocalizedString[] text)
	{
		ResetFontSize();
		UpdateText(text);
	}

	public object GetSaveState()
	{
		return new object[2] { runtimeDisplayName.Value, runtimeText };
	}

	public void LoadState(object loadedState)
	{
		if (loadedState != null)
		{
			object[] array = (object[])loadedState;
			runtimeDisplayName.Value = (LocalizedString)array[0];
			UpdateText((LocalizedStringCollection)array[1]);
		}
	}

	public void SetFontSize(float newFontSize)
	{
		fontSize = newFontSize;
	}

	public void ResetFontSize()
	{
		fontSize = 22f;
	}

	public void ShakeForTarget(Context target)
	{
		ClientRpcParams clientRpcParams = GetClientRpcParams(target);
		Shake_ClientRpc(clientRpcParams);
	}

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

	public void ShakeForAll()
	{
		Shake_ClientRpc();
	}

	public void ShowAlert(string displayText)
	{
		ShowAlert_ClientRpc(displayText);
	}

	public void ShowAlertForTarget(Context target, string displayText)
	{
		ClientRpcParams clientRpcParams = GetClientRpcParams(target);
		ShowAlert_ClientRpc(displayText, clientRpcParams);
	}

	[ClientRpc]
	private void ShowAlert_ClientRpc(string displayText, ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(2234065250u, clientRpcParams, RpcDelivery.Reliable);
			bool value = displayText != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(displayText);
			}
			__endSendClientRpc(ref bufferWriter, 2234065250u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((bool)dialogueBubble)
			{
				dialogueBubble.ShowAlert(displayText);
			}
		}
	}

	public void EndlessAwake()
	{
		if (base.IsHost)
		{
			UpdateText(text);
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (TextBubbleReferences)referenceBase;
	}

	private void SetText()
	{
		text = new LocalizedString[text.Length];
		for (int i = 0; i < defaultText.Length; i++)
		{
			text[i] = new LocalizedString();
			text[i].SetStringValue(defaultText[i], LocalizedString.ActiveLanguage);
		}
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (runtimeDisplayName == null)
		{
			throw new Exception("TextBubble.runtimeDisplayName cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		runtimeDisplayName.Initialize(this);
		__nameNetworkVariable(runtimeDisplayName, "runtimeDisplayName");
		NetworkVariableFields.Add(runtimeDisplayName);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(601407714u, __rpc_handler_601407714, "ReadClientRpc");
		__registerRpc(1686333811u, __rpc_handler_1686333811, "Shake_ClientRpc");
		__registerRpc(1732833465u, __rpc_handler_1732833465, "CloseClientRpc");
		__registerRpc(2234065250u, __rpc_handler_2234065250, "ShowAlert_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_601407714(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			LocalizedString value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ByteUnpacker.ReadValueBitPacked(reader, out int value3);
			ByteUnpacker.ReadValueBitPacked(reader, out int value4);
			reader.ReadValueSafe(out float value5, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value7, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((TextBubble)target).ReadClientRpc(value2, value3, value4, value5, value6, value7, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1686333811(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((TextBubble)target).Shake_ClientRpc(client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1732833465(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((TextBubble)target).CloseClientRpc(client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2234065250(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
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
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((TextBubble)target).ShowAlert_ClientRpc(s, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "TextBubble";
	}
}
