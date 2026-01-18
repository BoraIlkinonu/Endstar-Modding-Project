using System;
using Endless.Shared.EndlessQualitySettings;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AD RID: 173
	public abstract class UIBaseQualityLevelHandler : UIGameObject
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060003C2 RID: 962 RVA: 0x0001383A File Offset: 0x00011A3A
		// (set) Token: 0x060003C3 RID: 963 RVA: 0x00013842 File Offset: 0x00011A42
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060003C4 RID: 964
		public abstract bool IsMobileSupported { get; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x060003C5 RID: 965 RVA: 0x0001384B File Offset: 0x00011A4B
		// (set) Token: 0x060003C6 RID: 966 RVA: 0x00013853 File Offset: 0x00011A53
		public bool IsChanged { get; protected set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x060003C7 RID: 967 RVA: 0x0001385C File Offset: 0x00011A5C
		protected bool IsCustom
		{
			get
			{
				return this.QualityMenu.GetCurrentQualityPresetName() == "Custom";
			}
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x00013873 File Offset: 0x00011A73
		protected virtual void OnDisable()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("OnDisable", this);
			}
			this.IsChanged = false;
		}

		// Token: 0x060003C9 RID: 969
		public abstract void Initialize();

		// Token: 0x060003CA RID: 970
		public abstract void Apply();

		// Token: 0x060003CB RID: 971 RVA: 0x0001388F File Offset: 0x00011A8F
		public void Reinitialize()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Reinitialize", this);
			}
			this.IsChanged = false;
			this.Initialize();
		}

		// Token: 0x040002C0 RID: 704
		[SerializeField]
		protected QualityMenu QualityMenu;
	}
}
