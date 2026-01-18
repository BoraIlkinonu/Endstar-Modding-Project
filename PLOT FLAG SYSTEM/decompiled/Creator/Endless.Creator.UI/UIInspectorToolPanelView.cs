using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI;

public class UIInspectorToolPanelView : UIDockableToolPanelView<InspectorTool>, IBackable
{
	private static readonly Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum> { 
	{
		typeof(IEnumerable),
		UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized
	} };

	[Header("UIInspectorToolPanelView")]
	[SerializeField]
	private UIInputField label;

	[SerializeField]
	private TextMeshProUGUI definitionNameText;

	[SerializeField]
	private string definitionNamePreText = "Name: ";

	[SerializeField]
	private UIDisplayAndHideHandler eyeDropperCanvasDisplayAndHideHandler;

	[SerializeField]
	private UIText eyeDropperText;

	[SerializeField]
	[TextArea]
	private string eyeDropperTextTemplate = "Click on a {0} in the world to set it as the referenced object.\n\n\n\nThe selected object will then become the one acted upon. You can remove the reference or replace it at any time later.";

	[SerializeField]
	private UIButton reAddToGameLibraryButton;

	[SerializeField]
	private UIIEnumerablePresenter propertiesPresenter;

	[SerializeField]
	private UIIEnumerableStraightVirtualizedView iEnumerableStraightVirtualizedView;

	public SerializableGuid AssetId { get; private set; } = SerializableGuid.Empty;

	public SerializableGuid InstanceId { get; private set; } = SerializableGuid.Empty;

	public PropEntry PropEntry { get; private set; }

	protected override bool DisplayOnToolChangeMatchToToolType => false;

	protected override float ListSize => iEnumerableStraightVirtualizedView.ContentSize;

	protected override void Start()
	{
		base.Start();
		Tool.OnItemInspected.AddListener(View);
		Tool.OnItemEyeDropped.AddListener(OnItemEyeDropped);
		Tool.OnStateChanged.AddListener(OnInspectorToolStateChanged);
		eyeDropperCanvasDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		iEnumerableStraightVirtualizedView.OnEnumerableCountChanged += Resize;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		iEnumerableStraightVirtualizedView.OnEnumerableCountChanged -= Resize;
	}

	public void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		switch (Tool.State)
		{
		case InspectorTool.States.Inspect:
			if (InputManager.InputUnrestricted)
			{
				Tool.DeselectProp();
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			break;
		case InspectorTool.States.EyeDropper:
			Tool.SetStateToInspect();
			break;
		default:
			DebugUtility.LogNoEnumSupportError(this, Tool.State);
			break;
		}
	}

