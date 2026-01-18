using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001CE RID: 462
	public class UIInspectorScriptValueInputModalController : UIGameObject
	{
		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x060006E3 RID: 1763 RVA: 0x00022D62 File Offset: 0x00020F62
		private Type InspectorScriptValueType
		{
			get
			{
				return this.view.InspectorScriptValueType;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060006E4 RID: 1764 RVA: 0x00022D6F File Offset: 0x00020F6F
		private bool IsCollection
		{
			get
			{
				return this.view.IsCollection;
			}
		}

		// Token: 0x060006E5 RID: 1765 RVA: 0x00022D7C File Offset: 0x00020F7C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.createInspectorScriptValueButton.onClick.AddListener(new UnityAction(this.CreateInspectorScriptValueFromInput));
		}

		// Token: 0x060006E6 RID: 1766 RVA: 0x00022DB4 File Offset: 0x00020FB4
		private bool ValidateInspectorScriptValue()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ValidateInspectorScriptValue", Array.Empty<object>());
			}
			bool flag = true;
			if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(this.nameInputField.text))
			{
				this.nameInputField.PlayInvalidInputTweens();
				flag = false;
			}
			using (List<InspectorScriptValue>.Enumerator enumerator = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript().InspectorValues.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!(enumerator.Current.Name != this.nameInputField.text))
					{
						this.nameInputField.PlayInvalidInputTweens();
						flag = false;
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x060006E7 RID: 1767 RVA: 0x00022E70 File Offset: 0x00021070
		private void CreateInspectorScriptValueFromInput()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateInspectorScriptValueFromInput", Array.Empty<object>());
			}
			if (!this.ValidateInspectorScriptValue())
			{
				return;
			}
			string text = this.nameInputField.text;
			object defaultValueAsObject = this.GetDefaultValueAsObject();
			string text2 = JsonConvert.SerializeObject(defaultValueAsObject);
			Type type = defaultValueAsObject.GetType();
			if (this.InspectorScriptValueType.IsEnum)
			{
				type = this.InspectorScriptValueType;
				if (this.IsCollection)
				{
					type = type.MakeArrayType();
				}
			}
			else if (this.IsCollection)
			{
				type = this.InspectorScriptValueType.MakeArrayType();
			}
			string text3 = this.descriptionInputField.text;
			if (this.verboseLogging)
			{
				DebugUtility.Log("memberName: " + text, this);
				DebugUtility.Log(string.Format("{0}: {1}", "InspectorScriptValueType", this.InspectorScriptValueType), this);
				DebugUtility.Log("type: " + type.Name, this);
				DebugUtility.Log("defaultValue: " + text2, this);
			}
			this.CreateInspectorScriptValue(type, text, text3, text2, true);
		}

		// Token: 0x060006E8 RID: 1768 RVA: 0x00022F68 File Offset: 0x00021168
		private object GetDefaultValueAsObject()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetDefaultValueAsObject", Array.Empty<object>());
			}
			object modelAsObject = this.view.Presentable.ModelAsObject;
			Type inspectorScriptValueType = this.view.InspectorScriptValueType;
			if (this.verboseLogging)
			{
				DebugUtility.Log("model: " + JsonConvert.SerializeObject(modelAsObject), this);
				DebugUtility.Log("valueType: " + inspectorScriptValueType.Name, this);
			}
			return modelAsObject;
		}

		// Token: 0x060006E9 RID: 1769 RVA: 0x00022FE0 File Offset: 0x000211E0
		private void CreateInspectorScriptValue(Type type, string memberName, string description, string defaultValue, bool exitAfterUpdateScript)
		{
			if (this.verboseLogging || Application.isEditor)
			{
				DebugUtility.LogMethod(this, "CreateInspectorScriptValue", new object[] { type.Name, memberName, description, defaultValue, exitAfterUpdateScript });
			}
			int typeId = EndlessTypeMapping.Instance.GetTypeId(type.AssemblyQualifiedName);
			InspectorScriptValue inspectorScriptValue = new InspectorScriptValue(memberName, string.Empty, typeId, description, defaultValue, Array.Empty<ClampValue>());
			if (this.verboseLogging || Application.isEditor)
			{
				DebugUtility.Log("inspectorScriptValue: " + JsonConvert.SerializeObject(inspectorScriptValue), this);
			}
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			cloneOfScript.InspectorValues.Add(inspectorScriptValue);
			InspectorOrganizationData inspectorOrganizationData = new InspectorOrganizationData(typeId, memberName, -1, this.groupNameInputField.text, this.hideToggle.IsOn, "");
			cloneOfScript.InspectorOrganizationData.Add(inspectorOrganizationData);
			if (exitAfterUpdateScript)
			{
				MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
				return;
			}
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScript(cloneOfScript);
		}

		// Token: 0x0400062D RID: 1581
		[SerializeField]
		private UIInspectorScriptValueInputModalView view;

		// Token: 0x0400062E RID: 1582
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x0400062F RID: 1583
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000630 RID: 1584
		[SerializeField]
		private UIInputField groupNameInputField;

		// Token: 0x04000631 RID: 1585
		[SerializeField]
		private UIToggle hideToggle;

		// Token: 0x04000632 RID: 1586
		[SerializeField]
		private UIButton createInspectorScriptValueButton;

		// Token: 0x04000633 RID: 1587
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
