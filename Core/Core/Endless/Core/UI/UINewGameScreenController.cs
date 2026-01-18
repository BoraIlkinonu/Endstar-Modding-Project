using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Creator;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.UI;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000080 RID: 128
	[RequireComponent(typeof(UINewGameScreenView))]
	public class UINewGameScreenController : UIBaseGameScreenController, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x0600028B RID: 651 RVA: 0x0000E10A File Offset: 0x0000C30A
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600028C RID: 652 RVA: 0x0000E112 File Offset: 0x0000C312
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600028D RID: 653 RVA: 0x0000E11A File Offset: 0x0000C31A
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Combine(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(this.CreateNewGame));
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000E154 File Offset: 0x0000C354
		protected override void Start()
		{
			base.Start();
			this.createButton.onClick.AddListener(new UnityAction(this.DisplayNewLevelStateModal));
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0000E178 File Offset: 0x0000C378
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UINewLevelStateModalController.CreateLevel = (Action<string, string, LevelStateTemplateSourceBase>)Delegate.Remove(UINewLevelStateModalController.CreateLevel, new Action<string, string, LevelStateTemplateSourceBase>(this.CreateNewGame));
			this.gameToEdit = null;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0000E1C4 File Offset: 0x0000C3C4
		private void DisplayNewLevelStateModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayNewLevelStateModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.newLevelStateModalSource, UIModalManagerStackActions.ClearStack, new object[] { UINewLevelStateModalView.Contexts.NewGame });
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0000E200 File Offset: 0x0000C400
		private async void CreateNewGame(string levelName, string levelDescription, LevelStateTemplateSourceBase selectedLevelStateTemplate)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateNewGame", new object[] { levelName, levelDescription, selectedLevelStateTemplate.name });
			}
			if (!this.GameNameText.text.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				this.OnLoadingStarted.Invoke();
				bool flag = this.localGroupMemberRoleValidator.ValidateAllLocalGroupMembersHaveRole(this.userRolesModel.UserRoles);
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "allLocalGroupMembersHaveRoles", flag), this);
				}
				if (!flag)
				{
					this.OnLoadingEnded.Invoke();
				}
				else
				{
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
					Game game = new Game
					{
						Name = this.GameNameText.text,
						Description = this.DescriptionInputField.text
					};
					await MonoBehaviourSingleton<DefaultContentManager>.Instance.AddAllDefaults(game);
					LevelState levelState;
					try
					{
						levelState = await selectedLevelStateTemplate.CreateLevelState(game, levelName, levelDescription, false);
					}
					catch (Exception ex)
					{
						this.OnLoadingEnded.Invoke();
						ErrorHandler.HandleError(ErrorCodes.UINewGameScreenController_LevelCreationFail, new Exception("Failed creating level from template " + selectedLevelStateTemplate.name, ex), true, false);
						return;
					}
					object anonymousObjectForUpload = game.GetAnonymousObjectForUpload(levelState);
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.CreateAssetAsync(anonymousObjectForUpload, base.VerboseLogging, 60);
					if (graphQlResult.HasErrors)
					{
						this.OnLoadingEnded.Invoke();
						ErrorHandler.HandleError(ErrorCodes.UINewGameScreenController_CreateGame, graphQlResult.GetErrorMessage(0), true, false);
					}
					else
					{
						this.gameToEdit = GameLoader.Load(graphQlResult.GetDataMember().ToString());
						foreach (UserRole userRole in this.userRolesModel.UserRoles)
						{
							if (userRole.UserId != EndlessServices.Instance.CloudService.ActiveUserId)
							{
								await MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(this.gameToEdit.AssetID, userRole.UserId, userRole.Role);
							}
						}
						IEnumerator<UserRole> enumerator = null;
						if (this.gameToEdit == null)
						{
							this.OnLoadingEnded.Invoke();
							UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
							ErrorHandler.HandleError(ErrorCodes.UINewGameScreenController_LoadGame, new Exception("Expected a game as result from CreateAsset call, but got " + graphQlResult.RawResult), true, false);
						}
						else if (this.gameToEdit.levels.Count == 0)
						{
							this.OnLoadingEnded.Invoke();
							UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
							ErrorHandler.HandleError(ErrorCodes.UINewGameScreenController_NoLevels, new Exception("Newly created game has no levels! This should be impossible."), true, false);
						}
						else
						{
							this.OnLoadingEnded.Invoke();
							MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(this.gameToEdit.AssetID, null, this.gameToEdit.levels[0].AssetID, MainMenuGameContext.Edit);
						}
					}
				}
			}
		}

		// Token: 0x040001DB RID: 475
		[Header("UINewGameScreenController")]
		[SerializeField]
		private UIUserRolesModel userRolesModel;

		// Token: 0x040001DC RID: 476
		[SerializeField]
		private UIButton createButton;

		// Token: 0x040001DD RID: 477
		[SerializeField]
		private UINewLevelStateModalView newLevelStateModalSource;

		// Token: 0x040001DE RID: 478
		[SerializeField]
		private LocalGroupMemberRoleValidator localGroupMemberRoleValidator;

		// Token: 0x040001DF RID: 479
		private Game gameToEdit;
	}
}
