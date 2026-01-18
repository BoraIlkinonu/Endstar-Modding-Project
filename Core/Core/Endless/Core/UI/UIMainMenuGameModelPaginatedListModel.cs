using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000060 RID: 96
	public class UIMainMenuGameModelPaginatedListModel : UIBaseAssetCloudPaginatedListModel<MainMenuGameModel>
	{
		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060001BB RID: 443 RVA: 0x0000A8A6 File Offset: 0x00008AA6
		// (set) Token: 0x060001BC RID: 444 RVA: 0x0000A8AE File Offset: 0x00008AAE
		public MainMenuGameContext MainMenuGameContext { get; private set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060001BD RID: 445 RVA: 0x0000A8B7 File Offset: 0x00008AB7
		protected override string AssetType
		{
			get
			{
				return "game";
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060001BE RID: 446 RVA: 0x00003CF2 File Offset: 0x00001EF2
		protected override bool PopulateRefs
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060001BF RID: 447 RVA: 0x0000A8BE File Offset: 0x00008ABE
		protected override MainMenuGameModel SkeletonData { get; } = new MainMenuGameModel();

		// Token: 0x060001C0 RID: 448 RVA: 0x0000A8C8 File Offset: 0x00008AC8
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (this.MainMenuGameContext == MainMenuGameContext.Admin && !EndlessCloudService.IsAdmin && !EndlessCloudService.IsModerator)
			{
				return;
			}
			if (this.requestOnEnable && !base.IsInitialized)
			{
				base.Request(null);
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000A91C File Offset: 0x00008B1C
		protected override Task<GraphQlResult> RequestPage(PaginationParams paginationParams, CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestPage", new object[] { paginationParams, cancellationToken });
			}
			AssetParams assetParams = new AssetParams(this.AssetFilter, this.PopulateRefs, MainMenuGameModel.AssetReturnArgs);
			switch (this.MainMenuGameContext)
			{
			case MainMenuGameContext.Edit:
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "role", this.role), this);
				}
				return EndlessServices.Instance.CloudService.GetAssetsByTypeAvailableForRoleAsync(this.AssetType, (int)this.role, assetParams, paginationParams, true, base.VerboseLogging);
			case MainMenuGameContext.Play:
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "publishState", this.publishState), this);
				}
				return EndlessServices.Instance.CloudService.GetAssetsByTypeAndPublishStateAsync(this.AssetType, this.publishState.ToEndlessCloudServicesCompatibleString(), assetParams, paginationParams, base.VerboseLogging);
			case MainMenuGameContext.Admin:
				assetParams.PopulateRefs = false;
				return EndlessServices.Instance.CloudService.GetAssetsByTypeAsync(this.AssetType, assetParams, paginationParams, false, true, 60);
			default:
				throw new ArgumentOutOfRangeException("MainMenuGameContext", string.Format("No support for {0}!", this.MainMenuGameContext));
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000AA64 File Offset: 0x00008C64
		protected override void HandleError(Exception exception)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleError", new object[] { exception });
			}
			switch (this.MainMenuGameContext)
			{
			case MainMenuGameContext.Edit:
				ErrorHandler.HandleError(ErrorCodes.UIMainMenuGameModelListModel_GetGamesWithRole, exception, true, false);
				return;
			case MainMenuGameContext.Play:
				ErrorHandler.HandleError(ErrorCodes.UIMainMenuGameModelListModel_GetPublishedGames, exception, true, false);
				return;
			case MainMenuGameContext.Admin:
				ErrorHandler.HandleError(ErrorCodes.UIMainMenuGameModelListModel_GetUserReportedGames, exception, true, false);
				return;
			default:
				throw new ArgumentOutOfRangeException("MainMenuGameContext", string.Format("No support for {0}!", this.MainMenuGameContext));
			}
		}

		// Token: 0x0400013C RID: 316
		[SerializeField]
		private Roles role = Roles.Owner;

		// Token: 0x0400013D RID: 317
		[SerializeField]
		private UIPublishStates publishState = UIPublishStates.Public;

		// Token: 0x0400013E RID: 318
		[SerializeField]
		private bool requestOnEnable = true;

		// Token: 0x0400013F RID: 319
		private CancellationTokenSource cancelTokenSource;
	}
}
