using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIConsoleWindowView : UIBaseWindowView
{
	private const string MESSAGES_KEY = "messages";

	private const string SCOPE = "scope";

	private const string ID = "id";

	[SerializeField]
	private UIConsoleMessageListModel messageListModel;

	[SerializeField]
	private UIButton scopeButton;

	private ConsoleScope currentScope;

	private SerializableGuid currentId;

	protected override void Start()
	{
		base.Start();
		scopeButton.onClick.AddListener(OnScopeCleared);
		NetworkBehaviourSingleton<UserScriptingConsole>.Instance.OnMessageReceived += OnMessageReceived;
		NetworkBehaviourSingleton<UserScriptingConsole>.Instance.OnMessagesCleared += OnMessagesCleared;
	}

	private void OnMessagesCleared()
	{
		messageListModel.Set(new List<UIConsoleMessageModel>(), triggerEvents: true);
	}

	private void OnMessageReceived(ConsoleMessage newMessage, List<ConsoleMessage> allMessages)
	{
		List<UIConsoleMessageModel> list = new List<UIConsoleMessageModel>();
		if (currentId == SerializableGuid.Empty || currentScope == ConsoleScope.All)
		{
			list = allMessages.Select((ConsoleMessage message) => new UIConsoleMessageModel
			{
				Message = message
			}).ToList();
		}
		else
		{
			switch (currentScope)
			{
			case ConsoleScope.Instance:
				list = (from message in allMessages
					where message.InstanceId == currentId
					select new UIConsoleMessageModel
					{
						Message = message
					}).ToList();
				break;
			case ConsoleScope.Asset:
				list = (from message in allMessages
					where message.AssetId == currentId
					select new UIConsoleMessageModel
					{
						Message = message
					}).ToList();
				break;
			}
		}
		messageListModel.Set(list, triggerEvents: true);
	}

	private void OnScopeCleared()
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{
				"messages",
				NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessages()
			},
			{
				"scope",
				ConsoleScope.All
			},
			{
				"id",
				SerializableGuid.Empty
			}
		};
		Initialize(supplementalData);
	}

	public static UIConsoleWindowView Display(IReadOnlyList<ConsoleMessage> messagesToDisplay, ConsoleScope scope, SerializableGuid serializableGuid, Transform parent)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "messages", messagesToDisplay },
			{ "scope", scope },
			{ "id", serializableGuid }
		};
		return (UIConsoleWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIConsoleWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Debug.Log("UIConsoleWindowView.Initialize()");
		if (!supplementalData.TryGetValue("messages", out var value))
		{
			Debug.Log("UIConsoleWindowView.Initialize: No message object to display.");
			Close();
			return;
		}
		if (!(value is IReadOnlyList<ConsoleMessage> source))
		{
			Debug.Log("UIConsoleWindowView.Initialize: Message object existed in supplemental data, but failed to cast to IReadOnlyList<ConsoleMessage>.");
			Close();
			return;
		}
		if (!supplementalData.TryGetValue("scope", out var value2))
		{
			Debug.Log("UIConsoleWindowView.Initialize: No scope set for console window.");
			Close();
			return;
		}
		if (!(value2 is ConsoleScope consoleScope))
		{
			Debug.Log("UIConsoleWindowView.Initialize: scopeObj could not be cast into a valid scope.");
			Close();
			return;
		}
		if (consoleScope != ConsoleScope.All)
		{
			if (!supplementalData.TryGetValue("id", out var value3))
			{
				Debug.Log("UIConsoleWindowView.Initialize: No serializable guid set to inspect in console.");
				Close();
				return;
			}
			if (!(value3 is SerializableGuid serializableGuid))
			{
				Debug.Log("UIConsoleWindowView.Initialize: serializable guid could not be determined from id value.");
				Close();
				return;
			}
			currentId = serializableGuid;
		}
		else
		{
			currentId = SerializableGuid.Empty;
		}
		List<UIConsoleMessageModel> list = source.Select((ConsoleMessage message) => new UIConsoleMessageModel
		{
			Message = message
		}).ToList();
		currentScope = consoleScope;
		TextMeshProUGUI componentInChildren = scopeButton.GetComponentInChildren<TextMeshProUGUI>();
		switch (currentScope)
		{
		case ConsoleScope.Instance:
		{
			scopeButton.gameObject.SetActive(value: true);
			if (componentInChildren != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(currentId, out var propEntry))
			{
				componentInChildren.text = "Prop Instance: " + propEntry.Label;
			}
			break;
		}
		case ConsoleScope.Asset:
		{
			scopeButton.gameObject.SetActive(value: true);
			if (componentInChildren != null && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(currentId, out var metadata))
			{
				componentInChildren.text = "Asset: " + metadata.PropData.Name;
			}
			break;
		}
		default:
			scopeButton.gameObject.SetActive(value: false);
			break;
		}
		messageListModel.Set(list, triggerEvents: true);
	}
}
