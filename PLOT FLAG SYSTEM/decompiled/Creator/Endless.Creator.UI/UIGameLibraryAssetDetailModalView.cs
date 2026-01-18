using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetDetailModalView : UIEscapableModalView
{
	[SerializeField]
	private UIButton editButton;

	[SerializeField]
	private UIButton duplicateButton;

	[SerializeField]
	private UIGameAssetDetailView gameAssetDetail;

	[SerializeField]
	private UIModalMatchCloseHandler modalMatchCloseHandler;

	private AssetContexts assetContext;

	private bool initialized;

	public override void OnDespawn()
	{
		base.OnDespawn();
		gameAssetDetail.Clear();
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		if (!initialized)
		{
			gameAssetDetail.UserRolesModel.OnLocalClientRoleSet.AddListener(HandleEditButtonVisibility);
			initialized = true;
		}
		UIGameAsset model = (UIGameAsset)modalData[0];
		assetContext = (AssetContexts)modalData[1];
		modalMatchCloseHandler.enabled = assetContext != AssetContexts.MainMenu;
		editButton.gameObject.SetActive(value: false);
		duplicateButton.gameObject.SetActive(value: false);
		gameAssetDetail.SetContext(assetContext);
		gameAssetDetail.View(model);
	}

	public override void Close()
	{
		base.Close();
		modalMatchCloseHandler.enabled = false;
	}

	private void HandleEditButtonVisibility(Roles localClientRole)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleEditButtonVisibility", localClientRole);
		}
		bool active = assetContext == AssetContexts.MainMenu && localClientRole.IsGreaterThanOrEqualTo(Roles.Editor);
		editButton.gameObject.SetActive(active);
	}
}
