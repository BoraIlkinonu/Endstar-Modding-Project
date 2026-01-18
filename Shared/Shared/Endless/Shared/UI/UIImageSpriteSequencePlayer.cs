using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000145 RID: 325
	[RequireComponent(typeof(Image))]
	public class UIImageSpriteSequencePlayer : UIGameObject
	{
		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000805 RID: 2053 RVA: 0x00021D79 File Offset: 0x0001FF79
		private Image Image
		{
			get
			{
				if (!this.image)
				{
					base.TryGetComponent<Image>(out this.image);
				}
				return this.image;
			}
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00021D9B File Offset: 0x0001FF9B
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (this.play && this.coroutine == null)
			{
				this.Play();
			}
		}

		// Token: 0x06000807 RID: 2055 RVA: 0x00021DCC File Offset: 0x0001FFCC
		public void Play()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Play", Array.Empty<object>());
			}
			this.play = true;
			if (!base.gameObject.activeInHierarchy || this.coroutine != null)
			{
				return;
			}
			this.coroutine = base.StartCoroutine(this.PlayCoroutine());
		}

		// Token: 0x06000808 RID: 2056 RVA: 0x00021E20 File Offset: 0x00020020
		public void Stop()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Stop", Array.Empty<object>());
			}
			this.play = false;
			if (this.coroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.coroutine);
			this.coroutine = null;
		}

		// Token: 0x06000809 RID: 2057 RVA: 0x00021E5D File Offset: 0x0002005D
		private IEnumerator PlayCoroutine()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayCoroutine", Array.Empty<object>());
			}
			for (;;)
			{
				this.index++;
				if (this.index >= this.sprites.Length)
				{
					this.index = 0;
				}
				this.Image.sprite = this.sprites[this.index];
				yield return new WaitForSecondsRealtime(this.nextSpriteDelay);
			}
			yield break;
		}

		// Token: 0x040004DB RID: 1243
		[Tooltip("In seconds")]
		[SerializeField]
		private float nextSpriteDelay = 0.1f;

		// Token: 0x040004DC RID: 1244
		[SerializeField]
		private Sprite[] sprites = new Sprite[0];

		// Token: 0x040004DD RID: 1245
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040004DE RID: 1246
		private Image image;

		// Token: 0x040004DF RID: 1247
		private Coroutine coroutine;

		// Token: 0x040004E0 RID: 1248
		private int index;

		// Token: 0x040004E1 RID: 1249
		private bool play;
	}
}
