using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200001D RID: 29
	public class TwoHandedRangedWeaponVisualReferences : ComponentReferences
	{
		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000080 RID: 128 RVA: 0x00002C14 File Offset: 0x00000E14
		public Transform MuzzleFlashPoint
		{
			get
			{
				return this.muzzleFlashPoint;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00002C1C File Offset: 0x00000E1C
		public Transform EjectionPoint
		{
			get
			{
				return this.ejectionPoint;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000082 RID: 130 RVA: 0x00002C24 File Offset: 0x00000E24
		public Transform Magazine
		{
			get
			{
				return this.magazine;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00002C2C File Offset: 0x00000E2C
		public Transform MagazineEjectionPoint
		{
			get
			{
				return this.magazineEjectionPoint;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000084 RID: 132 RVA: 0x00002C34 File Offset: 0x00000E34
		public Transform OffhandPlacement
		{
			get
			{
				return this.offhandPlacement;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000085 RID: 133 RVA: 0x00002C3C File Offset: 0x00000E3C
		public ProjectileShooterEjectionSettings EjectionSettings
		{
			get
			{
				return this.ejectionSettings;
			}
		}

		// Token: 0x0400004F RID: 79
		[SerializeField]
		private Transform muzzleFlashPoint;

		// Token: 0x04000050 RID: 80
		[SerializeField]
		private Transform ejectionPoint;

		// Token: 0x04000051 RID: 81
		[SerializeField]
		private Transform magazine;

		// Token: 0x04000052 RID: 82
		[SerializeField]
		private Transform magazineEjectionPoint;

		// Token: 0x04000053 RID: 83
		[SerializeField]
		private Transform offhandPlacement;

		// Token: 0x04000054 RID: 84
		[SerializeField]
		private ProjectileShooterEjectionSettings ejectionSettings;
	}
}
