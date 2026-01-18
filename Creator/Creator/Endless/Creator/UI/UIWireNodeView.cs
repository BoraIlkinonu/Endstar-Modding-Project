using System;
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

namespace Endless.Creator.UI
{
	// Token: 0x0200030F RID: 783
	public class UIWireNodeView : UIGameObject, IPoolableT
	{
		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000E13 RID: 3603 RVA: 0x00042FD6 File Offset: 0x000411D6
		// (set) Token: 0x06000E14 RID: 3604 RVA: 0x00042FDE File Offset: 0x000411DE
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000E15 RID: 3605 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000E16 RID: 3606 RVA: 0x00042FE7 File Offset: 0x000411E7
		// (set) Token: 0x06000E17 RID: 3607 RVA: 0x00042FEF File Offset: 0x000411EF
		public SerializableGuid InspectedObjectId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000E18 RID: 3608 RVA: 0x00042FF8 File Offset: 0x000411F8
		// (set) Token: 0x06000E19 RID: 3609 RVA: 0x00043000 File Offset: 0x00041200
		public EndlessEventInfo NodeEvent { get; private set; }

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000E1A RID: 3610 RVA: 0x00043009 File Offset: 0x00041209
		// (set) Token: 0x06000E1B RID: 3611 RVA: 0x00043011 File Offset: 0x00041211
		public int NodeIndex { get; private set; } = -1;

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000E1C RID: 3612 RVA: 0x0004301A File Offset: 0x0004121A
		// (set) Token: 0x06000E1D RID: 3613 RVA: 0x00043022 File Offset: 0x00041222
		public string AssemblyQualifiedTypeName { get; private set; }

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000E1E RID: 3614 RVA: 0x0004302B File Offset: 0x0004122B
		// (set) Token: 0x06000E1F RID: 3615 RVA: 0x00043033 File Offset: 0x00041233
		public UIWiringObjectInspectorView ParentWiringObjectInspectorView { get; private set; }

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000E20 RID: 3616 RVA: 0x0004303C File Offset: 0x0004123C
		public bool IsOnLeftOfScreen
		{
			get
			{
				return this.ParentWiringObjectInspectorView.IsOnLeftOfScreen;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000E21 RID: 3617 RVA: 0x00043049 File Offset: 0x00041249
		public Vector3 WirePoint
		{
			get
			{
				return this.wireCount.rectTransform.position;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000E22 RID: 3618 RVA: 0x0004305B File Offset: 0x0004125B
		public string MemberName
		{
			get
			{
				return this.NodeEvent.MemberName;
			}
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00043068 File Offset: 0x00041268
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00043084 File Offset: 0x00041284
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.ToggleOnNothingSelectedTweens(false);
			this.ToggleSelectedVisuals(false);
			this.dropHandler.gameObject.SetActive(false);
			this.InspectedObjectId = SerializableGuid.Empty;
			this.NodeEvent = null;
			this.NodeIndex = -1;
			this.wireOrganizationData = null;
			this.wireNodeController.OnDespawn();
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x000430F4 File Offset: 0x000412F4
		public void Initialize(SerializableGuid inspectedObjectId, EndlessEventInfo nodeEvent, int nodeIndex, string assemblyQualifiedTypeName, UIWiringObjectInspectorView parentWiringObjectInspectorView, Color color, bool showComponentName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { inspectedObjectId, nodeEvent, assemblyQualifiedTypeName, parentWiringObjectInspectorView, color, showComponentName });
			}
			this.InspectedObjectId = inspectedObjectId;
			this.NodeEvent = nodeEvent;
			this.NodeIndex = nodeIndex;
			this.AssemblyQualifiedTypeName = assemblyQualifiedTypeName;
			this.ParentWiringObjectInspectorView = parentWiringObjectInspectorView;
			bool flag = !nodeEvent.Description.IsNullOrEmptyOrWhiteSpace();
			string text = "<color=#D1D5D9>" + nodeEvent.MemberName + "</color>";
			if (showComponentName)
			{
				string simpleTypeName = this.GetSimpleTypeName(assemblyQualifiedTypeName);
				text = text + " <color=#808080>(" + simpleTypeName + ")</color>";
			}
			this.label.text = text;
			this.label.color = color;
			this.label.fontStyle = (flag ? FontStyles.Underline : FontStyles.Normal);
			this.label.raycastTarget = flag;
			this.descrptionTooltip.enabled = flag;
			this.descrptionTooltip.SetTooltip(nodeEvent.Description);
			this.LoadWireOrganizationData();
			this.UpdateWireCount();
			for (int i = 0; i < this.toggleImagesToApplyColorTo.Length; i++)
			{
				this.toggleImagesToApplyColorTo[i].color = new Color(color.r, color.g, color.b, this.toggleImagesToApplyColorTo[i].color.a);
			}
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x0004325C File Offset: 0x0004145C
		public void SetIsOnLeftOfScreen(bool isOnLeftOfScreen)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetIsOnLeftOfScreen", new object[] { isOnLeftOfScreen });
			}
			this.label.alignment = (isOnLeftOfScreen ? TextAlignmentOptions.Left : TextAlignmentOptions.Right);
			this.labelHorizontalLayoutGroup.childAlignment = (isOnLeftOfScreen ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight);
			for (int i = 0; i < this.rectTransformDictionaries.Length; i++)
			{
				this.rectTransformDictionaries[i].Apply(isOnLeftOfScreen ? "left" : "right");
			}
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x000432E6 File Offset: 0x000414E6
		public void ToggleOnNothingSelectedTweens(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleOnNothingSelectedTweens", new object[] { state });
			}
			if (state)
			{
				this.onNothingSelectedTweens.Tween();
				return;
			}
			this.onNothingSelectedTweens.Cancel();
		}

		// Token: 0x06000E28 RID: 3624 RVA: 0x00043324 File Offset: 0x00041524
		public void ToggleSelectedVisuals(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleSelectedVisuals", new object[] { state });
			}
			if (state)
			{
				this.onSelectedTweens.Tween();
				return;
			}
			this.onDeselectedTweens.Tween();
		}

		// Token: 0x06000E29 RID: 3625 RVA: 0x00043364 File Offset: 0x00041564
		public void UpdateWireCount()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateWireCount", Array.Empty<object>());
			}
			IReadOnlyList<WireBundle> readOnlyList = (this.ParentWiringObjectInspectorView.IsReceiver ? WiringUtilities.GetWiresWithAReceiverOf(this.InspectedObjectId, this.NodeEvent.MemberName) : WiringUtilities.GetWiresEmittingFrom(this.InspectedObjectId, this.NodeEvent.MemberName));
			bool flag = readOnlyList.Count > 0;
			this.wireCount.gameObject.SetActive(flag);
			if (flag)
			{
				this.wireCount.text = readOnlyList.Count.ToString();
			}
		}

