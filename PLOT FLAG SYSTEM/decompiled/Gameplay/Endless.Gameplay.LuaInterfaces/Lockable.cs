using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Lockable
{
	private Endless.Gameplay.Lockable lockable;

	internal Lockable(Endless.Gameplay.Lockable lockable)
	{
		this.lockable = lockable;
	}

	public KeyLibraryReference GetKeyReference()
	{
		return lockable.KeyReference;
	}

	public void Unlock(Context instigator)
	{
		lockable.Unlock(instigator);
	}

	public bool GetIsLocked()
	{
		return lockable.IsLocked;
	}
}
