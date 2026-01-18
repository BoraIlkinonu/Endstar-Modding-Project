using System;
using UnityEngine;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BA RID: 186
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Core/Scroll Sensitivity Range Settings", fileName = "Scroll Sensitivity Range Setting")]
	public class ScrollSensitivityRangeSettings : ScriptableObject
	{
		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600043C RID: 1084 RVA: 0x000155A1 File Offset: 0x000137A1
		// (set) Token: 0x0600043D RID: 1085 RVA: 0x000155A9 File Offset: 0x000137A9
		public float UserFacingMin { get; private set; } = 0.1f;

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x000155B2 File Offset: 0x000137B2
		// (set) Token: 0x0600043F RID: 1087 RVA: 0x000155BA File Offset: 0x000137BA
		public float UserFacingMax { get; private set; } = 10f;

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000440 RID: 1088 RVA: 0x000155C3 File Offset: 0x000137C3
		// (set) Token: 0x06000441 RID: 1089 RVA: 0x000155CB File Offset: 0x000137CB
		public float UserFacingDefault { get; private set; } = 5f;

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x000155D4 File Offset: 0x000137D4
		// (set) Token: 0x06000443 RID: 1091 RVA: 0x000155DC File Offset: 0x000137DC
		public float InternalSensitivityMin { get; private set; } = 0.006f;

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000444 RID: 1092 RVA: 0x000155E5 File Offset: 0x000137E5
		// (set) Token: 0x06000445 RID: 1093 RVA: 0x000155ED File Offset: 0x000137ED
		public float InternalSensitivityMax { get; private set; } = 0.594f;

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x06000446 RID: 1094 RVA: 0x000155F6 File Offset: 0x000137F6
		public float InternalDefaultSensitivity
		{
			get
			{
				return this.UserFacingValueToInternalValue(this.UserFacingDefault);
			}
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00015604 File Offset: 0x00013804
		public float UserFacingValueToInternalValue(float userFacingValue)
		{
			float num = Mathf.InverseLerp(this.UserFacingMin, this.UserFacingMax, userFacingValue);
			return Mathf.Lerp(this.InternalSensitivityMin, this.InternalSensitivityMax, num);
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00015638 File Offset: 0x00013838
		public float InternalValueToUserFacingValue(float internalValue)
		{
			float num = Mathf.InverseLerp(this.InternalSensitivityMin, this.InternalSensitivityMax, internalValue);
			return Mathf.Lerp(this.UserFacingMin, this.UserFacingMax, num);
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x0001566C File Offset: 0x0001386C
		private void OnValidate()
		{
			if (this.UserFacingMin >= this.UserFacingMax)
			{
				this.UserFacingMax = this.UserFacingMin + 0.1f;
			}
			if (this.InternalSensitivityMin >= this.InternalSensitivityMax)
			{
				this.InternalSensitivityMax = this.InternalSensitivityMin + 0.01f;
			}
			this.UserFacingDefault = Mathf.Clamp(this.UserFacingDefault, this.UserFacingMin, this.UserFacingMax);
		}

		// Token: 0x040002DC RID: 732
		private const float MIN = 0.0001f;
	}
}
