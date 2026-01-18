using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using TMPro;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class InspectorToolErrorIndicator : PooledGameObject
{
	private SerializableGuid activeInstanceId;

	[SerializeField]
	private float multipleOffset = 0.3f;

	[SerializeField]
	private GameObject warningLine;

	[SerializeField]
	private TextMeshPro warningCountLabel;

	[SerializeField]
	private GameObject errorLine;

	[SerializeField]
	private TextMeshPro errorCountLabel;

	public void InitializeFor(SerializableGuid instanceId)
	{
		activeInstanceId = instanceId;
		IReadOnlyList<ConsoleMessage> consoleMessagesForInstanceId = NetworkBehaviourSingleton<UserScriptingConsole>.Instance.GetConsoleMessagesForInstanceId(instanceId);
		int num = 0;
		int num2 = 0;
		foreach (ConsoleMessage item in consoleMessagesForInstanceId)
		{
			if (item.LogType == LogType.Warning)
			{
				num++;
				continue;
			}
			LogType logType = item.LogType;
			if (logType == LogType.Assert || logType == LogType.Error || logType == LogType.Exception)
			{
				num2++;
			}
		}
		warningLine.SetActive(num > 0);
		warningCountLabel.text = num.ToString();
		errorLine.SetActive(num > 0);
		errorCountLabel.text = num2.ToString();
	}

	public void Release()
	{
		activeInstanceId = SerializableGuid.Empty;
	}

	private void OnMouseDown()
	{
		Debug.Log("Hey! We had a mouse down here!");
		InspectorTool inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Inspector) as InspectorTool;
		if ((bool)inspectorTool)
		{
			inspectorTool.ShowConsoleLogFor(activeInstanceId);
		}
		else
		{
			Debug.LogError("Failed to acquire the Inspector Tool from the tool manager! This should *never* happen.");
		}
	}

	private void Update()
	{
		base.transform.LookAt(Camera.main.transform.position, UnityEngine.Vector3.up);
	}
}
