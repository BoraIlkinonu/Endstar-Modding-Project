namespace Endless.Creator.UI;

public abstract class UIAssetWithScreenshotsController : UIAssetReadAndWriteController
{
	protected abstract void RemoveScreenshot(int index);

	protected abstract void RearrangeScreenshots(int oldIndex, int newIndex);
}
