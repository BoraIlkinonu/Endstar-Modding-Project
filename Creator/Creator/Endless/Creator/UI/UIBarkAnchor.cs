using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000097 RID: 151
	public class UIBarkAnchor : UIBaseAnchor
	{
		// Token: 0x06000251 RID: 593 RVA: 0x00010E70 File Offset: 0x0000F070
		public static UIBarkAnchor CreateInstance(UIBarkAnchor prefab, Transform target, RectTransform container, string text, float secondsToDisplay = 5f, Color? backgroundColor = null, Vector3? offset = null)
		{
			UIBarkAnchor uibarkAnchor = UIBaseAnchor.CreateAndInitialize<UIBarkAnchor>(prefab, target, container, offset);
			uibarkAnchor.SetText(text);
			uibarkAnchor.SetBackgroundColor(backgroundColor);
			uibarkAnchor.SetAutoCloseTime(secondsToDisplay);
			return uibarkAnchor;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00010E93 File Offset: 0x0000F093
		public void SetText(string text)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetText", new object[] { text });
			}
			this.barkText.text = text;
		}

		// Token: 0x06000253 RID: 595 RVA: 0x00010EC0 File Offset: 0x0000F0C0
		public void SetBackgroundColor(Color? backgroundColor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetBackgroundColor", new object[] { backgroundColor });
			}
			this.backgroundImage.color = backgroundColor ?? this.defaultBackgroundColor;
		}

		// Token: 0x06000254 RID: 596 RVA: 0x00010F14 File Offset: 0x0000F114
		public void SetAutoCloseTime(float secondsToDisplay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAutoCloseTime", new object[] { secondsToDisplay });
			}
			if (this.closeCoroutine != null)
			{
				base.StopCoroutine(this.closeCoroutine);
			}
			this.closeCoroutine = base.StartCoroutine(this.WaitAndClose(secondsToDisplay));
		}

		// Token: 0x06000255 RID: 597 RVA: 0x00010F6A File Offset: 0x0000F16A
		protected override void UnregisterAnchorAndHide()
		{
			base.UnregisterAnchorAndHide();
			if (this.closeCoroutine != null)
			{
				base.StopCoroutine(this.closeCoroutine);
				this.closeCoroutine = null;
			}
		}

		// Token: 0x06000256 RID: 598 RVA: 0x00010F8D File Offset: 0x0000F18D
		private IEnumerator WaitAndClose(float secondsToWait)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitAndClose", new object[] { secondsToWait });
			}
			yield return new WaitForSeconds(secondsToWait);
			this.closeCoroutine = null;
			this.Close();
			yield break;
		}

		// Token: 0x040002A3 RID: 675
		[Header("UIBarkAnchor")]
		[SerializeField]
		private Image backgroundImage;

		// Token: 0x040002A4 RID: 676
		[SerializeField]
		private TextMeshProUGUI barkText;

		// Token: 0x040002A5 RID: 677
		[SerializeField]
		private Color defaultBackgroundColor = Color.blue;

		// Token: 0x040002A6 RID: 678
		private Coroutine closeCoroutine;
	}
}
