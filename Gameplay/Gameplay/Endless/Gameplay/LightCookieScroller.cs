using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Gameplay
{
	// Token: 0x0200008A RID: 138
	public class LightCookieScroller : MonoBehaviour
	{
		// Token: 0x0600027B RID: 635 RVA: 0x0000DA42 File Offset: 0x0000BC42
		private void OnValidate()
		{
			if (this.lightData == null)
			{
				this.lightData = base.GetComponent<UniversalAdditionalLightData>();
			}
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0000DA42 File Offset: 0x0000BC42
		private void Start()
		{
			if (this.lightData == null)
			{
				this.lightData = base.GetComponent<UniversalAdditionalLightData>();
			}
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0000DA60 File Offset: 0x0000BC60
		private void Update()
		{
			if (this.lightData == null)
			{
				return;
			}
			if (this.lightData.lightCookieOffset.x >= this.scrollWrap)
			{
				this.lightData.lightCookieOffset = Vector2.zero;
				return;
			}
			this.lightData.lightCookieOffset += new Vector2(this.scrollSpeed, 0f) * Time.deltaTime;
		}

		// Token: 0x04000267 RID: 615
		[SerializeField]
		private float scrollSpeed;

		// Token: 0x04000268 RID: 616
		[SerializeField]
		private float scrollWrap;

		// Token: 0x04000269 RID: 617
		[SerializeField]
		private UniversalAdditionalLightData lightData;
	}
}
