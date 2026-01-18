using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000144 RID: 324
	[ExecuteInEditMode]
	[RequireComponent(typeof(Image))]
	public class UIImageColorMatcher : MonoBehaviour
	{
		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x00021C62 File Offset: 0x0001FE62
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

		// Token: 0x06000802 RID: 2050 RVA: 0x00021C84 File Offset: 0x0001FE84
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.imageToMatch)
			{
				if (Application.isPlaying)
				{
					DebugUtility.LogWarning(this, "OnEnable", "imageToMatch field is required!", Array.Empty<object>());
				}
				return;
			}
			this.Match();
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00021CDC File Offset: 0x0001FEDC
		public void Match()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Match", Array.Empty<object>());
			}
			this.Image.color = new Color(this.imageToMatch.color.r * this.darknTint, this.imageToMatch.color.g * this.darknTint, this.imageToMatch.color.b * this.darknTint, this.imageToMatch.color.a);
		}

		// Token: 0x040004D7 RID: 1239
		[SerializeField]
		private Image imageToMatch;

		// Token: 0x040004D8 RID: 1240
		[SerializeField]
		[Range(0f, 1f)]
		private float darknTint = 0.75f;

		// Token: 0x040004D9 RID: 1241
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040004DA RID: 1242
		private Image image;
	}
}
