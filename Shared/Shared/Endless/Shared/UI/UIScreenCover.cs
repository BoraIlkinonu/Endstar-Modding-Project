using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200024F RID: 591
	[DefaultExecutionOrder(-2147483648)]
	public class UIScreenCover : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000EF8 RID: 3832 RVA: 0x000407CF File Offset: 0x0003E9CF
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000EF9 RID: 3833 RVA: 0x000407D7 File Offset: 0x0003E9D7
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000EFA RID: 3834 RVA: 0x000407DF File Offset: 0x0003E9DF
		public bool IsDisplaying
		{
			get
			{
				return this.displayAndHideHandler.IsDisplaying;
			}
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x000407EC File Offset: 0x0003E9EC
		public void Display(string textToDisplay, Action onScreenCovered = null, bool tweenIn = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					textToDisplay,
					onScreenCovered.DebugIsNull(),
					tweenIn
				});
			}
			this.textToDisplay = textToDisplay;
			if (this.displayAndHideHandler.IsDisplaying)
			{
				this.displayAndHideHandler.SetToDisplayEnd(true);
				this.textDisplayAndHideHandler.Hide(new Action(this.ApplyTextToDisplay));
				if (onScreenCovered != null)
				{
					onScreenCovered();
				}
			}
			else
			{
				if (tweenIn)
				{
					this.displayAndHideHandler.Display(onScreenCovered);
				}
				else
				{
					this.displayAndHideHandler.SetToDisplayEnd(true);
					if (onScreenCovered != null)
					{
						onScreenCovered();
					}
				}
				this.ApplyTextToDisplay();
			}
			this.OnLoadingStarted.Invoke();
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x000408A1 File Offset: 0x0003EAA1
		public void UpdateText(string textToDisplay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateText", new object[] { textToDisplay });
			}
			this.textToDisplay = textToDisplay;
			this.text.text = textToDisplay;
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x000408D3 File Offset: 0x0003EAD3
		public void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			this.textDisplayAndHideHandler.Hide();
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x0004090E File Offset: 0x0003EB0E
		public void SetDisplayDuration(float inSeconds)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayDuration", new object[] { inSeconds });
			}
			this.displayAndHideHandler.SetDisplayDuration(inSeconds);
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x0004093E File Offset: 0x0003EB3E
		private void ApplyTextToDisplay()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyTextToDisplay", Array.Empty<object>());
			}
			this.text.text = this.textToDisplay;
			this.textDisplayAndHideHandler.Display();
		}

		// Token: 0x04000971 RID: 2417
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x04000972 RID: 2418
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000973 RID: 2419
		[SerializeField]
		private UIDisplayAndHideHandler textDisplayAndHideHandler;

		// Token: 0x04000974 RID: 2420
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000975 RID: 2421
		private string textToDisplay = string.Empty;
	}
}
