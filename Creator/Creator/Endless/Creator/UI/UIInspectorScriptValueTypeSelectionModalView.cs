using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001D1 RID: 465
	public class UIInspectorScriptValueTypeSelectionModalView : UIScriptModalView
	{
		// Token: 0x060006F8 RID: 1784 RVA: 0x0002349D File Offset: 0x0002169D
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.inspectorScriptValueTypeRadio.SetDefaultValue(EndlessTypeMapping.Instance.LuaInspectorTypes[0]);
			this.inspectorScriptValueTypeRadio.SetValueToDefault(true);
			this.isCollectionToggle.SetIsOn(false, false, true);
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x000234D7 File Offset: 0x000216D7
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x000234FC File Offset: 0x000216FC
		public void ViewInspectorScriptValueTypeInfo(Type inspectorScriptValueType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewInspectorScriptValueTypeInfo", new object[] { inspectorScriptValueType });
			}
			DisplayNameAndDescription displayNameAndDescription = this.inspectorScriptValueTypeInfoDictionary[inspectorScriptValueType.Name];
			this.displayNameText.text = displayNameAndDescription.DisplayName;
			this.descriptionText.text = displayNameAndDescription.Description;
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x0002355C File Offset: 0x0002175C
		public void HandleIsCollectionToggleVisibility(Type inspectorScriptValueType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleIsCollectionToggleVisibility", new object[] { inspectorScriptValueType });
			}
			Type value = this.inspectorScriptValueTypeRadio.Value;
			bool flag = !this.nonCollectionSupported.Contains(value);
			this.isCollectionToggle.gameObject.SetActive(flag);
			if (!flag)
			{
				this.isCollectionToggle.SetIsOn(false, false, true);
			}
		}

		// Token: 0x04000644 RID: 1604
		[Header("UIInspectorScriptValueTypeSelectionModalView")]
		[SerializeField]
		private InspectorScriptValueTypeInfoDictionary inspectorScriptValueTypeInfoDictionary;

		// Token: 0x04000645 RID: 1605
		[SerializeField]
		private UIInspectorScriptValueTypeRadio inspectorScriptValueTypeRadio;

		// Token: 0x04000646 RID: 1606
		[SerializeField]
		private UIToggle isCollectionToggle;

		// Token: 0x04000647 RID: 1607
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000648 RID: 1608
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x04000649 RID: 1609
		private readonly HashSet<Type> nonCollectionSupported = new HashSet<Type> { typeof(NpcClassCustomizationData) };
	}
}
