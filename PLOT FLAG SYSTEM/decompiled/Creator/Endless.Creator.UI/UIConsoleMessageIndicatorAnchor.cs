using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIConsoleMessageIndicatorAnchor : UIBaseAnchor
{
	[SerializeField]
	private Image warningIcon;

	[SerializeField]
	private TextMeshProUGUI warningCountLabel;

	[SerializeField]
	private RectTransform divider;

	[SerializeField]
	private Image errorIcon;

	[SerializeField]
	private TextMeshProUGUI errorCountLabel;

	[SerializeField]
	private UIButton showConsoleButton;

	private IReadOnlyList<ConsoleMessage> messages;

	private SerializableGuid instanceId;

	public static UIConsoleMessageIndicatorAnchor CreateInstance(UIConsoleMessageIndicatorAnchor prefab, Transform target, RectTransform container, SerializableGuid instanceId, UnityEngine.Vector3? offset = null)
	{
		UIConsoleMessageIndicatorAnchor uIConsoleMessageIndicatorAnchor = UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
		uIConsoleMessageIndicatorAnchor.SetInstanceId(instanceId);
		return uIConsoleMessageIndicatorAnchor;
	}

	private void Start()
	{
		showConsoleButton.onClick.AddListener(OnShowConsoleButtonClicked);
	}

	public void SetInstanceId(SerializableGuid instanceId)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInstanceId", instanceId);
		}
		this.instanceId = instanceId;
		messages = NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId);
		int num = 0;
		int num2 = 0;
		foreach (ConsoleMessage message in messages)
		{
			if (message.LogType == LogType.Warning)
			{
				num++;
				continue;
			}
			LogType logType = message.LogType;
			if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception)
			{
				num2++;
			}
		}
		num = Mathf.Min(99, num);
		num2 = Mathf.Min(99, num2);
		warningIcon.gameObject.SetActive(num > 0);
		warningCountLabel.text = ((num > 0) ? num.ToString() : string.Empty);
		errorIcon.gameObject.SetActive(num2 > 0);
		errorCountLabel.text = ((num2 > 0) ? num2.ToString() : string.Empty);
		divider.gameObject.SetActive(num > 0 && num2 > 0);
	}

	private void OnShowConsoleButtonClicked()
	{
		UIConsoleWindowView.Display(messages, ConsoleScope.Instance, instanceId, null);
	}
}
