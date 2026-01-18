using System;
using System.Threading.Tasks;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIGameLibraryRemoveDropHandler : UIRemoveDropHandler, IUILoadingSpinnerViewCompatible
{
	[Header("UIGameLibraryRemoveDropHandler")]
	[SerializeField]
	private UIBaseListView<UIGameAsset> listView;

	[SerializeField]
	private UIGameLibraryAssetReplacementModalView gameLibraryEntryReplacementModalSource;

	private UIGameAsset toRemove;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected override void OnDroppedGameObject(GameObject droppedGameObject)
	{
		base.OnDroppedGameObject(droppedGameObject);
		if (!droppedGameObject.TryGetComponent<IListCellViewable>(out var component))
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Could not get IListCellViewable from " + droppedGameObject.DebugSafeName(), this);
			}
			return;
		}
		try
		{
			UIGameAsset uIGameAsset = (UIGameAsset)component.ModelAsObject;
			toRemove = uIGameAsset;
			if (toRemove.Type == UIGameAssetTypes.Terrain)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(gameLibraryEntryReplacementModalSource, UIModalManagerStackActions.MaintainStack, toRemove, listView.ListCellSizeType);
				return;
			}
			if (toRemove.Type == UIGameAssetTypes.Prop)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to remove " + toRemove.Name + " from the game? This will remove it from all levels.", ConfirmRemoveProp, MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack);
				return;
			}
			UIGameAssetTypes type = toRemove.Type;
			if (type == UIGameAssetTypes.SFX || type == UIGameAssetTypes.Music || type == UIGameAssetTypes.Ambient)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to remove " + toRemove.Name + " from the game? This will remove it from all levels.", ConfirmRemoveAudio, MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack);
			}
		}
		catch (InvalidCastException)
		{
			Debug.LogError("Could not cast listCellViewable.ModelAsObject to a type of UIGameAsset!", this);
		}
	}

	private void ConfirmRemoveProp()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmRemoveProp");
		}
		RemoveAsset(MonoBehaviourSingleton<GameEditor>.Instance.RemovePropFromGameLibrary);
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private void ConfirmRemoveAudio()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmRemoveAudio");
		}
		RemoveAsset(MonoBehaviourSingleton<GameEditor>.Instance.RemoveAudioFromGameLibrary);
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private async void RemoveAsset(Func<SerializableGuid, Task<bool>> removeFunction)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveAsset");
		}
		OnLoadingStarted.Invoke();
		await removeFunction(toRemove.AssetID);
		OnLoadingEnded.Invoke();
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}
}
