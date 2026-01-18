using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIWireNodeView : UIGameObject, IPoolableT
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private HorizontalLayoutGroup labelHorizontalLayoutGroup;

	[SerializeField]
	private UITooltip descrptionTooltip;

	[SerializeField]
	private TextMeshProUGUI wireCount;

	[SerializeField]
	private UIRectTransformDictionary[] rectTransformDictionaries = new UIRectTransformDictionary[0];

	[SerializeField]
	private Image[] toggleImagesToApplyColorTo = new Image[0];

	[SerializeField]
	private DropHandler dropHandler;

	[SerializeField]
	private UIWireNodeController wireNodeController;

	[Header("Tweens")]
	[SerializeField]
	private TweenCollection onNothingSelectedTweens;

	[SerializeField]
	private TweenCollection onSelectedTweens;

	[SerializeField]
	private TweenCollection onDeselectedTweens;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private WireOrganizationData wireOrganizationData;

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	public SerializableGuid InspectedObjectId { get; private set; } = SerializableGuid.Empty;

	public EndlessEventInfo NodeEvent { get; private set; }

	public int NodeIndex { get; private set; } = -1;

	public string AssemblyQualifiedTypeName { get; private set; }

	public UIWiringObjectInspectorView ParentWiringObjectInspectorView { get; private set; }

	public bool IsOnLeftOfScreen => ParentWiringObjectInspectorView.IsOnLeftOfScreen;

	public Vector3 WirePoint => wireCount.rectTransform.position;

	public string MemberName => NodeEvent.MemberName;

	public void OnSpawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawn");
		}
	}

	public void OnDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDespawn");
		}
		ToggleOnNothingSelectedTweens(state: false);
		ToggleSelectedVisuals(state: false);
		dropHandler.gameObject.SetActive(value: false);
		InspectedObjectId = SerializableGuid.Empty;
		NodeEvent = null;
		NodeIndex = -1;
		wireOrganizationData = null;
		wireNodeController.OnDespawn();
	}

	public void Initialize(SerializableGuid inspectedObjectId, EndlessEventInfo nodeEvent, int nodeIndex, string assemblyQualifiedTypeName, UIWiringObjectInspectorView parentWiringObjectInspectorView, Color color, bool showComponentName)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", inspectedObjectId, nodeEvent, assemblyQualifiedTypeName, parentWiringObjectInspectorView, color, showComponentName);
		}
		InspectedObjectId = inspectedObjectId;
		NodeEvent = nodeEvent;
		NodeIndex = nodeIndex;
		AssemblyQualifiedTypeName = assemblyQualifiedTypeName;
		ParentWiringObjectInspectorView = parentWiringObjectInspectorView;
		bool flag = !nodeEvent.Description.IsNullOrEmptyOrWhiteSpace();
		string text = "<color=#D1D5D9>" + nodeEvent.MemberName + "</color>";
		if (showComponentName)
		{
			string simpleTypeName = GetSimpleTypeName(assemblyQualifiedTypeName);
			text = text + " <color=#808080>(" + simpleTypeName + ")</color>";
		}
		label.text = text;
		label.color = color;
		label.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
		label.raycastTarget = flag;
		descrptionTooltip.enabled = flag;
		descrptionTooltip.SetTooltip(nodeEvent.Description);
		LoadWireOrganizationData();
		UpdateWireCount();
		for (int i = 0; i < toggleImagesToApplyColorTo.Length; i++)
		{
			toggleImagesToApplyColorTo[i].color = new Color(color.r, color.g, color.b, toggleImagesToApplyColorTo[i].color.a);
		}
	}

	public void SetIsOnLeftOfScreen(bool isOnLeftOfScreen)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetIsOnLeftOfScreen", isOnLeftOfScreen);
		}
		label.alignment = (isOnLeftOfScreen ? TextAlignmentOptions.Left : TextAlignmentOptions.Right);
		labelHorizontalLayoutGroup.childAlignment = (isOnLeftOfScreen ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
		for (int i = 0; i < rectTransformDictionaries.Length; i++)
		{
			rectTransformDictionaries[i].Apply(isOnLeftOfScreen ? "left" : "right");
		}
	}

	public void ToggleOnNothingSelectedTweens(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleOnNothingSelectedTweens", state);
		}
		if (state)
		{
			onNothingSelectedTweens.Tween();
		}
		else
		{
			onNothingSelectedTweens.Cancel();
		}
	}

	public void ToggleSelectedVisuals(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleSelectedVisuals", state);
		}
		if (state)
		{
			onSelectedTweens.Tween();
		}
		else
		{
			onDeselectedTweens.Tween();
		}
	}

	public void UpdateWireCount()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateWireCount");
		}
		IReadOnlyList<WireBundle> readOnlyList = (ParentWiringObjectInspectorView.IsReceiver ? WiringUtilities.GetWiresWithAReceiverOf(InspectedObjectId, NodeEvent.MemberName) : WiringUtilities.GetWiresEmittingFrom(InspectedObjectId, NodeEvent.MemberName));
		bool flag = readOnlyList.Count > 0;
		wireCount.gameObject.SetActive(flag);
		if (flag)
		{
			wireCount.text = readOnlyList.Count.ToString();
		}
	}

	public void ToggleDropHandler(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleDropHandler", state);
		}
		dropHandler.gameObject.SetActive(state);
	}

	private void LoadWireOrganizationData()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadWireOrganizationData");
		}
		SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(InspectedObjectId);
		if (!TryGetScript(assetIdFromInstanceId, out var script))
		{
			if (verboseLogging)
			{
				Debug.Log("Disabled: false", this);
			}
			SetWireOrganizationDataAndActivate(null);
			return;
		}
		int componentId = (string.IsNullOrEmpty(AssemblyQualifiedTypeName) ? (-1) : EndlessTypeMapping.Instance.GetTypeId(AssemblyQualifiedTypeName));
		WireOrganizationData wireOrganizationData = (ParentWiringObjectInspectorView.IsReceiver ? script.GetWireOrganizationReceiverData(NodeEvent.MemberName, componentId) : script.GetWireOrganizationEventData(NodeEvent.MemberName, componentId));
		SetWireOrganizationDataAndActivate(wireOrganizationData);
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "Disabled", wireOrganizationData.Disabled), this);
		}
	}

	private bool TryGetScript(SerializableGuid assetId, out Script script)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "TryGetScript", assetId);
		}
		script = null;
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
		if (runtimePropInfo.EndlessProp == null)
		{
			Debug.LogWarning(string.Format("{0}: EndlessProp is null for asset {1}", "LoadWireOrganizationData", assetId), this);
			return false;
		}
		if (runtimePropInfo.EndlessProp.ScriptComponent == null)
		{
			Debug.LogWarning(string.Format("{0}: ScriptComponent is null for asset {1}", "LoadWireOrganizationData", assetId), this);
			return false;
		}
		if (runtimePropInfo.EndlessProp.ScriptComponent.Script == null)
		{
			Debug.LogWarning(string.Format("{0}: Script is null for asset {1}", "LoadWireOrganizationData", assetId), this);
			return false;
		}
		script = runtimePropInfo.EndlessProp.ScriptComponent.Script;
		return true;
	}

	private void SetWireOrganizationDataAndActivate(WireOrganizationData data)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetWireOrganizationDataAndActivate", data);
		}
		wireOrganizationData = data;
		base.gameObject.SetActive(data != null && !data.Disabled);
	}

	private string GetSimpleTypeName(string assemblyQualifiedTypeName)
	{
		if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
		{
			return "Lua";
		}
		string text = assemblyQualifiedTypeName.Split(',')[0];
		int num = text.LastIndexOf('.');
		if (num < 0)
		{
			return text;
		}
		return text.Substring(num + 1);
	}
}
