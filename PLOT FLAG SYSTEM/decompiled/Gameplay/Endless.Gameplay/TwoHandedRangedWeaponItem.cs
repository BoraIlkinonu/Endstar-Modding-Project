using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay;

public class TwoHandedRangedWeaponItem : RangedWeaponItem
{
	protected override void HandleVisualReferenceInitialized(ComponentReferences references)
	{
		TwoHandedRangedWeaponVisualReferences twoHandedRangedWeaponVisualReferences = (TwoHandedRangedWeaponVisualReferences)references;
		projectileShooter.SetupProjectileShooterReferences(twoHandedRangedWeaponVisualReferences.MuzzleFlashPoint, twoHandedRangedWeaponVisualReferences.EjectionPoint, twoHandedRangedWeaponVisualReferences.Magazine, twoHandedRangedWeaponVisualReferences.MagazineEjectionPoint, twoHandedRangedWeaponVisualReferences.EjectionSettings);
		offhandPlacement = twoHandedRangedWeaponVisualReferences.OffhandPlacement;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "TwoHandedRangedWeaponItem";
	}
}
