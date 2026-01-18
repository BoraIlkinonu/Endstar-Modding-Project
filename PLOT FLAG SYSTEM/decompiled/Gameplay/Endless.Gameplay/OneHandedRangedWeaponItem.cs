using Endless.Props.ReferenceComponents;

namespace Endless.Gameplay;

public class OneHandedRangedWeaponItem : RangedWeaponItem
{
	protected override void HandleVisualReferenceInitialized(ComponentReferences references)
	{
		OneHandedRangedWeaponVisualReferences oneHandedRangedWeaponVisualReferences = (OneHandedRangedWeaponVisualReferences)references;
		projectileShooter.SetupProjectileShooterReferences(oneHandedRangedWeaponVisualReferences.MuzzleFlashPoint, oneHandedRangedWeaponVisualReferences.EjectionPoint, oneHandedRangedWeaponVisualReferences.Magazine, oneHandedRangedWeaponVisualReferences.MagazineEjectionPoint, oneHandedRangedWeaponVisualReferences.EjectionSettings);
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
		return "OneHandedRangedWeaponItem";
	}
}
