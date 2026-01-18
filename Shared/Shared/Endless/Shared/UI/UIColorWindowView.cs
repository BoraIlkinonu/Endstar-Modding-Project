using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200027F RID: 639
	public class UIColorWindowView : UIBaseWindowView
	{
		// Token: 0x06000FF8 RID: 4088 RVA: 0x00044562 File Offset: 0x00042762
		protected override void Start()
		{
			base.Start();
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x00044588 File Offset: 0x00042788
		public static UIColorWindowView Display(UIColorWindowModel model, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIColorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIColorWindowView>(parent, dictionary);
		}

		// Token: 0x06000FFA RID: 4090 RVA: 0x000445B8 File Offset: 0x000427B8
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.model = (UIColorWindowModel)supplementalData["model"];
			UIBaseColorView.Styles style = this.model.Style;
			this.windowTitleText.Value = ((style == UIBaseColorView.Styles.DetailHdr) ? "HDR Color" : "Color");
			this.colorPresenter.SetModel(this.model.Color, false);
			this.hdrColorPresenter.SetModel(this.model.Color, false);
			this.colorPresenter.gameObject.SetActive(style != UIBaseColorView.Styles.DetailHdr);
			this.hdrColorPresenter.gameObject.SetActive(style == UIBaseColorView.Styles.DetailHdr);
			UIBaseColorView[] array = this.colorViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetInteractable(this.model.Interactable);
			}
			this.confirmButton.interactable = this.model.Interactable;
		}

		// Token: 0x06000FFB RID: 4091 RVA: 0x000446A0 File Offset: 0x000428A0
		private void Confirm()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Confirm", this);
			}
			Color color = ((this.model.Style == UIBaseColorView.Styles.DetailHdr) ? this.hdrColorPresenter.Model : this.colorPresenter.Model);
			UIColorWindowModel uicolorWindowModel = this.model;
			if (uicolorWindowModel != null)
			{
				uicolorWindowModel.OnConfirm(color);
			}
			this.Close();
		}

		// Token: 0x04000A2C RID: 2604
		[Header("UIColorWindowView")]
		[SerializeField]
		private UIText windowTitleText;

		// Token: 0x04000A2D RID: 2605
		[SerializeField]
		private UIColorPresenter colorPresenter;

		// Token: 0x04000A2E RID: 2606
		[SerializeField]
		private UIColorPresenter hdrColorPresenter;

		// Token: 0x04000A2F RID: 2607
		[SerializeField]
		private UIBaseColorView[] colorViews = Array.Empty<UIBaseColorView>();

		// Token: 0x04000A30 RID: 2608
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000A31 RID: 2609
		private UIColorWindowModel model;
	}
}
