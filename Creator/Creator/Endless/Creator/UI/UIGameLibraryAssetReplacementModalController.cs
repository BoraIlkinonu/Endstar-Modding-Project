using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001B7 RID: 439
	[RequireComponent(typeof(UIGameLibraryAssetReplacementModalView))]
	public class UIGameLibraryAssetReplacementModalController : UIGameObject
	{
		// Token: 0x06000686 RID: 1670 RVA: 0x00021BDE File Offset: 0x0001FDDE
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIGameAssetListCellController.SelectAction = (Action<UIGameAsset>)Delegate.Combine(UIGameAssetListCellController.SelectAction, new Action<UIGameAsset>(this.OnSelect));
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x00021C18 File Offset: 0x0001FE18
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIGameAssetListCellController.SelectAction = (Action<UIGameAsset>)Delegate.Remove(UIGameAssetListCellController.SelectAction, new Action<UIGameAsset>(this.OnSelect));
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00021C54 File Offset: 0x0001FE54
		private void OnSelect(UIGameAsset toReplace)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", new object[] { toReplace });
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameLibraryEntryRemoveConfirmationModalSource, UIModalManagerStackActions.PopStack, new object[]
			{
				this.view.ToRemove,
				toReplace
			});
		}

		// Token: 0x040005DD RID: 1501
		[SerializeField]
		private UIGameLibraryAssetReplacementModalView view;

		// Token: 0x040005DE RID: 1502
		[SerializeField]
		private UIGameLibraryAssetReplacementConfirmationModalView gameLibraryEntryRemoveConfirmationModalSource;

		// Token: 0x040005DF RID: 1503
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
