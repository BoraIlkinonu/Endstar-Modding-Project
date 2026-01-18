using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000069 RID: 105
	public class UIScreenConfirmationModalView : UIEscapableModalView
	{
		// Token: 0x060001E5 RID: 485 RVA: 0x0000B170 File Offset: 0x00009370
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.previousResolution = (Resolution)modalData[0];
			this.previousFullScreenMode = (FullScreenMode)modalData[1];
			this.countdownStatus = this.countdown;
			this.DisplayCountdownStatus();
			this.countdownCoroutine = MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.Countdown());
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000B1C8 File Offset: 0x000093C8
		public override void Close()
		{
			base.Close();
			this.CancelCountdown();
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x0000B1D8 File Offset: 0x000093D8
		public void Revert()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Revert", Array.Empty<object>());
			}
			int width = this.previousResolution.width;
			int height = this.previousResolution.height;
			FullScreenMode fullScreenMode = this.previousFullScreenMode;
			Debug.Log(string.Format("Setting resolution via Revert {0}, {1}, {2}", width, height, fullScreenMode));
			Screen.SetResolution(width, height, fullScreenMode);
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x0000B24F File Offset: 0x0000944F
		private void CancelCountdown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelCountdown", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StopCoroutine(this.countdownCoroutine);
			this.countdownCoroutine = null;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000B280 File Offset: 0x00009480
		private IEnumerator Countdown()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Countdown", Array.Empty<object>());
			}
			while (this.countdownStatus > 0)
			{
				yield return new WaitForSecondsRealtime(1f);
				this.countdownStatus--;
				this.DisplayCountdownStatus();
			}
			this.Revert();
			yield break;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000B28F File Offset: 0x0000948F
		private void DisplayCountdownStatus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayCountdownStatus", Array.Empty<object>());
			}
			this.countdownText.text = string.Format(this.countdownTextFormat, this.countdownStatus);
		}

		// Token: 0x0400015B RID: 347
		[Header("UIScreenConfirmationModalView")]
		[SerializeField]
		private string countdownTextFormat = "Your settings will revert in ({0}) seconds...";

		// Token: 0x0400015C RID: 348
		[SerializeField]
		private int countdown = 15;

		// Token: 0x0400015D RID: 349
		[SerializeField]
		private TextMeshProUGUI countdownText;

		// Token: 0x0400015E RID: 350
		private Resolution previousResolution;

		// Token: 0x0400015F RID: 351
		private FullScreenMode previousFullScreenMode;

		// Token: 0x04000160 RID: 352
		private Coroutine countdownCoroutine;

		// Token: 0x04000161 RID: 353
		private int countdownStatus = 15;
	}
}
