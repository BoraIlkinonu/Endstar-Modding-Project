using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetListView : UIBaseListView<LevelAsset>, IRoleInteractable
{
	public enum SelectActions
	{
		OpenLevelLoaderModal,
		StartEditMatch
	}

	[Header("UILevelAssetListView")]
	[SerializeField]
	private UILevelAssetListRowView staticLevelAssetListRowSource;

	[SerializeField]
	private UILevelAssetListRowView draggableLevelAssetListRowSource;

	[field: SerializeField]
	public SelectActions SelectAction { get; private set; }

	public bool LocalUserCanInteract { get; private set; }

	protected override void Start()
	{
		base.Start();
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		SetCellSourceBasedOnLocalUserCanInteract();
	}

	public void SetLocalUserCanInteract(bool localUserCanInteract)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLocalUserCanInteract", localUserCanInteract);
		}
		if (LocalUserCanInteract != localUserCanInteract)
		{
			LocalUserCanInteract = localUserCanInteract;
			SetCellSourceBasedOnLocalUserCanInteract();
		}
	}

	private void SetCellSourceBasedOnLocalUserCanInteract()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetCellSourceBasedOnLocalUserCanInteract");
		}
		SetCellSource(LocalUserCanInteract ? draggableLevelAssetListRowSource : staticLevelAssetListRowSource);
	}
}
