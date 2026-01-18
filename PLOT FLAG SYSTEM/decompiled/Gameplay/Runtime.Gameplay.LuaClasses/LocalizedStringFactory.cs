using Endless.Shared;

namespace Runtime.Gameplay.LuaClasses;

public class LocalizedStringFactory
{
	private static LocalizedStringFactory instance;

	internal static LocalizedStringFactory Instance => instance ?? (instance = new LocalizedStringFactory());

	public LocalizedString Create(string text)
	{
		return new LocalizedString(text);
	}

	public LocalizedString Create(int language, string text)
	{
		return new LocalizedString((Language)language, text);
	}
}
