namespace Endless.Gameplay;

public static class EnumExtensions
{
	public static bool Contains(this AppearanceIKController.IKMode mode, AppearanceIKController.IKMode otherMode)
	{
		return (mode & otherMode) != 0;
	}
}
