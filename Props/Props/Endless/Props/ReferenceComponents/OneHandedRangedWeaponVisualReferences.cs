using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200001A RID: 26
	public class OneHandedRangedWeaponVisualReferences : ComponentReferences
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000078 RID: 120 RVA: 0x00002BD4 File Offset: 0x00000DD4
		public Transform MuzzleFlashPoint
		{
			get
			{
				return this.muzzleFlashPoint;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00002BDC File Offset: 0x00000DDC
		public Transform EjectionPoint
		{
			get
			{
				return this.ejectionPoint;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00002BE4 File Offset: 0x00000DE4
		public Transform Magazine
		{
			get
			{
				return this.magazine;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00002BEC File Offset: 0x00000DEC
		public Transform MagazineEjectionPoint
		{
			get
			{
				return this.magazineEjectionPoint;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600007C RID: 124 RVA: 0x00002BF4 File Offset: 0x00000DF4
		public ProjectileShooterEjectionSettings EjectionSettings
		{
			get
			{
				return this.ejectionSettings;
			}
		}

		// Token: 0x0400004A RID: 74
		[SerializeField]
		private Transform muzzleFlashPoint;

		// Token: 0x0400004B RID: 75
		[SerializeField]
		private Transform ejectionPoint;

		// Token: 0x0400004C RID: 76
		[SerializeField]
		private Transform magazine;

		// Token: 0x0400004D RID: 77
		[SerializeField]
		private Transform magazineEjectionPoint;

		// Token: 0x0400004E RID: 78
		[SerializeField]
		private ProjectileShooterEjectionSettings ejectionSettings;
	}
}
