using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001F3 RID: 499
	public class UIHeartHalfView : UIPoolableGameObject
	{
		// Token: 0x060007CF RID: 1999 RVA: 0x00026688 File Offset: 0x00024888
		public override void OnSpawn()
		{
			base.OnSpawn();
			this.surplusText.enabled = false;
			this.displayAndHideHandler.SetToDisplayStart(false);
		}

		// Token: 0x060007D0 RID: 2000 RVA: 0x000266A8 File Offset: 0x000248A8
		public void View(int index, int displayIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { index, displayIndex });
			}
			bool flag = index % 2 == 0;
			this.image.sprite = (flag ? this.leftSprite : this.rightSprite);
			float num = (flag ? 0f : (base.RectTransform.rect.width / -2f));
			this.surplusText.rectTransform.localPosition = new Vector3(num, 0f, 0f);
			this.displayAndHideHandler.SetDisplayDelay((float)displayIndex * 0.125f);
			this.displayAndHideHandler.Display();
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00026768 File Offset: 0x00024968
		public void ViewSurplus(int surplus)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSurplus", new object[] { surplus });
			}
			this.surplusText.text = string.Format("+{0:N0}", surplus);
			this.surplusText.enabled = true;
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x000267BE File Offset: 0x000249BE
		public void DisableSurplus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisableSurplus", Array.Empty<object>());
			}
			this.surplusText.enabled = false;
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x000267E4 File Offset: 0x000249E4
		public void HideAndDespawnOnComplete()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideAndDespawnOnComplete", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide(new Action(this.Despawn));
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x00026815 File Offset: 0x00024A15
		private void Despawn()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIHeartHalfView>(this);
		}

		// Token: 0x040006EF RID: 1775
		[Header("UIHeartHalfView")]
		[SerializeField]
		private Sprite leftSprite;

		// Token: 0x040006F0 RID: 1776
		[SerializeField]
		private Sprite rightSprite;

		// Token: 0x040006F1 RID: 1777
		[SerializeField]
		private Image image;

		// Token: 0x040006F2 RID: 1778
		[SerializeField]
		private TextMeshProUGUI surplusText;

		// Token: 0x040006F3 RID: 1779
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;
	}
}
