namespace Endless.Gameplay.LuaInterfaces;

internal class Destroyable
{
	private readonly DestroyableComponent destroyable;

	internal Destroyable(DestroyableComponent destroyableComponent)
	{
		destroyable = destroyableComponent;
	}
}
