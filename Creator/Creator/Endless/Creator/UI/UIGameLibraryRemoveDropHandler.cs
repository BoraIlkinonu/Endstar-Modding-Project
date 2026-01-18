using System;
using System.Threading.Tasks;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000DB RID: 219
	public class UIGameLibraryRemoveDropHandler : UIRemoveDropHandler, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600039B RID: 923 RVA: 0x000175AE File Offset: 0x000157AE
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600039C RID: 924 RVA: 0x000175B6 File Offset: 0x000157B6
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600039D RID: 925 RVA: 0x000175C0 File Offset: 0x000157C0
		protected override void OnDroppedGameObject(GameObject droppedGameObject)
		{
			base.OnDroppedGameObject(droppedGameObject);
			IListCellViewable listCellViewable;
			if (!droppedGameObject.TryGetComponent<IListCellViewable>(out listCellViewable))
			{
				if (base.VerboseLogging)
				{
					DebugUtility.Log("Could not get IListCellViewable from " + droppedGameObject.DebugSafeName(true), this);
				}
				return;
			}
			try
			{
				UIGameAsset uigameAsset = (UIGameAsset)listCellViewable.ModelAsObject;
				this.toRemove = uigameAsset;
				if (this.toRemove.Type == UIGameAssetTypes.Terrain)
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameLibraryEntryReplacementModalSource, UIModalManagerStackActions.MaintainStack, new object[]
					{
						this.toRemove,
						this.listView.ListCellSizeType
					});
				}
				else if (this.toRemove.Type == UIGameAssetTypes.Prop)
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to remove " + this.toRemove.Name + " from the game? This will remove it from all levels.", new Action(this.ConfirmRemoveProp), new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack), UIModalManagerStackActions.MaintainStack);
				}
				else
				{
					UIGameAssetTypes type = this.toRemove.Type;
					if (type == UIGameAssetTypes.SFX || type == UIGameAssetTypes.Music || type == UIGameAssetTypes.Ambient)
					{
						MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Are you sure you want to remove " + this.toRemove.Name + " from the game? This will remove it from all levels.", new Action(this.ConfirmRemoveAudio), new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack), UIModalManagerStackActions.MaintainStack);
					}
				}
			}
			catch (InvalidCastException)
			{
				Debug.LogError("Could not cast listCellViewable.ModelAsObject to a type of UIGameAsset!", this);
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00017730 File Offset: 0x00015930
		private void ConfirmRemoveProp()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmRemoveProp", Array.Empty<object>());
			}
			this.RemoveAsset(new Func<SerializableGuid, Task<bool>>(MonoBehaviourSingleton<GameEditor>.Instance.RemovePropFromGameLibrary));
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0001776A File Offset: 0x0001596A
		private void ConfirmRemoveAudio()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmRemoveAudio", Array.Empty<object>());
			}
			this.RemoveAsset(new Func<SerializableGuid, Task<bool>>(MonoBehaviourSingleton<GameEditor>.Instance.RemoveAudioFromGameLibrary));
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x000177A4 File Offset: 0x000159A4
		private async void RemoveAsset(Func<SerializableGuid, Task<bool>> removeFunction)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveAsset", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			await removeFunction(this.toRemove.AssetID);
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x040003CA RID: 970
		[Header("UIGameLibraryRemoveDropHandler")]
		[SerializeField]
		private UIBaseListView<UIGameAsset> listView;

		// Token: 0x040003CB RID: 971
		[SerializeField]
		private UIGameLibraryAssetReplacementModalView gameLibraryEntryReplacementModalSource;

		// Token: 0x040003CC RID: 972
		private UIGameAsset toRemove;
	}
}
