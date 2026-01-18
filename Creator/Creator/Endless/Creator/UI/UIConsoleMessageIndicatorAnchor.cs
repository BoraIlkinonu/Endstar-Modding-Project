using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000099 RID: 153
	public class UIConsoleMessageIndicatorAnchor : UIBaseAnchor
	{
		// Token: 0x0600025E RID: 606 RVA: 0x00011051 File Offset: 0x0000F251
		public static UIConsoleMessageIndicatorAnchor CreateInstance(UIConsoleMessageIndicatorAnchor prefab, Transform target, RectTransform container, SerializableGuid instanceId, global::UnityEngine.Vector3? offset = null)
		{
			UIConsoleMessageIndicatorAnchor uiconsoleMessageIndicatorAnchor = UIBaseAnchor.CreateAndInitialize<UIConsoleMessageIndicatorAnchor>(prefab, target, container, offset);
			uiconsoleMessageIndicatorAnchor.SetInstanceId(instanceId);
			return uiconsoleMessageIndicatorAnchor;
		}

		// Token: 0x0600025F RID: 607 RVA: 0x00011064 File Offset: 0x0000F264
		private void Start()
		{
			this.showConsoleButton.onClick.AddListener(new UnityAction(this.OnShowConsoleButtonClicked));
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00011084 File Offset: 0x0000F284
		public void SetInstanceId(SerializableGuid instanceId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInstanceId", new object[] { instanceId });
			}
			this.instanceId = instanceId;
			this.messages = NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId);
			int num = 0;
			int num2 = 0;
			foreach (ConsoleMessage consoleMessage in this.messages)
			{
				if (consoleMessage.LogType == LogType.Warning)
				{
					num++;
				}
				else
				{
					LogType logType = consoleMessage.LogType;
					if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception)
					{
						num2++;
					}
				}
			}
			num = Mathf.Min(99, num);
			num2 = Mathf.Min(99, num2);
			this.warningIcon.gameObject.SetActive(num > 0);
			this.warningCountLabel.text = ((num > 0) ? num.ToString() : string.Empty);
			this.errorIcon.gameObject.SetActive(num2 > 0);
			this.errorCountLabel.text = ((num2 > 0) ? num2.ToString() : string.Empty);
			this.divider.gameObject.SetActive(num > 0 && num2 > 0);
		}

		// Token: 0x06000261 RID: 609 RVA: 0x000111C0 File Offset: 0x0000F3C0
		private void OnShowConsoleButtonClicked()
		{
			UIConsoleWindowView.Display(this.messages, ConsoleScope.Instance, this.instanceId, null);
		}

		// Token: 0x040002AB RID: 683
		[SerializeField]
		private Image warningIcon;

		// Token: 0x040002AC RID: 684
		[SerializeField]
		private TextMeshProUGUI warningCountLabel;

		// Token: 0x040002AD RID: 685
		[SerializeField]
		private RectTransform divider;

		// Token: 0x040002AE RID: 686
		[SerializeField]
		private Image errorIcon;

		// Token: 0x040002AF RID: 687
		[SerializeField]
		private TextMeshProUGUI errorCountLabel;

		// Token: 0x040002B0 RID: 688
		[SerializeField]
		private UIButton showConsoleButton;

		// Token: 0x040002B1 RID: 689
		private IReadOnlyList<ConsoleMessage> messages;

		// Token: 0x040002B2 RID: 690
		private SerializableGuid instanceId;
	}
}
