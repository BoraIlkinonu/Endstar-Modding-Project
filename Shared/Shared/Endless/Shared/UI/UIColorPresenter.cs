using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001DD RID: 477
	public class UIColorPresenter : UIBasePresenter<Color>
	{
		// Token: 0x06000BAC RID: 2988 RVA: 0x00032544 File Offset: 0x00030744
		protected override void Start()
		{
			base.Start();
			UIBaseColorView uibaseColorView = base.View.Interface as UIBaseColorView;
			uibaseColorView.OnColorChanged = (Action<Color>)Delegate.Combine(uibaseColorView.OnColorChanged, new Action<Color>(base.SetModelAndTriggerOnModelChanged));
			UIColorDefaultView uicolorDefaultView = uibaseColorView as UIColorDefaultView;
			if (uicolorDefaultView != null)
			{
				uicolorDefaultView.OpenColorWindow += this.OpenColorWindow;
			}
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x000325A4 File Offset: 0x000307A4
		public override void OnDespawn()
		{
			base.OnDespawn();
			if (this.colorWindow)
			{
				this.colorWindow.Close();
			}
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x000325C4 File Offset: 0x000307C4
		private void OpenColorWindow(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenColorWindow", new object[] { interactable });
			}
			UIBaseColorView.Styles styles = (UIBaseColorView.Styles)base.View.Interface.StyleEnum;
			if (styles != UIBaseColorView.Styles.Default)
			{
				if (styles == UIBaseColorView.Styles.Hdr)
				{
					styles = UIBaseColorView.Styles.DetailHdr;
				}
			}
			else
			{
				styles = UIBaseColorView.Styles.Detail;
			}
			UIColorWindowModel uicolorWindowModel = new UIColorWindowModel(base.Model, styles, new Action<Color>(base.SetModelAndTriggerOnModelChanged), interactable);
			this.colorWindow = UIColorWindowView.Display(uicolorWindowModel, null);
			if (this.colorWindow)
			{
				this.colorWindow.CloseUnityEvent.AddListener(new UnityAction(this.ClearColorWindow));
			}
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x00032668 File Offset: 0x00030868
		private void ClearColorWindow()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearColorWindow", Array.Empty<object>());
			}
			this.colorWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.ClearColorWindow));
			this.colorWindow = null;
		}

		// Token: 0x04000798 RID: 1944
		private UIColorWindowView colorWindow;
	}
}
