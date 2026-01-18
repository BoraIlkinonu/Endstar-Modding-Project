using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIGameLibraryAssetReplacementModalView))]
public class UIGameLibraryAssetReplacementModalController : UIGameObject
{
	[SerializeField]
	private UIGameLibraryAssetReplacementModalView view;

	[SerializeField]
	private UIGameLibraryAssetReplacementConfirmationModalView gameLibraryEntryRemoveConfirmationModalSource;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		UIGameAssetListCellController.SelectAction = (Action<UIGameAsset>)Delegate.Combine(UIGameAssetListCellController.SelectAction, new Action<UIGameAsset>(OnSelect));
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		UIGameAssetListCellController.SelectAction = (Action<UIGameAsset>)Delegate.Remove(UIGameAssetListCellController.SelectAction, new Action<UIGameAsset>(OnSelect));
	}

	private void OnSelect(UIGameAsset toReplace)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelect", toReplace);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameLibraryEntryRemoveConfirmationModalSource, UIModalManagerStackActions.PopStack, view.ToRemove, toReplace);
	}
}
