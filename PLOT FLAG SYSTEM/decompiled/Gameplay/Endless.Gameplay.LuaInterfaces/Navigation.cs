using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Navigation
{
	private readonly DynamicNavigationComponent navigationComponent;

	internal Navigation(DynamicNavigationComponent dynamicNavigationComponent)
	{
		navigationComponent = dynamicNavigationComponent;
	}

	public void SetBlockingBehavior(Context instigator, bool isBlocking)
	{
		navigationComponent.SetBlockingBehavior(instigator, isBlocking);
	}
}
