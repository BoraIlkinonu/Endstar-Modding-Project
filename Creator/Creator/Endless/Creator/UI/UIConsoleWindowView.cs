using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002CB RID: 715
	public class UIConsoleWindowView : UIBaseWindowView
	{
		// Token: 0x06000C0F RID: 3087 RVA: 0x00039B70 File Offset: 0x00037D70
		protected override void Start()
		{
			base.Start();
			this.scopeButton.onClick.AddListener(new UnityAction(this.OnScopeCleared));
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.OnMessageReceived += this.OnMessageReceived;
			NetworkBehaviourSingleton<UserScriptingConsole>.Instance.OnMessagesCleared += this.OnMessagesCleared;
		}

		// Token: 0x06000C10 RID: 3088 RVA: 0x00039BCB File Offset: 0x00037DCB
		private void OnMessagesCleared()
		{
			this.messageListModel.Set(new List<UIConsoleMessageModel>(), true);
		}

		// Token: 0x06000C11 RID: 3089 RVA: 0x00039BE0 File Offset: 0x00037DE0
		private void OnMessageReceived(ConsoleMessage newMessage, List<ConsoleMessage> allMessages)
		{
			List<UIConsoleMessageModel> list = new List<UIConsoleMessageModel>();
			if (this.currentId == SerializableGuid.Empty || this.currentScope == ConsoleScope.All)
			{
				list = allMessages.Select((ConsoleMessage message) => new UIConsoleMessageModel
				{
					Message = message
				}).ToList<UIConsoleMessageModel>();
			}
			else
			{
				ConsoleScope consoleScope = this.currentScope;
				if (consoleScope != ConsoleScope.Instance)
				{
					if (consoleScope == ConsoleScope.Asset)
					{
						list = (from message in allMessages
							where message.AssetId == this.currentId
							select new UIConsoleMessageModel
							{
								Message = message
							}).ToList<UIConsoleMessageModel>();
					}
				}
				else
				{
					list = (from message in allMessages
						where message.InstanceId == this.currentId
						select new UIConsoleMessageModel
						{
							Message = message
						}).ToList<UIConsoleMessageModel>();
				}
			}
			this.messageListModel.Set(list, true);
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x00039CD8 File Offset: 0x00037ED8
		private void OnScopeCleared()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
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
			this.Initialize(dictionary);
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x00039D30 File Offset: 0x00037F30
		public static UIConsoleWindowView Display(IReadOnlyList<ConsoleMessage> messagesToDisplay, ConsoleScope scope, SerializableGuid serializableGuid, Transform parent)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "messages", messagesToDisplay },
				{ "scope", scope },
				{ "id", serializableGuid }
			};
			return (UIConsoleWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIConsoleWindowView>(parent, dictionary);
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x00039D84 File Offset: 0x00037F84
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			Debug.Log("UIConsoleWindowView.Initialize()");
			object obj;
			if (!supplementalData.TryGetValue("messages", out obj))
			{
				Debug.Log("UIConsoleWindowView.Initialize: No message object to display.");
				this.Close();
				return;
			}
			IReadOnlyList<ConsoleMessage> readOnlyList = obj as IReadOnlyList<ConsoleMessage>;
			if (readOnlyList == null)
			{
				Debug.Log("UIConsoleWindowView.Initialize: Message object existed in supplemental data, but failed to cast to IReadOnlyList<ConsoleMessage>.");
				this.Close();
				return;
			}
			object obj2;
			if (!supplementalData.TryGetValue("scope", out obj2))
			{
				Debug.Log("UIConsoleWindowView.Initialize: No scope set for console window.");
				this.Close();
				return;
			}
			if (obj2 is ConsoleScope)
			{
				ConsoleScope consoleScope = (ConsoleScope)obj2;
				if (consoleScope != ConsoleScope.All)
				{
					object obj3;
					if (!supplementalData.TryGetValue("id", out obj3))
					{
						Debug.Log("UIConsoleWindowView.Initialize: No serializable guid set to inspect in console.");
						this.Close();
						return;
					}
					if (!(obj3 is SerializableGuid))
					{
						Debug.Log("UIConsoleWindowView.Initialize: serializable guid could not be determined from id value.");
						this.Close();
						return;
					}
					SerializableGuid serializableGuid = (SerializableGuid)obj3;
					this.currentId = serializableGuid;
				}
				else
				{
					this.currentId = SerializableGuid.Empty;
				}
				List<UIConsoleMessageModel> list = readOnlyList.Select((ConsoleMessage message) => new UIConsoleMessageModel
				{
					Message = message
				}).ToList<UIConsoleMessageModel>();
				this.currentScope = consoleScope;
				TextMeshProUGUI componentInChildren = this.scopeButton.GetComponentInChildren<TextMeshProUGUI>();
				ConsoleScope consoleScope2 = this.currentScope;
				if (consoleScope2 != ConsoleScope.Instance)
				{
					if (consoleScope2 != ConsoleScope.Asset)
					{
						this.scopeButton.gameObject.SetActive(false);
					}
					else
					{
						this.scopeButton.gameObject.SetActive(true);
						PropLibrary.RuntimePropInfo runtimePropInfo;
						if (componentInChildren != null && MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.currentId, out runtimePropInfo))
						{
							componentInChildren.text = "Asset: " + runtimePropInfo.PropData.Name;
						}
					}
				}
				else
				{
					this.scopeButton.gameObject.SetActive(true);
					PropEntry propEntry;
					if (componentInChildren != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(this.currentId, out propEntry))
					{
						componentInChildren.text = "Prop Instance: " + propEntry.Label;
					}
				}
				this.messageListModel.Set(list, true);
				return;
			}
			Debug.Log("UIConsoleWindowView.Initialize: scopeObj could not be cast into a valid scope.");
			this.Close();
		}

		// Token: 0x04000A6B RID: 2667
		private const string MESSAGES_KEY = "messages";

		// Token: 0x04000A6C RID: 2668
		private const string SCOPE = "scope";

		// Token: 0x04000A6D RID: 2669
		private const string ID = "id";

		// Token: 0x04000A6E RID: 2670
		[SerializeField]
		private UIConsoleMessageListModel messageListModel;

		// Token: 0x04000A6F RID: 2671
		[SerializeField]
		private UIButton scopeButton;

		// Token: 0x04000A70 RID: 2672
		private ConsoleScope currentScope;

		// Token: 0x04000A71 RID: 2673
		private SerializableGuid currentId;
	}
}
