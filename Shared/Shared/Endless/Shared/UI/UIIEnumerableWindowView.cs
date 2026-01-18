using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000282 RID: 642
	public class UIIEnumerableWindowView : UIBaseWindowView
	{
		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06001016 RID: 4118 RVA: 0x000449D7 File Offset: 0x00042BD7
		// (set) Token: 0x06001017 RID: 4119 RVA: 0x000449DF File Offset: 0x00042BDF
		public UIIEnumerablePresenter IEnumerablePresenter { get; private set; }

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06001018 RID: 4120 RVA: 0x000449E8 File Offset: 0x00042BE8
		// (set) Token: 0x06001019 RID: 4121 RVA: 0x000449F0 File Offset: 0x00042BF0
		public UIIEnumerableWindowModel Model { get; set; }

		// Token: 0x0600101A RID: 4122 RVA: 0x000449FC File Offset: 0x00042BFC
		public static UIIEnumerableWindowView Display(UIIEnumerableWindowModel model, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIIEnumerableWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIIEnumerableWindowView>(parent, dictionary);
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x00044A2C File Offset: 0x00042C2C
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.Model = (UIIEnumerableWindowModel)supplementalData["Model".ToLower()];
			this.canvas.overrideSorting = this.Model.WindowCanvasOverrideSorting > -1;
			this.canvas.sortingOrder = this.Model.WindowCanvasOverrideSorting;
			this.titleText.text = this.Model.Title;
			if (!this.iEnumerablePresenterDictionarySetup)
			{
				this.SetupIEnumerablePresenterDictionary();
			}
			foreach (KeyValuePair<UIBaseIEnumerableView.ArrangementStyle, UIIEnumerablePresenter> keyValuePair in this.iEnumerablePresenterDictionary)
			{
				keyValuePair.Value.gameObject.SetActive(false);
			}
			this.IEnumerablePresenter = this.iEnumerablePresenterDictionary[this.Model.Style];
			this.IEnumerablePresenter.IEnumerableView.SetTypeStyleOverrideDictionary(this.Model.TypeStyleOverrideDictionary);
			this.IEnumerablePresenter.gameObject.SetActive(true);
			this.IEnumerablePresenter.SetModel(this.Model.Items, true);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(this.Model.ToString(), this);
			}
			bool flag = false;
			if (this.Model.SelectionType != null)
			{
				flag = true;
				SelectionType value = this.Model.SelectionType.Value;
				this.IEnumerablePresenter.SetSelectionType(value, false);
				this.IEnumerablePresenter.SetSelected(this.Model.OriginalSelection, true, false);
			}
			this.IEnumerablePresenter.IEnumerableView.SetCanSelect(flag);
			string text = (flag ? "CanSelect" : "CanNotSelect");
			UIRectTransformDictionary[] array = this.iEnumerableRectTransformDictionaries;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Apply(text);
			}
			this.confirmButton.gameObject.SetActive(flag);
			this.TryToJumpToFirstSelection();
		}

		// Token: 0x0600101C RID: 4124 RVA: 0x00044C2C File Offset: 0x00042E2C
		private void TryToJumpToFirstSelection()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TryToJumpToFirstSelection", Array.Empty<object>());
			}
			if (this.Model.SelectionType == null)
			{
				return;
			}
			if (this.Model.OriginalSelection.Count == 0)
			{
				return;
			}
			object obj = this.IEnumerablePresenter.SelectedItemsList.FirstOrDefault<object>();
			if (obj == null)
			{
				return;
			}
			int itemIndex = this.IEnumerablePresenter.GetItemIndex(obj);
			if (itemIndex == -1)
			{
				return;
			}
			UIIEnumerableStraightVirtualizedView uiienumerableStraightVirtualizedView = this.IEnumerablePresenter.IEnumerableView as UIIEnumerableStraightVirtualizedView;
			if (uiienumerableStraightVirtualizedView != null)
			{
				uiienumerableStraightVirtualizedView.JumpToDataIndex(itemIndex, 0.5f, 0f, true, TweenEase.EaseInOutQuart, 0.25f, null, UIIEnumerableStraightVirtualizedView.LoopJumpDirectionEnum.Closest, false);
				return;
			}
			UIIEnumerableView uiienumerableView = this.IEnumerablePresenter.IEnumerableView as UIIEnumerableView;
			if (uiienumerableView != null)
			{
				uiienumerableView.JumpToDataIndex(itemIndex);
			}
		}

		// Token: 0x0600101D RID: 4125 RVA: 0x00044CF0 File Offset: 0x00042EF0
		public override void Close()
		{
			base.Close();
			foreach (KeyValuePair<UIBaseIEnumerableView.ArrangementStyle, UIIEnumerablePresenter> keyValuePair in this.iEnumerablePresenterDictionary)
			{
				keyValuePair.Value.Clear();
			}
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x00044D50 File Offset: 0x00042F50
		public void SetItems(IEnumerable items)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetItems", new object[] { items });
			}
			this.IEnumerablePresenter.SetModel(items, true);
		}

		// Token: 0x0600101F RID: 4127 RVA: 0x00044D7C File Offset: 0x00042F7C
		private void SetupIEnumerablePresenterDictionary()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetupIEnumerablePresenterDictionary", Array.Empty<object>());
			}
			if (this.iEnumerablePresenterDictionarySetup)
			{
				return;
			}
			this.iEnumerablePresenterDictionary = new Dictionary<UIBaseIEnumerableView.ArrangementStyle, UIIEnumerablePresenter>
			{
				{
					UIBaseIEnumerableView.ArrangementStyle.GridVertical,
					this.iEnumerablePresenterGridVertical
				},
				{
					UIBaseIEnumerableView.ArrangementStyle.StraightVertical,
					this.iEnumerablePresenterStraightVertical
				},
				{
					UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized,
					this.iEnumerablePresenterStraightVerticalVirtualized
				}
			};
			this.iEnumerablePresenterDictionarySetup = true;
		}

		// Token: 0x04000A3C RID: 2620
		private const string CAN_SELECT_KEY = "CanSelect";

		// Token: 0x04000A3D RID: 2621
		private const string CAN_NOT_SELECT_KEY = "CanNotSelect";

		// Token: 0x04000A3E RID: 2622
		[Header("UIIEnumerableWindowView")]
		[SerializeField]
		private Canvas canvas;

		// Token: 0x04000A3F RID: 2623
		[SerializeField]
		private TextMeshProUGUI titleText;

		// Token: 0x04000A40 RID: 2624
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenterGridVertical;

		// Token: 0x04000A41 RID: 2625
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenterStraightVertical;

		// Token: 0x04000A42 RID: 2626
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenterStraightVerticalVirtualized;

		// Token: 0x04000A43 RID: 2627
		[SerializeField]
		private UIRectTransformDictionary[] iEnumerableRectTransformDictionaries = Array.Empty<UIRectTransformDictionary>();

		// Token: 0x04000A44 RID: 2628
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000A45 RID: 2629
		private Dictionary<UIBaseIEnumerableView.ArrangementStyle, UIIEnumerablePresenter> iEnumerablePresenterDictionary;

		// Token: 0x04000A46 RID: 2630
		private bool iEnumerablePresenterDictionarySetup;
	}
}
