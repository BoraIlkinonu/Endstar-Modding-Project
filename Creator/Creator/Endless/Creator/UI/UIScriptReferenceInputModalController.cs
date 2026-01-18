using System;
using System.Collections.Generic;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D6 RID: 470
	public class UIScriptReferenceInputModalController : UIGameObject
	{
		// Token: 0x06000711 RID: 1809 RVA: 0x00023B81 File Offset: 0x00021D81
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.continueButton.onClick.AddListener(new UnityAction(this.CreateScriptReference));
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x00023BB8 File Offset: 0x00021DB8
		private bool ValidateScriptReference()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ValidateScriptReference", Array.Empty<object>());
			}
			bool flag = true;
			if (!MonoBehaviourSingleton<UIScriptDataWizard>.Instance.IsValidScriptingName(this.nameInCodeInputField.text))
			{
				this.nameInCodeInputField.PlayInvalidInputTweens();
				flag = false;
			}
			using (List<ScriptReference>.Enumerator enumerator = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript().ScriptReferences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!(enumerator.Current.NameInCode != this.nameInCodeInputField.text))
					{
						this.nameInCodeInputField.PlayInvalidInputTweens();
						flag = false;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x00023C70 File Offset: 0x00021E70
		private void CreateScriptReference()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateScriptReference", Array.Empty<object>());
			}
			if (!this.ValidateScriptReference())
			{
				return;
			}
			if (this.transformIdentifierListModel.SelectedTypedList.Count == 0)
			{
				return;
			}
			UITransformIdentifier uitransformIdentifier = this.transformIdentifierListModel.SelectedTypedList[0];
			ScriptReference scriptReference = new ScriptReference(this.nameInCodeInputField.text, uitransformIdentifier.TransformIdentifier.UniqueId, ScriptReference.ReferenceType.Transform);
			Script cloneOfScript = MonoBehaviourSingleton<UIScriptDataWizard>.Instance.GetCloneOfScript();
			cloneOfScript.ScriptReferences.Add(scriptReference);
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.UpdateScriptAndExit(cloneOfScript);
		}

		// Token: 0x04000669 RID: 1641
		[Header("UIScriptReferenceInputModalController")]
		[SerializeField]
		private UIInputField nameInCodeInputField;

		// Token: 0x0400066A RID: 1642
		[SerializeField]
		private UITransformIdentifierListModel transformIdentifierListModel;

		// Token: 0x0400066B RID: 1643
		[SerializeField]
		private UIButton continueButton;

		// Token: 0x0400066C RID: 1644
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
