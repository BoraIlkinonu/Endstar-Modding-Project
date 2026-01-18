using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class BouncePad
{
	private Endless.Gameplay.BouncePad bouncePad;

	internal BouncePad(Endless.Gameplay.BouncePad bouncePad)
	{
		this.bouncePad = bouncePad;
	}

	public void SetBounceHeight(Context instigator, int value)
	{
		bouncePad.SetBounceHeight(value);
	}

	public int GetBounceHeight()
	{
		return bouncePad.heightInfo.Value.Height;
	}

	public void Activate(Context instigator)
	{
		bouncePad.Activate();
	}

	public void Deactivate(Context instigator)
	{
		bouncePad.Deactivate();
	}

	public bool GetActive()
	{
		return bouncePad.toggleInfo.Value.Active;
	}
}
