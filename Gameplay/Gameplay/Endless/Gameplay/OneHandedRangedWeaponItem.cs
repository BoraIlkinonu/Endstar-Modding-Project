using System;
using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay
{
	// Token: 0x020002B6 RID: 694
	public class OneHandedRangedWeaponItem : RangedWeaponItem
	{
		// Token: 0x06000F8B RID: 3979 RVA: 0x000506C0 File Offset: 0x0004E8C0
		protected override void HandleVisualReferenceInitialized(ComponentReferences references)
		{
			OneHandedRangedWeaponVisualReferences oneHandedRangedWeaponVisualReferences = (OneHandedRangedWeaponVisualReferences)references;
			this.projectileShooter.SetupProjectileShooterReferences(oneHandedRangedWeaponVisualReferences.MuzzleFlashPoint, oneHandedRangedWeaponVisualReferences.EjectionPoint, oneHandedRangedWeaponVisualReferences.Magazine, oneHandedRangedWeaponVisualReferences.MagazineEjectionPoint, oneHandedRangedWeaponVisualReferences.EjectionSettings);
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x00050708 File Offset: 0x0004E908
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x0005071E File Offset: 0x0004E91E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x00050728 File Offset: 0x0004E928
		protected internal override string __getTypeName()
		{
			return "OneHandedRangedWeaponItem";
		}
	}
}
