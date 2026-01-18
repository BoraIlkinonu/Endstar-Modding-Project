using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D4 RID: 468
	public class UIScriptEventInputModalController : UIGameObject
	{
		// Token: 0x06000704 RID: 1796 RVA: 0x00023708 File Offset: 0x00021908
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.createEndlessParameterButton.onClick.AddListener(new UnityAction(this.CreateEndlessParameter));
			this.continueButton.onClick.AddListener(new UnityAction(this.CreateEndlessEventInfo));
		}

		// Token: 0x06000705 RID: 1797 RVA: 0x00023768 File Offset: 0x00021968
		private bool ValidateEndlessParameter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ValidateEndlessParameter", Array.Empty<object>());
			}
			bool flag = true;
			if (this.endlessParameterInfoListModel.Count >= this.endlessParameterInfoLimit.Value)
			{
				flag = false;
			}
			if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(this.endlessParameterInfoDisplayNameInputField.text))
			{
				this.endlessParameterInfoDisplayNameInputField.PlayInvalidInputTweens();
				flag = false;
			}
			using (IEnumerator<EndlessParameterInfo> enumerator = this.endlessParameterInfoListModel.ReadOnlyList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.DisplayName == this.endlessParameterInfoDisplayNameInputField.text)
					{
						this.endlessParameterInfoDisplayNameInputField.PlayInvalidInputTweens();
						flag = false;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000706 RID: 1798 RVA: 0x00023830 File Offset: 0x00021A30
		private void CreateEndlessParameter()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateEndlessParameter", Array.Empty<object>());
			}
			if (!this.ValidateEndlessParameter())
			{
				return;
			}
			Type type = this.endlessParameterInfoDataTypeRadio.Value;
			if (this.isCollectionToggle.IsOn)
			{
				type = type.MakeArrayType();
			}
			EndlessParameterInfo endlessParameterInfo = new EndlessParameterInfo(EndlessTypeMapping.Instance.GetTypeId(type.AssemblyQualifiedName), this.endlessParameterInfoDisplayNameInputField.text);
			this.endlessParameterInfoListModel.Add(endlessParameterInfo, true);
			this.endlessParameterInfoDisplayNameInputField.Clear(true);
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x000238B8 File Offset: 0x00021AB8
		private bool ValidateEndlessEventInfo()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ValidateEndlessEventInfo", Array.Empty<object>());
			}
			bool flag = true;
			if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(this.memberNameInputField.text))
			{
				this.memberNameInputField.PlayInvalidInputTweens();
				flag = false;
			}
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			using (List<EndlessEventInfo>.Enumerator enumerator = (this.view.IsEvent ? cloneOfScript.Events : cloneOfScript.Receivers).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.MemberName == this.memberNameInputField.text)
					{
						this.memberNameInputField.PlayInvalidInputTweens();
						flag = false;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x00023988 File Offset: 0x00021B88
		private void CreateEndlessEventInfo()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateEndlessEventInfo", Array.Empty<object>());
			}
			if (!this.ValidateEndlessEventInfo())
			{
				return;
			}
			List<EndlessParameterInfo> list = this.endlessParameterInfoListModel.ReadOnlyList.ToList<EndlessParameterInfo>();
			EndlessEventInfo endlessEventInfo = new EndlessEventInfo(this.memberNameInputField.text, list)
			{
				Description = this.descriptionInputField.text
			};
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			if (this.view.IsEvent)
			{
				cloneOfScript.Events.Add(endlessEventInfo);
			}
			else
			{
				cloneOfScript.Receivers.Add(endlessEventInfo);
			}
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
		}

		// Token: 0x04000653 RID: 1619
		[SerializeField]
		private UIScriptEventInputModalView view;

		// Token: 0x04000654 RID: 1620
		[SerializeField]
		private IntVariable endlessParameterInfoLimit;

		// Token: 0x04000655 RID: 1621
		[SerializeField]
		private UIInputField memberNameInputField;

		// Token: 0x04000656 RID: 1622
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000657 RID: 1623
		[Header("EndlessParameterInfo Creation")]
		[SerializeField]
		private UIEndlessParameterInfoListModel endlessParameterInfoListModel;

		// Token: 0x04000658 RID: 1624
		[SerializeField]
		private UIInputField endlessParameterInfoDisplayNameInputField;

		// Token: 0x04000659 RID: 1625
		[SerializeField]
		private UIInspectorScriptValueTypeRadio endlessParameterInfoDataTypeRadio;

		// Token: 0x0400065A RID: 1626
		[SerializeField]
		private UIToggle isCollectionToggle;

		// Token: 0x0400065B RID: 1627
		[SerializeField]
		private UIButton createEndlessParameterButton;

		// Token: 0x0400065C RID: 1628
		[SerializeField]
		private UIButton continueButton;

		// Token: 0x0400065D RID: 1629
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
