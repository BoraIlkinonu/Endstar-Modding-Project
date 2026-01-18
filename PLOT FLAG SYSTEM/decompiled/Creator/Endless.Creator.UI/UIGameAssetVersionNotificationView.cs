using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetVersionNotificationView : UIGameObject
{
	[SerializeField]
	private GameObject upToDate;

	[SerializeField]
	private GameObject seenUpdate;

	[SerializeField]
	private GameObject newUpdate;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public void View(GameEditorAssetVersionManager.UpdateStatus updateStatus)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", updateStatus);
		}
		upToDate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.UpToDate);
		seenUpdate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.SeenUpdate);
		newUpdate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.NewUpdate);
	}
}
