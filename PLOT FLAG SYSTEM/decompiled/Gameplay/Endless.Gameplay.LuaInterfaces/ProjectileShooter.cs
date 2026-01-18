using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class ProjectileShooter
{
	private readonly ProjectileShooterComponent component;

	internal ProjectileShooter(ProjectileShooterComponent projectileShooterComponent)
	{
		component = projectileShooterComponent;
	}

	public void Shoot(Context instigator)
	{
		component.Shoot();
	}
}
