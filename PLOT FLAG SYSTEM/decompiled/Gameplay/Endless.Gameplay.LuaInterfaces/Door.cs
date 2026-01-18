using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Door
{
	private readonly Endless.Gameplay.Door door;

	internal Door(Endless.Gameplay.Door door)
	{
		this.door = door;
	}

	public void Open(Context instigator, bool forward)
	{
		door.Open(instigator, forward);
	}

	public void OpenFromUser(Context instigator)
	{
		door.OpenFromUser(instigator);
	}

	public void ToggleOpenFromUser(Context instigator)
	{
		door.ToggleOpenFromUser(instigator);
	}

	public void Close(Context instigator)
	{
		door.Close(instigator);
	}
}
