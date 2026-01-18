namespace Endless.Gameplay.Screenshotting;

public class ScreenshotOptions
{
	public bool HideCharacter { get; private set; }

	public bool HideUi { get; private set; } = true;

	public bool HidePlayerTags { get; private set; } = true;

	public ScreenshotOptions()
	{
		HideCharacter = false;
		HideUi = false;
	}

	public ScreenshotOptions(bool hideCharacter, bool hideUi = true, bool hidePlayerTags = true)
	{
		HideCharacter = hideCharacter;
		HideUi = hideUi;
		HidePlayerTags = hidePlayerTags;
	}
}
