using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000EE RID: 238
	public class PostProcessQualityHandler : MonoBehaviour
	{
		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x060005AF RID: 1455 RVA: 0x00018520 File Offset: 0x00016720
		private Dictionary<PostProcessQuality.PostProcessQualityLevel, VolumeProfile> QualityMap
		{
			get
			{
				if (this.qualityMap == null)
				{
					this.qualityMap = new Dictionary<PostProcessQuality.PostProcessQualityLevel, VolumeProfile>();
					foreach (PostProcessQualityHandler.VolumeProfilePair volumeProfilePair in this.volumeQualities)
					{
						this.qualityMap.Add(volumeProfilePair.QualityLevel, volumeProfilePair.Profile);
					}
				}
				return this.qualityMap;
			}
		}

		// Token: 0x060005B0 RID: 1456 RVA: 0x0001859C File Offset: 0x0001679C
		private void OnValidate()
		{
			if (this.postProcessVolume == null)
			{
				this.postProcessVolume = base.GetComponent<Volume>();
			}
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x000185B8 File Offset: 0x000167B8
		private void Start()
		{
			this.HandleQualityLevelChanged(PostProcessQuality.CurrentQualityLevel);
			PostProcessQuality.PostProcessQualityLevelChanged.AddListener(new UnityAction<PostProcessQuality.PostProcessQualityLevel>(this.HandleQualityLevelChanged));
		}

		// Token: 0x060005B2 RID: 1458 RVA: 0x000185DC File Offset: 0x000167DC
		private void HandleQualityLevelChanged(PostProcessQuality.PostProcessQualityLevel currentQualityLevel)
		{
			if (this.QualityMap.ContainsKey(currentQualityLevel))
			{
				this.postProcessVolume.profile = this.QualityMap[currentQualityLevel];
				return;
			}
			Debug.LogWarning(string.Format("No profile for {0}. Using default. {1}", currentQualityLevel, base.gameObject.name), base.gameObject);
			this.postProcessVolume.profile = null;
		}

		// Token: 0x04000316 RID: 790
		[SerializeField]
		private Volume postProcessVolume;

		// Token: 0x04000317 RID: 791
		[SerializeField]
		private List<PostProcessQualityHandler.VolumeProfilePair> volumeQualities = new List<PostProcessQualityHandler.VolumeProfilePair>();

		// Token: 0x04000318 RID: 792
		private Dictionary<PostProcessQuality.PostProcessQualityLevel, VolumeProfile> qualityMap;

		// Token: 0x020000EF RID: 239
		[Serializable]
		public class VolumeProfilePair
		{
			// Token: 0x04000319 RID: 793
			public PostProcessQuality.PostProcessQualityLevel QualityLevel;

			// Token: 0x0400031A RID: 794
			public VolumeProfile Profile;
		}
	}
}
