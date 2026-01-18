using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
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
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI
{
	// Token: 0x020002A7 RID: 679
	public class UIInspectorToolPanelView : UIDockableToolPanelView<InspectorTool>, IBackable
	{
		// Token: 0x1700016C RID: 364
		// (get) Token: 0x06000B4E RID: 2894 RVA: 0x0003502A File Offset: 0x0003322A
		// (set) Token: 0x06000B4F RID: 2895 RVA: 0x00035032 File Offset: 0x00033232
		public SerializableGuid AssetId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x06000B50 RID: 2896 RVA: 0x0003503B File Offset: 0x0003323B
		// (set) Token: 0x06000B51 RID: 2897 RVA: 0x00035043 File Offset: 0x00033243
		public SerializableGuid InstanceId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x06000B52 RID: 2898 RVA: 0x0003504C File Offset: 0x0003324C
		// (set) Token: 0x06000B53 RID: 2899 RVA: 0x00035054 File Offset: 0x00033254
		public PropEntry PropEntry { get; private set; }

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x06000B54 RID: 2900 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override bool DisplayOnToolChangeMatchToToolType
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x06000B55 RID: 2901 RVA: 0x0003505D File Offset: 0x0003325D
		protected override float ListSize
		{
			get
			{
				return this.iEnumerableStraightVirtualizedView.ContentSize;
			}
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x0003506C File Offset: 0x0003326C
		protected override void Start()
		{
			base.Start();
			this.Tool.OnItemInspected.AddListener(new UnityAction<SerializableGuid, SerializableGuid, PropEntry>(this.View));
			this.Tool.OnItemEyeDropped.AddListener(new UnityAction<SerializableGuid, SerializableGuid, PropEntry>(this.OnItemEyeDropped));
			this.Tool.OnStateChanged.AddListener(new UnityAction<InspectorTool.States>(this.OnInspectorToolStateChanged));
			this.eyeDropperCanvasDisplayAndHideHandler.SetToHideEnd(true);
			this.iEnumerableStraightVirtualizedView.OnEnumerableCountChanged += this.Resize;
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x000350F6 File Offset: 0x000332F6
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.iEnumerableStraightVirtualizedView.OnEnumerableCountChanged -= this.Resize;
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x00035128 File Offset: 0x00033328
		public void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			InspectorTool.States state = this.Tool.State;
			if (state != InspectorTool.States.Inspect)
			{
				if (state != InspectorTool.States.EyeDropper)
				{
					DebugUtility.LogNoEnumSupportError<InspectorTool.States>(this, this.Tool.State);
					return;
				}
				this.Tool.SetStateToInspect();
				return;
			}
			else
			{
				if (InputManager.InputUnrestricted)
				{
					this.Tool.DeselectProp();
					return;
				}
				EventSystem.current.SetSelectedGameObject(null);
				return;
			}
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x0003519E File Offset: 0x0003339E
		private void Resize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Resize", Array.Empty<object>());
			}
			this.iEnumerableStraightVirtualizedView.Resize(true);
			this.iEnumerableStraightVirtualizedView.ReapplyItemSizes();
			base.TweenToMaxPanelHeight();
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x000351D5 File Offset: 0x000333D5
		public override void Display()
		{
			base.Display();
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x000351F8 File Offset: 0x000333F8
		public override void Hide()
		{
			base.Hide();
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
			this.AssetId = SerializableGuid.Empty;
			this.InstanceId = SerializableGuid.Empty;
			this.PropEntry = null;
			this.propertiesPresenter.Clear();
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0003524B File Offset: 0x0003344B
		public void Review()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Review", Array.Empty<object>());
			}
			this.View(this.AssetId, this.InstanceId, this.PropEntry);
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x00035280 File Offset: 0x00033480
		private async void View(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[]
				{
					assetId,
					instanceId,
					(propEntry == null) ? "null" : propEntry.Label
				});
			}
			if (assetId.IsEmpty)
			{
				if (this.Tool.State != InspectorTool.States.EyeDropper)
				{
					this.Clear();
					this.Hide();
				}
			}
			else if (!(assetId == this.AssetId) || !(instanceId == this.InstanceId))
			{
				this.AssetId = assetId;
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(assetId);
				TaskAwaiter<GetAllRolesResult> taskAwaiter = MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, null, false).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<GetAllRolesResult> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<GetAllRolesResult>);
				}
				bool flag = taskAwaiter.GetResult().Roles.GetRoleForUserId(EndlessServices.Instance.CloudService.ActiveUserId).IsGreaterThan(Roles.Viewer);
				this.label.interactable = flag;
				this.iEnumerableStraightVirtualizedView.SetInteractable(flag);
				this.iEnumerableStraightVirtualizedView.SetChildIEnumerableCanAddAndRemoveItems(flag);
				bool flag2 = runtimePropInfo.IsMissingObject && flag;
				this.reAddToGameLibraryButton.gameObject.SetActive(flag2);
				this.Clear();
				this.InstanceId = instanceId;
				this.PropEntry = propEntry;
				if (propEntry == null)
				{
					DebugUtility.LogException(new NullReferenceException("propEntry is null"), this);
				}
				else
				{
					this.label.text = propEntry.Label;
				}
				this.definitionNameText.text = this.definitionNamePreText + runtimePropInfo.PropData.Name;
				List<object> properties = this.GetProperties(runtimePropInfo, propEntry);
				this.iEnumerableStraightVirtualizedView.SetScrollPositionImmediately(0f);
				Dictionary<Type, Enum> dictionary = new Dictionary<Type, Enum>(UIInspectorToolPanelView.typeStyleOverrideDictionary);
				this.propertiesPresenter.IEnumerableView.SetTypeStyleOverrideDictionary(dictionary);
				this.propertiesPresenter.SetModel(properties, true);
				this.Display();
				base.TweenToMaxPanelHeight();
			}
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x000352CF File Offset: 0x000334CF
		private void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.propertiesPresenter.Clear();
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x000352F4 File Offset: 0x000334F4
		private List<object> GetProperties(PropLibrary.RuntimePropInfo runtimePropInfo, PropEntry propEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetProperties", new object[]
				{
					runtimePropInfo.EndlessProp.Prop.Name,
					propEntry.Label
				});
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
			foreach (InspectorScriptValue inspectorScriptValue in script.InspectorValues)
			{
				if (!script.GetInspectorOrganizationData(inspectorScriptValue.DataType, inspectorScriptValue.Name, -1).Hide)
				{
					Tuple<int, string> tuple = new Tuple<int, string>(inspectorScriptValue.DataType, inspectorScriptValue.Name);
					UIInspectorPropertyModel uiinspectorPropertyModel = new UIInspectorPropertyModel(propEntry, inspectorScriptValue);
					if (!dictionary.TryAdd(tuple, uiinspectorPropertyModel))
					{
						DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple.Item1, tuple.Item2), this);
					}
				}
			}
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(runtimePropInfo.PropData.BaseTypeId, out baseTypeDefinition))
			{
				List<InspectorExposedVariable> inspectableMembers = baseTypeDefinition.InspectableMembers;
				if (inspectableMembers.Count > 0)
				{
					int typeId = EndlessTypeMapping.Instance.GetTypeId(baseTypeDefinition.ComponentBase.GetType().AssemblyQualifiedName);
					foreach (InspectorExposedVariable inspectorExposedVariable in inspectableMembers)
					{
						int typeId2 = EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable.DataType);
						InspectorOrganizationData inspectorOrganizationData = scriptComponent.Script.GetInspectorOrganizationData(typeId2, inspectorExposedVariable.MemberName, typeId);
						if (!inspectorOrganizationData.Hide)
						{
							Tuple<int, string> tuple2 = new Tuple<int, string>(inspectorOrganizationData.DataType, inspectorOrganizationData.MemberName);
							UIInspectorPropertyModel uiinspectorPropertyModel2 = new UIInspectorPropertyModel(propEntry, inspectorExposedVariable, baseTypeDefinition);
							if (!dictionary.TryAdd(tuple2, uiinspectorPropertyModel2))
							{
								DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple2.Item1, tuple2.Item2), this);
							}
						}
					}
				}
			}
			foreach (string text in runtimePropInfo.PropData.ComponentIds)
			{
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition))
				{
					List<InspectorExposedVariable> inspectableMembers2 = componentDefinition.InspectableMembers;
					if (componentDefinition.InspectableMembers.Count > 0)
					{
						int typeId3 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
						foreach (InspectorExposedVariable inspectorExposedVariable2 in inspectableMembers2)
						{
							int typeId4 = EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable2.DataType);
							InspectorOrganizationData inspectorOrganizationData2 = scriptComponent.Script.GetInspectorOrganizationData(typeId4, inspectorExposedVariable2.MemberName, typeId3);
							if (!inspectorOrganizationData2.Hide)
							{
								Tuple<int, string> tuple3 = new Tuple<int, string>(inspectorOrganizationData2.DataType, inspectorOrganizationData2.MemberName);
								UIInspectorPropertyModel uiinspectorPropertyModel3 = new UIInspectorPropertyModel(propEntry, inspectorExposedVariable2, componentDefinition);
								if (!dictionary.TryAdd(tuple3, uiinspectorPropertyModel3))
								{
									DebugUtility.LogError(string.Format("Duplicate {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple3.Item1, tuple3.Item2), this);
								}
							}
						}
					}
				}
			}
			foreach (InspectorOrganizationData inspectorOrganizationData3 in script.InspectorOrganizationData)
			{
				string groupName = inspectorOrganizationData3.GetGroupName();
				if (!orderedDictionary.Contains(groupName))
				{
					orderedDictionary.Add(groupName, new List<UIInspectorPropertyModel>());
				}
				if (!inspectorOrganizationData3.Hide)
				{
					Tuple<int, string> tuple4 = new Tuple<int, string>(inspectorOrganizationData3.DataType, inspectorOrganizationData3.MemberName);
					UIInspectorPropertyModel uiinspectorPropertyModel4;
					if (dictionary.TryGetValue(tuple4, out uiinspectorPropertyModel4))
					{
						((List<UIInspectorPropertyModel>)orderedDictionary[groupName]).Add(uiinspectorPropertyModel4);
					}
					else
					{
						DebugUtility.LogError(string.Format("Could not get {0} with key of ( {1}, {2} )", "UIInspectorPropertyModel", tuple4.Item1, tuple4.Item2), this);
					}
				}
			}
			foreach (object obj in orderedDictionary)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				string text2 = (string)dictionaryEntry.Key;
				List<UIInspectorPropertyModel> list2 = (List<UIInspectorPropertyModel>)dictionaryEntry.Value;
				if (base.VerboseLogging)
				{
					foreach (UIInspectorPropertyModel uiinspectorPropertyModel5 in list2)
					{
						Debug.Log(uiinspectorPropertyModel5, this);
					}
				}
				UIInspectorGroupName uiinspectorGroupName = new UIInspectorGroupName(text2);
				list.Add(uiinspectorGroupName);
				list.AddRange(list2);
			}
			return list;
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x000358B8 File Offset: 0x00033AB8
		private void OnItemEyeDropped(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemEyeDropped", new object[] { assetId, instanceId, propEntry.InstanceId });
			}
			base.Undock();
		}

		// Token: 0x06000B61 RID: 2913 RVA: 0x00035904 File Offset: 0x00033B04
		private void OnInspectorToolStateChanged(InspectorTool.States newState)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInspectorToolStateChanged", new object[] { newState });
			}
			this.propertiesPresenter.gameObject.SetActive(newState == InspectorTool.States.Inspect);
			if (newState == InspectorTool.States.Inspect)
			{
				this.eyeDropperCanvasDisplayAndHideHandler.Hide(new Action(this.iEnumerableStraightVirtualizedView.ResizeAndKeepPosition));
				return;
			}
			if (newState != InspectorTool.States.EyeDropper)
			{
				DebugUtility.LogNoEnumSupportError<InspectorTool.States>(this, newState);
				return;
			}
			string text;
			if (this.Tool.EyeDropperReferenceFilter == ReferenceFilter.None)
			{
				text = "Cell";
			}
			else
			{
				text = this.Tool.EyeDropperReferenceFilter.ToString();
			}
			this.eyeDropperText.Value = string.Format(this.eyeDropperTextTemplate, text);
			this.eyeDropperCanvasDisplayAndHideHandler.Display();
		}

		// Token: 0x04000993 RID: 2451
		private static readonly Dictionary<Type, Enum> typeStyleOverrideDictionary = new Dictionary<Type, Enum> { 
		{
			typeof(IEnumerable),
			UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized
		} };

		// Token: 0x04000994 RID: 2452
		[Header("UIInspectorToolPanelView")]
		[SerializeField]
		private UIInputField label;

		// Token: 0x04000995 RID: 2453
		[SerializeField]
		private TextMeshProUGUI definitionNameText;

		// Token: 0x04000996 RID: 2454
		[SerializeField]
		private string definitionNamePreText = "Name: ";

		// Token: 0x04000997 RID: 2455
		[SerializeField]
		private UIDisplayAndHideHandler eyeDropperCanvasDisplayAndHideHandler;

		// Token: 0x04000998 RID: 2456
		[SerializeField]
		private UIText eyeDropperText;

		// Token: 0x04000999 RID: 2457
		[SerializeField]
		[TextArea]
		private string eyeDropperTextTemplate = "Click on a {0} in the world to set it as the referenced object.\n\n\n\nThe selected object will then become the one acted upon. You can remove the reference or replace it at any time later.";

		// Token: 0x0400099A RID: 2458
		[SerializeField]
		private UIButton reAddToGameLibraryButton;

		// Token: 0x0400099B RID: 2459
		[SerializeField]
		private UIIEnumerablePresenter propertiesPresenter;

		// Token: 0x0400099C RID: 2460
		[SerializeField]
		private UIIEnumerableStraightVirtualizedView iEnumerableStraightVirtualizedView;
	}
}
