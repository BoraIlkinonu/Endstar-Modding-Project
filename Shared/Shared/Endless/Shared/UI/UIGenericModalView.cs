using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001CA RID: 458
	public class UIGenericModalView : UIEscapableModalView
	{
		// Token: 0x06000B64 RID: 2916 RVA: 0x00031220 File Offset: 0x0002F420
		public override void OnDespawn()
		{
			base.OnDespawn();
			for (int i = 0; i < this.spawnedButtons.Length; i++)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIGenericModalButtonIconAndTextView>(this.spawnedButtons[i]);
			}
			this.spawnedButtons = new UIGenericModalButtonIconAndTextView[0];
		}

		// Token: 0x06000B65 RID: 2917 RVA: 0x00031264 File Offset: 0x0002F464
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			string text = modalData[0] as string;
			Sprite sprite = modalData[1] as Sprite;
			string text2 = modalData[2] as string;
			UIModalGenericViewAction[] array = modalData[3] as UIModalGenericViewAction[];
			this.titleText.text = text;
			this.titleIconSpriteImage.sprite = sprite;
			this.titleIconSpriteContainer.gameObject.SetActive(sprite != null);
			this.bodyText.text = text2;
			this.spawnedButtons = new UIGenericModalButtonIconAndTextView[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIGenericModalButtonIconAndTextView uigenericModalButtonIconAndTextView = this.buttonSource;
				Transform transform = this.buttonsContainer;
				UIGenericModalButtonIconAndTextView uigenericModalButtonIconAndTextView2 = instance.Spawn<UIGenericModalButtonIconAndTextView>(uigenericModalButtonIconAndTextView, default(Vector3), default(Quaternion), transform);
				this.spawnedButtons[i] = uigenericModalButtonIconAndTextView2;
				uigenericModalButtonIconAndTextView2.SetUp(array[i]);
			}
		}

		// Token: 0x04000749 RID: 1865
		[Header("UIGenericModalView")]
		[SerializeField]
		private TextMeshProUGUI titleText;

		// Token: 0x0400074A RID: 1866
		[SerializeField]
		private Image titleIconSpriteImage;

		// Token: 0x0400074B RID: 1867
		[SerializeField]
		private GameObject titleIconSpriteContainer;

		// Token: 0x0400074C RID: 1868
		[SerializeField]
		private TextMeshProUGUI bodyText;

		// Token: 0x0400074D RID: 1869
		[SerializeField]
		private RectTransform buttonsContainer;

		// Token: 0x0400074E RID: 1870
		[SerializeField]
		private UIGenericModalButtonIconAndTextView buttonSource;

		// Token: 0x0400074F RID: 1871
		private UIGenericModalButtonIconAndTextView[] spawnedButtons = new UIGenericModalButtonIconAndTextView[0];
	}
}
