using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F6 RID: 246
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/QualityPreset")]
	public class QualityPreset : ScriptableObject
	{
		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060005DE RID: 1502 RVA: 0x00018E2C File Offset: 0x0001702C
		// (set) Token: 0x060005DF RID: 1503 RVA: 0x00018E34 File Offset: 0x00017034
		public HighDynamicRangeQuality HighDynamicRangeQuality { get; private set; }

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060005E0 RID: 1504 RVA: 0x00018E3D File Offset: 0x0001703D
		// (set) Token: 0x060005E1 RID: 1505 RVA: 0x00018E45 File Offset: 0x00017045
		public LightingQuality LightingQuality { get; private set; }

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060005E2 RID: 1506 RVA: 0x00018E4E File Offset: 0x0001704E
		// (set) Token: 0x060005E3 RID: 1507 RVA: 0x00018E56 File Offset: 0x00017056
		public TextureQuality TextureQuality { get; private set; }

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060005E4 RID: 1508 RVA: 0x00018E5F File Offset: 0x0001705F
		// (set) Token: 0x060005E5 RID: 1509 RVA: 0x00018E67 File Offset: 0x00017067
		public AntialiasingQuality AntiAliasingQuality { get; private set; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060005E6 RID: 1510 RVA: 0x00018E70 File Offset: 0x00017070
		// (set) Token: 0x060005E7 RID: 1511 RVA: 0x00018E78 File Offset: 0x00017078
		public ShaderQuality ShaderQuality { get; private set; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x00018E81 File Offset: 0x00017081
		// (set) Token: 0x060005E9 RID: 1513 RVA: 0x00018E89 File Offset: 0x00017089
		public PostProcessQuality PostProcessQuality { get; private set; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060005EA RID: 1514 RVA: 0x00018E92 File Offset: 0x00017092
		// (set) Token: 0x060005EB RID: 1515 RVA: 0x00018E9A File Offset: 0x0001709A
		public ModelQuality ModelQuality { get; private set; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060005EC RID: 1516 RVA: 0x00018EA4 File Offset: 0x000170A4
		public IReadOnlyList<QualityOption> QualityOptions
		{
			get
			{
				if (this.builtQualityOptions == null)
				{
					this.builtQualityOptions = new List<QualityOption> { this.HighDynamicRangeQuality, this.LightingQuality, this.TextureQuality, this.AntiAliasingQuality, this.ShaderQuality, this.PostProcessQuality, this.ModelQuality };
				}
				return this.builtQualityOptions;
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060005ED RID: 1517 RVA: 0x00018F1E File Offset: 0x0001711E
		public string DisplayName
		{
			get
			{
				if (!MobileUtility.IsMobile)
				{
					return this.displayName;
				}
				return this.mobileDisplayName;
			}
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x00018F34 File Offset: 0x00017134
		public void SaveIndividualSettings()
		{
			foreach (QualityOption qualityOption in this.QualityOptions)
			{
				Debug.Log("Saving " + qualityOption.SaveKey + " -> " + qualityOption.DisplayName);
				PlayerPrefs.SetString(qualityOption.SaveKey, qualityOption.DisplayName);
			}
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x00018FAC File Offset: 0x000171AC
		public void ClearIndividualSettings()
		{
			foreach (QualityOption qualityOption in this.QualityOptions)
			{
				PlayerPrefs.DeleteKey(qualityOption.SaveKey);
			}
		}

		// Token: 0x0400033D RID: 829
		[SerializeField]
		private string displayName = "Unnamed";

		// Token: 0x0400033E RID: 830
		[SerializeField]
		private string mobileDisplayName = "Unnamed";

		// Token: 0x0400033F RID: 831
		private IReadOnlyList<QualityOption> builtQualityOptions;
	}
}