	private void Resize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Resize");
		}
		iEnumerableStraightVirtualizedView.Resize(keepPosition: true);
		iEnumerableStraightVirtualizedView.ReapplyItemSizes();
		TweenToMaxPanelHeight();
	}

	public override void Display()
	{
		base.Display();
		if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
		AssetId = SerializableGuid.Empty;
		InstanceId = SerializableGuid.Empty;
		PropEntry = null;
		propertiesPresenter.Clear();
	}

	public void Review()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Review");
		}
		View(AssetId, InstanceId, PropEntry);
	}

	private async void View(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", assetId, instanceId, (propEntry == null) ? "null" : propEntry.Label);
		}
		if (assetId.IsEmpty)
		{
			if (Tool.State != InspectorTool.States.EyeDropper)
			{
				Clear();
				Hide();
			}
		}
		else if (!(assetId == AssetId) || !(instanceId == InstanceId))
		{
			AssetId = assetId;
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
			bool flag = (await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID)).Roles.GetRoleForUserId(EndlessServices.Instance.CloudService.ActiveUserId).IsGreaterThan(Roles.Viewer);
			label.interactable = flag;
			iEnumerableStraightVirtualizedView.SetInteractable(flag);
			iEnumerableStraightVirtualizedView.SetChildIEnumerableCanAddAndRemoveItems(flag);
			bool active = runtimePropInfo.IsMissingObject && flag;
			reAddToGameLibraryButton.gameObject.SetActive(active);
			Clear();
			InstanceId = instanceId;
			PropEntry = propEntry;
			if (propEntry == null)
			{
				DebugUtility.LogException(new NullReferenceException("propEntry is null"), this);
			}
			else
			{
				label.text = propEntry.Label;
			}
			definitionNameText.text = definitionNamePreText + runtimePropInfo.PropData.Name;
			List<object> properties = GetProperties(runtimePropInfo, propEntry);
			iEnumerableStraightVirtualizedView.SetScrollPositionImmediately(0f);
			Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(typeStyleOverrideDictionary);
			propertiesPresenter.IEnumerableView.SetTypeStyleOverrideDictionary(dictionary);
			propertiesPresenter.SetModel(properties, triggerOnModelChanged: true);
			Display();
			TweenToMaxPanelHeight();
		}
	}

	private void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		propertiesPresenter.Clear();
	}

	private List<object> GetProperties(PropLibrary.RuntimePropInfo runtimePropInfo, PropEntry propEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetProperties", runtimePropInfo.EndlessProp.Prop.Name, propEntry.Label);
		}
		List<object> list = new List<object>();
		EndlessScriptComponent scriptComponent = runtimePropInfo.EndlessProp.ScriptComponent;
		if (scriptComponent == null)
		{
			DebugUtility.LogException(new NullReferenceException("endlessScriptComponent is null"), this);
			return list;
		}
		if (scriptComponent.Script == null)
		{
			DebugUtility.LogException(new NullReferenceException("endlessScriptComponent.Script is null"), this);
			return list;
		}
		OrderedDictionary orderedDictionary = new OrderedDictionary();
		Script script = scriptComponent.Script;
		Dictionary<Tuple<int, string>, UIInspectorPropertyModel> dictionary = new Dictionary<Tuple<int, string>, UIInspectorPropertyModel>();
		foreach (InspectorScriptValue inspectorValue in script.InspectorValues)
		{
			if (!script.GetInspectorOrganizationData(inspectorValue.DataType, inspectorValue.Name, -1).Hide)
			{
				Tuple<int, string> tuple = new Tuple<int, string>(inspectorValue.DataType, inspectorValue.Name);
				UIInspectorPropertyModel value = new UIInspectorPropertyModel(propEntry, inspectorValue);
				if (!dictionary.TryAdd(tuple, value))
				{
					DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple.Item1, tuple.Item2), this);
				}
			}
		}
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out var componentDefinition))
		{
			List<InspectorExposedVariable> inspectableMembers = componentDefinition.InspectableMembers;
			if (inspectableMembers.Count > 0)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
				foreach (InspectorExposedVariable item in inspectableMembers)
				{
					int typeId2 = EndlessTypeMapping.Instance.GetTypeId(item.DataType);
					InspectorOrganizationData inspectorOrganizationData = scriptComponent.Script.GetInspectorOrganizationData(typeId2, item.MemberName, typeId);
					if (!inspectorOrganizationData.Hide)
					{
						Tuple<int, string> tuple2 = new Tuple<int, string>(inspectorOrganizationData.DataType, inspectorOrganizationData.MemberName);
						UIInspectorPropertyModel value2 = new UIInspectorPropertyModel(propEntry, item, componentDefinition);
						if (!dictionary.TryAdd(tuple2, value2))
						{
							DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple2.Item1, tuple2.Item2), this);
						}
					}
				}
			}
		}
		foreach (string componentId in runtimePropInfo.PropData.ComponentIds)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2))
			{
				continue;
			}
			List<InspectorExposedVariable> inspectableMembers2 = componentDefinition2.InspectableMembers;
			if (componentDefinition2.InspectableMembers.Count <= 0)
			{
				continue;
			}
			int typeId3 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition2.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (InspectorExposedVariable item2 in inspectableMembers2)
			{
				int typeId4 = EndlessTypeMapping.Instance.GetTypeId(item2.DataType);
				InspectorOrganizationData inspectorOrganizationData2 = scriptComponent.Script.GetInspectorOrganizationData(typeId4, item2.MemberName, typeId3);
				if (!inspectorOrganizationData2.Hide)
				{
					Tuple<int, string> tuple3 = new Tuple<int, string>(inspectorOrganizationData2.DataType, inspectorOrganizationData2.MemberName);
					UIInspectorPropertyModel value3 = new UIInspectorPropertyModel(propEntry, item2, componentDefinition2);
					if (!dictionary.TryAdd(tuple3, value3))
					{
						DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple3.Item1, tuple3.Item2), this);
					}
				}
			}
		}
		foreach (InspectorOrganizationData inspectorOrganizationDatum in script.InspectorOrganizationData)
		{
			string groupName = inspectorOrganizationDatum.GetGroupName();
			if (!orderedDictionary.Contains(groupName))
			{
				orderedDictionary.Add(groupName, new List<UIInspectorPropertyModel>());
			}
			if (!inspectorOrganizationDatum.Hide)
			{
				Tuple<int, string> tuple4 = new Tuple<int, string>(inspectorOrganizationDatum.DataType, inspectorOrganizationDatum.MemberName);
				if (dictionary.TryGetValue(tuple4, out var value4))
				{
					((List<UIInspectorPropertyModel>)orderedDictionary[groupName]).Add(value4);
				}
				else
				{
					DebugUtility.LogError(string.Format("Could not get {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple4.Item1, tuple4.Item2), this);
				}
			}
		}
		foreach (DictionaryEntry item3 in orderedDictionary)
		{
			string groupName2 = (string)item3.Key;
			List<UIInspectorPropertyModel> list2 = (List<UIInspectorPropertyModel>)item3.Value;
			if (base.VerboseLogging)
			{
				foreach (UIInspectorPropertyModel item4 in list2)
				{
					Debug.Log(item4, this);
				}
			}
			UIInspectorGroupName uIInspectorGroupName = new UIInspectorGroupName(groupName2);
			list.Add(uIInspectorGroupName);
			list.AddRange(list2);
		}
		return list;
	}

	private void OnItemEyeDropped(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnItemEyeDropped", assetId, instanceId, propEntry.InstanceId);
		}
		Undock();
	}

	private void OnInspectorToolStateChanged(InspectorTool.States newState)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInspectorToolStateChanged", newState);
		}
		propertiesPresenter.gameObject.SetActive(newState == InspectorTool.States.Inspect);
		switch (newState)
		{
		case InspectorTool.States.Inspect:
			eyeDropperCanvasDisplayAndHideHandler.Hide(iEnumerableStraightVirtualizedView.ResizeAndKeepPosition);
			break;
		case InspectorTool.States.EyeDropper:
		{
			string arg = ((Tool.EyeDropperReferenceFilter != ReferenceFilter.None) ? Tool.EyeDropperReferenceFilter.ToString() : "Cell");
			eyeDropperText.Value = string.Format(eyeDropperTextTemplate, arg);
			eyeDropperCanvasDisplayAndHideHandler.Display();
			break;
		}
		default:
			DebugUtility.LogNoEnumSupportError(this, newState);
			break;
		}
	}
}