		// Token: 0x06000E2A RID: 3626 RVA: 0x000433FC File Offset: 0x000415FC
		public void ToggleDropHandler(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleDropHandler", new object[] { state });
			}
			this.dropHandler.gameObject.SetActive(state);
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00043434 File Offset: 0x00041634
		private void LoadWireOrganizationData()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadWireOrganizationData", Array.Empty<object>());
			}
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(this.InspectedObjectId);
			Script script;
			if (!this.TryGetScript(assetIdFromInstanceId, out script))
			{
				if (this.verboseLogging)
				{
					Debug.Log("Disabled: false", this);
				}
				this.SetWireOrganizationDataAndActivate(null);
				return;
			}
			int num = (string.IsNullOrEmpty(this.AssemblyQualifiedTypeName) ? (-1) : EndlessTypeMapping.Instance.GetTypeId(this.AssemblyQualifiedTypeName));
			WireOrganizationData wireOrganizationData = (this.ParentWiringObjectInspectorView.IsReceiver ? script.GetWireOrganizationReceiverData(this.NodeEvent.MemberName, num) : script.GetWireOrganizationEventData(this.NodeEvent.MemberName, num));
			this.SetWireOrganizationDataAndActivate(wireOrganizationData);
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "Disabled", wireOrganizationData.Disabled), this);
			}
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0004351C File Offset: 0x0004171C
		private bool TryGetScript(SerializableGuid assetId, out Script script)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TryGetScript", new object[] { assetId });
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

		// Token: 0x06000E2D RID: 3629 RVA: 0x000435FC File Offset: 0x000417FC
		private void SetWireOrganizationDataAndActivate(WireOrganizationData data)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetWireOrganizationDataAndActivate", new object[] { data });
			}
			this.wireOrganizationData = data;
			base.gameObject.SetActive(data != null && !data.Disabled);
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x0004363C File Offset: 0x0004183C
		private string GetSimpleTypeName(string assemblyQualifiedTypeName)
		{
			if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
			{
				return "Lua";
			}
			string text = assemblyQualifiedTypeName.Split(',', StringSplitOptions.None)[0];
			int num = text.LastIndexOf('.');
			if (num < 0)
			{
				return text;
			}
			return text.Substring(num + 1);
		}

		// Token: 0x04000BF9 RID: 3065
		[SerializeField]
		private TextMeshProUGUI label;

		// Token: 0x04000BFA RID: 3066
		[SerializeField]
		private HorizontalLayoutGroup labelHorizontalLayoutGroup;

		// Token: 0x04000BFB RID: 3067
		[SerializeField]
		private UITooltip descrptionTooltip;

		// Token: 0x04000BFC RID: 3068
		[SerializeField]
		private TextMeshProUGUI wireCount;

		// Token: 0x04000BFD RID: 3069
		[SerializeField]
		private UIRectTransformDictionary[] rectTransformDictionaries = new UIRectTransformDictionary[0];

		// Token: 0x04000BFE RID: 3070
		[SerializeField]
		private Image[] toggleImagesToApplyColorTo = new Image[0];

		// Token: 0x04000BFF RID: 3071
		[SerializeField]
		private DropHandler dropHandler;

		// Token: 0x04000C00 RID: 3072
		[SerializeField]
		private UIWireNodeController wireNodeController;

		// Token: 0x04000C01 RID: 3073
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection onNothingSelectedTweens;

		// Token: 0x04000C02 RID: 3074
		[SerializeField]
		private TweenCollection onSelectedTweens;

		// Token: 0x04000C03 RID: 3075
		[SerializeField]
		private TweenCollection onDeselectedTweens;

		// Token: 0x04000C04 RID: 3076
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000C05 RID: 3077
		private WireOrganizationData wireOrganizationData;
	}
}
