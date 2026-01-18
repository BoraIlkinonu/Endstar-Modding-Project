using System;
using System.Collections.Generic;
using Endless.Props;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001D7 RID: 471
	public class UIScriptReferenceInputModalView : UIScriptModalView
	{
		// Token: 0x06000715 RID: 1813 RVA: 0x00023D08 File Offset: 0x00021F08
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.nameInCodeInputField.Clear(true);
			UIBaseWindowView displayed = MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed;
			if (displayed == null)
			{
				DebugUtility.LogError("displayedWindow is null!", this);
				this.Close();
			}
			UIScriptWindowView uiscriptWindowView = (UIScriptWindowView)displayed;
			if (uiscriptWindowView == null)
			{
				DebugUtility.LogError("displayedWindow is not a type of UIScriptWindowView!", this);
				this.Close();
			}
			TransformIdentifier[] componentsInChildren = uiscriptWindowView.Model.GetRuntimePropInfo().EndlessProp.gameObject.GetComponentsInChildren<TransformIdentifier>(true);
			List<UITransformIdentifier> list = new List<UITransformIdentifier>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				TransformIdentifier transformIdentifier = componentsInChildren[i];
				string text = transformIdentifier.gameObject.name;
				if (i == 0)
				{
					text = text.Replace("(Clone)", string.Empty);
				}
				UITransformIdentifier uitransformIdentifier = new UITransformIdentifier(transformIdentifier, text);
				list.Add(uitransformIdentifier);
			}
			this.transformIdentifierListModel.Set(list, true);
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0400066D RID: 1645
		[Header("UIScriptReferenceInputModalView")]
		[SerializeField]
		private UIInputField nameInCodeInputField;

		// Token: 0x0400066E RID: 1646
		[SerializeField]
		private UITransformIdentifierListModel transformIdentifierListModel;
	}
}
