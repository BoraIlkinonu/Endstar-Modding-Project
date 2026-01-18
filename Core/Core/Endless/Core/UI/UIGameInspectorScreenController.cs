using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Creator.UI;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200007A RID: 122
	public sealed class UIGameInspectorScreenController : UIGameObject
	{
		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000263 RID: 611 RVA: 0x0000D3B7 File Offset: 0x0000B5B7
		private UIGameInspectorScreenModel Model
		{
			get
			{
				return this.view.Model;
			}
		}

		// Token: 0x06000264 RID: 612 RVA: 0x0000D3C4 File Offset: 0x0000B5C4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.adminButton.gameObject.SetActive(this.Model.MainMenuGameContext == MainMenuGameContext.Admin);
			if (this.Model.MainMenuGameContext == MainMenuGameContext.Admin && (EndlessCloudService.IsAdmin || EndlessCloudService.IsModerator))
			{
				this.adminButton.onClick.AddListener(new UnityAction(this.Admin));
			}
			this.editButton.onClick.AddListener(new UnityAction(this.Edit));
			this.playButton.onClick.AddListener(new UnityAction(this.Play));
			this.copyShareLinkButton.onClick.AddListener(new UnityAction(this.CopyShareLink));
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000D494 File Offset: 0x0000B694
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			if (EndlessCloudService.IsAdmin || EndlessCloudService.IsModerator)
			{
				this.adminButton.onClick.RemoveListener(new UnityAction(this.Admin));
			}
			this.editButton.onClick.RemoveListener(new UnityAction(this.Edit));
			this.playButton.onClick.RemoveListener(new UnityAction(this.Play));
			this.copyShareLinkButton.onClick.RemoveListener(new UnityAction(this.CopyShareLink));
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000D538 File Offset: 0x0000B738
		private void Admin()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Admin", Array.Empty<object>());
			}
			if (EndlessCloudService.IsAdmin || EndlessCloudService.IsModerator)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.levelStateSelectionModalSource, UIModalManagerStackActions.ClearStack, new object[]
				{
					this.Model.MainMenuGameModel,
					true
				});
			}
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0000D59C File Offset: 0x0000B79C
		private void Edit()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Edit", Array.Empty<object>());
			}
			bool flag = this.Model.MainMenuGameContext == MainMenuGameContext.Admin;
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.levelStateSelectionModalSource, UIModalManagerStackActions.ClearStack, new object[]
			{
				this.Model.MainMenuGameModel,
				flag
			});
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000D600 File Offset: 0x0000B800
		private void Play()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Play", Array.Empty<object>());
			}
			List<AssetReference> levels = this.Model.MainMenuGameModel.Levels;
			if (levels.Count == 0)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayErrorModal("There must be at least one level!");
				return;
			}
			string text = ((this.Model.MainMenuGameContext == MainMenuGameContext.Play) ? this.Model.MainMenuGameModel.AssetVersion : null);
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(this.Model.MainMenuGameModel.AssetID, text, levels[0].AssetID, MainMenuGameContext.Play);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0000D698 File Offset: 0x0000B898
		private void CopyShareLink()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CopyShareLink", Array.Empty<object>());
			}
			NetworkEnvironment networkEnv = MatchmakingClientController.Instance.NetworkEnv;
			string valueOrDefault = UIGameInspectorScreenController.ShareUrlTemplates.GetValueOrDefault(networkEnv);
			if (valueOrDefault != null)
			{
				GUIUtility.systemCopyBuffer = string.Format(valueOrDefault, this.Model.MainMenuGameModel.AssetID);
				return;
			}
			DebugUtility.LogError(string.Format("Unsupported environment: {0}", networkEnv), this);
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0000D709 File Offset: 0x0000B909
		// Note: this type is marked as 'beforefieldinit'.
		static UIGameInspectorScreenController()
		{
			Dictionary<NetworkEnvironment, string> dictionary = new Dictionary<NetworkEnvironment, string>();
			dictionary[NetworkEnvironment.DEV] = "https://studio-dev.endlessstudios.com/studio/endstar/{0}?assetType=game";
			dictionary[NetworkEnvironment.STAGING] = "https://studio-staging.endlessstudios.com/studio/endstar/{0}?assetType=game";
			dictionary[NetworkEnvironment.PROD] = "https://studio.endlessstudios.com/studio/endstar/{0}?assetType=game";
			UIGameInspectorScreenController.ShareUrlTemplates = dictionary;
		}

		// Token: 0x040001AC RID: 428
		private static readonly IReadOnlyDictionary<NetworkEnvironment, string> ShareUrlTemplates;

		// Token: 0x040001AD RID: 429
		[Header("UIGameInspectorScreenController")]
		[SerializeField]
		private UIGameInspectorScreenView view;

		// Token: 0x040001AE RID: 430
		[SerializeField]
		private UIButton adminButton;

		// Token: 0x040001AF RID: 431
		[SerializeField]
		private UIButton editButton;

		// Token: 0x040001B0 RID: 432
		[SerializeField]
		private UIButton playButton;

		// Token: 0x040001B1 RID: 433
		[SerializeField]
		private UIButton copyShareLinkButton;

		// Token: 0x040001B2 RID: 434
		[SerializeField]
		private UILevelStateSelectionModalView levelStateSelectionModalSource;

		// Token: 0x040001B3 RID: 435
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
