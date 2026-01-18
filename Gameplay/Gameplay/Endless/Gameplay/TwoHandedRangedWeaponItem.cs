using System;
using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay
{
	// Token: 0x020002BC RID: 700
	public class TwoHandedRangedWeaponItem : RangedWeaponItem
	{
		// Token: 0x06000FF3 RID: 4083 RVA: 0x00051C40 File Offset: 0x0004FE40
		protected override void HandleVisualReferenceInitialized(ComponentReferences references)
		{
			TwoHandedRangedWeaponVisualReferences twoHandedRangedWeaponVisualReferences = (TwoHandedRangedWeaponVisualReferences)references;
			this.projectileShooter.SetupProjectileShooterReferences(twoHandedRangedWeaponVisualReferences.MuzzleFlashPoint, twoHandedRangedWeaponVisualReferences.EjectionPoint, twoHandedRangedWeaponVisualReferences.Magazine, twoHandedRangedWeaponVisualReferences.MagazineEjectionPoint, twoHandedRangedWeaponVisualReferences.EjectionSettings);
			this.offhandPlacement = twoHandedRangedWeaponVisualReferences.OffhandPlacement;
		}

		// Token: 0x06000FF5 RID: 4085 RVA: 0x00051C8C File Offset: 0x0004FE8C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FF6 RID: 4086 RVA: 0x0005071E File Offset: 0x0004E91E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FF7 RID: 4087 RVA: 0x00051CA2 File Offset: 0x0004FEA2
		protected internal override string __getTypeName()
		{
			return "TwoHandedRangedWeaponItem";
		}
	}
}
