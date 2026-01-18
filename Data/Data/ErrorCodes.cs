using System;
using Endless.Shared;

namespace Endless.Data
{
	// Token: 0x02000004 RID: 4
	public enum ErrorCodes
	{
		// Token: 0x0400000D RID: 13
		[UserFacingText("Error sending user report. Please try again.")]
		SendUserReport,
		// Token: 0x0400000E RID: 14
		[UserFacingText("Error retrieving asset versions. Please try again.")]
		UIGameAssetDetailView_RetrievingAssetVersions,
		// Token: 0x0400000F RID: 15
		[UserFacingText("Error retrieving the selected game. Please try again.")]
		ValidateGameState_GetGameAsset,
		// Token: 0x04000010 RID: 16
		[UserFacingText("Error retrieving assets by type. Please try again.")]
		RetrievingAssetsByType,
		// Token: 0x04000011 RID: 17
		[UserFacingText("Error converting platform id to user id.")]
		ClientDataPlatformIdToUserId,
		// Token: 0x04000012 RID: 18
		[UserFacingText("Error uploading file")]
		UploadFile,
		// Token: 0x04000013 RID: 19
		[UserFacingText("Error retrieving published asset versions. Please try again.")]
		UIGameAssetPublishModalView_GetPublishedVersionsOfAsset,
		// Token: 0x04000014 RID: 20
		[UserFacingText("Error updating asset. Please try again.")]
		UIGameAssetDetailController_UpdateAsset,
		// Token: 0x04000015 RID: 21
		[UserFacingText("Error retrieving script. Please try again.")]
		GetScript = 9,
		// Token: 0x04000016 RID: 22
		[UserFacingText("Error saving script. Please try again.")]
		UISaveScriptAndPropAndApplytoGameHander_SaveScript,
		// Token: 0x04000017 RID: 23
		[UserFacingText("Error saving prop with new script version. Please try saving again.")]
		UISaveScriptAndPropAndApplytoGameHander_SavePropAfterSavingScript,
		// Token: 0x04000018 RID: 24
		[UserFacingText("Error saving game with new prop version. Please try saving again.")]
		SaveGameAfterSavingProp,
		// Token: 0x04000019 RID: 25
		[UserFacingText("There was an error getting the version for this Asset. Please try again.")]
		UIGameAssetDetailController_NoVersionsOfAsset,
		// Token: 0x0400001A RID: 26
		[UserFacingText("There was an error getting the version for this Asset. Please try again.")]
		UISaveScriptAndPropAndApplyToGameHandler_GetVersions,
		// Token: 0x0400001B RID: 27
		[UserFacingText("There was an error cleaning up invalid data on the level. Please try loading the game again.")]
		CreatorManager_FailedBadDataCleanup,
		// Token: 0x0400001C RID: 28
		[UserFacingText("There was an error getting the version for this Asset. Please try again.")]
		UIVersionListModel_GetAssetForTimestamp,
		// Token: 0x0400001D RID: 29
		[UserFacingText("\nIt looks like your game is affected by a macOS security feature called App Translocation.\n\nTry moving the game to the <b>Applications</b> folder and open it by Control-clicking (or right-clicking) the app, then selecting <b>Open</b>.")]
		AppTranslocation,
		// Token: 0x0400001E RID: 30
		[UserFacingText("Error retrieving versions for level. Please try again.")]
		UIVersionListModel_RetrievingAssetVersions,
		// Token: 0x0400001F RID: 31
		[UserFacingText("Error parsing versions of the selected game. Please try again.")]
		UIGameAssetPublishModalView_ParsingVersions,
		// Token: 0x04000020 RID: 32
		[UserFacingText("Error retrieving versions of the selected game. Please try again.")]
		UIGameAssetPublishModalView_RetrievingAssetVersions,
		// Token: 0x04000021 RID: 33
		[UserFacingText("Error retrieving versions of the selected game. Please try again.")]
		UIPublishModel_NullGameVersions,
		// Token: 0x04000022 RID: 34
		[UserFacingText("Error retrieving versions of the selected game. Please try again.")]
		UIPublishModel_RetrievingAssetVersions,
		// Token: 0x04000023 RID: 35
		[UserFacingText("Error loading selected game data. Please try again.")]
		ValidateGameState_LoadingGame,
		// Token: 0x04000024 RID: 36
		[UserFacingText("Error updating game data. Please try again.")]
		GameEditor_UpdatingGame,
		// Token: 0x04000025 RID: 37
		[UserFacingText("Error getting asset. Please try again.")]
		UIBaseGameAssetVew_GetAsset,
		// Token: 0x04000026 RID: 38
		[UserFacingText("Error getting prop. Please try again.")]
		UIUserRolesModalModel_GetPropAsset,
		// Token: 0x04000027 RID: 39
		[UserFacingText("Error getting level data. Please try again.")]
		CreatorManager_GetLevelAssetInitialLoad,
		// Token: 0x04000028 RID: 40
		[UserFacingText("Error getting level data. Please try again.")]
		GameplayManager_GetLevelAsset,
		// Token: 0x04000029 RID: 41
		[UserFacingText("Error retrieving published asset versions. Please try again.")]
		UIPublishModel_GetPublishedVersionsOfAsset,
		// Token: 0x0400002A RID: 42
		[UserFacingText("There was an error getting the version for this Asset. Please try again.")]
		UIGameAssetDetailController_RetrievingAssetVersions,
		// Token: 0x0400002B RID: 43
		[UserFacingText("There was an error getting the versions for this prop. Please try again.")]
		UIScriptWindowModel_GetPropVersions,
		// Token: 0x0400002C RID: 44
		[UserFacingText("There was an error getting the versions for this props script. Please try again.")]
		UIScriptWindowModel_GetScriptVersions,
		// Token: 0x0400002D RID: 45
		[UserFacingText("Unable to reach the Endstar servers. Please check your internet connection and try again later. If this issue persists, please contact support.")]
		BuildUtilities_ForcedProductionBuildVersionGetRequestFailure,
		// Token: 0x0400002E RID: 46
		[UserFacingText("Unable to reach the Endstar servers. Please check your internet connection and try again later. If this issue persists, please contact support.")]
		BuildUtilities_TargetEnvironmentBuildVersionGetRequestFailure,
		// Token: 0x0400002F RID: 47
		[UserFacingText("Build version failed to parse. Please contact support.")]
		BuildUtilities_UnableToDetermineAvailableBuildVersions,
		// Token: 0x04000030 RID: 48
		[UserFacingText("There is a new version of Endstar available. Please launch the launcher and download the latest version.")]
		BuildUtilities_IncorrectVersionLaunched,
		// Token: 0x04000031 RID: 49
		[UserFacingText("Unable to determine the local clients version. Please launch the launcher and fix your Endstar installation.")]
		BuildUtilities_UnableToDetermineLocalBuildVersion,
		// Token: 0x04000032 RID: 50
		[UserFacingText("Error getting updated asset. Please try again.")]
		UIGameAssetDetailController_UpdateGameAsset,
		// Token: 0x04000033 RID: 51
		[UserFacingText("There was an error adding the Asset. Please try again.")]
		UIGameAssetDetailController_AddAsset,
		// Token: 0x04000034 RID: 52
		[UserFacingText("There was an error setting up the screen cover. A relaunch of the game may be necessary to have it function properly again.")]
		UIScreenCoverHandler_SubscribeSafely,
		// Token: 0x04000035 RID: 53
		[UserFacingText("There was an error setting up the screen cover. A relaunch of the game may be necessary to have it function properly again.")]
		UIScreenCoverHandler_Start_NullGameStateManager,
		// Token: 0x04000036 RID: 54
		[UserFacingText("There was an error setting up the screen cover. A relaunch of the game may be necessary to have it function properly again.")]
		UIScreenCoverHandler_OnMatchAllocated_NullGameStateManager,
		// Token: 0x04000037 RID: 55
		[UserFacingText("There was an error setting up the screen cover. A relaunch of the game may be necessary to have it function properly again.")]
		UIScreenCoverHandler_Start_NullStartMatchHelper,
		// Token: 0x04000038 RID: 56
		[UserFacingText("There was an error setting up the screen cover. A relaunch of the game may be necessary to have it function properly again.")]
		UIScreenCoverHandler_OnMatchAllocated_NullScreenCoverTokenHandler,
		// Token: 0x04000039 RID: 57
		[UserFacingText("There was an error getting your admin status. Please either restart the program or log out and log back in again to retry.")]
		UIMainMenuTabGroup_RequestAdminStatusAsync,
		// Token: 0x0400003A RID: 58
		[UserFacingText("There was an error getting your admin status. Please either restart the program or log out and log back in again to retry.")]
		UIAdminWindowDisplayHandler_RequestAdminStatusAsync,
		// Token: 0x0400003B RID: 59
		[UserFacingText("An error occured when starting the match. Please try again.")]
		UIStartMatchHelper_UserRequestedMatchStart = 1000,
		// Token: 0x0400003C RID: 60
		[UserFacingText("An unknown error occurred with your credentials. Please try again.")]
		UISignInScreenController_SetUserToken,
		// Token: 0x0400003D RID: 61
		[UserFacingText("An error occurred while authenticating with matchmaking. Do you have another copy of the game running? Please double check and try again.")]
		UISignInScreenController_Authentication,
		// Token: 0x0400003E RID: 62
		[UserFacingText("An unknown error occurred with validating your credentials. Please try again.")]
		UISignInScreenController_SignIn_Unknown_ErrorCode,
		// Token: 0x0400003F RID: 63
		[UserFacingText("Error registering new user. Please try again.")]
		NewUserRegistration,
		// Token: 0x04000040 RID: 64
		[UserFacingText("Error retrieving user identities. Please try again.")]
		RetrieveUserIdentities,
		// Token: 0x04000041 RID: 65
		[UserFacingText("Error creating matchmaking connection. Please try again.")]
		UIScreenCoverHandler_ConnectionToServerFailed,
		// Token: 0x04000042 RID: 66
		[UserFacingText("Error creating a Party. Please try again.")]
		UserGroupCreation,
		// Token: 0x04000043 RID: 67
		[UserFacingText("Error joining a Party. Please try again.")]
		UserGroupJoining,
		// Token: 0x04000044 RID: 68
		[UserFacingText("The match you were attempting to join was in an unknown game state and you have failed to join. Please try again.")]
		UnknownGameStateJoinError,
		// Token: 0x04000045 RID: 69
		[UserFacingText("Error ending the match. Please try again.")]
		UIMatchSectionController_EndMatch,
		// Token: 0x04000046 RID: 70
		[UserFacingText("Error subscribing to notifications for game. Please try starting another match.")]
		GameStateManager_SubscribeToObjectNotifications,
		// Token: 0x04000047 RID: 71
		[UserFacingText("Error unsubscribing to notifications for game. Please try starting another match.")]
		GameStateManager_UnsubscribeToObjectNotifications,
		// Token: 0x04000048 RID: 72
		[UserFacingText("Error migrating host for current game. If issues persist, please contact support.")]
		UIScreenCoverHandler_HostMigration,
		// Token: 0x04000049 RID: 73
		[UserFacingText("Invalid username or password, please correct the issue and try again")]
		UISignInScreenController_SignIn_InvalidUsernameOrPassword,
		// Token: 0x0400004A RID: 74
		[UserFacingText("Signing in requested timed-out. Our services may be down or overloaded, please try again later.")]
		UISignInScreenController_Timeout,
		// Token: 0x0400004B RID: 75
		[UserFacingText("Error subscribing to notifications. Please try starting another match.")]
		GameStateManager_SubscribeAllObjectNotificationsReconnect,
		// Token: 0x0400004C RID: 76
		[UserFacingText("Error subscribing to notifications for game. Please try starting another match.")]
		GameStateManager_SubscribeToObjectNotificationsReconnect,
		// Token: 0x0400004D RID: 77
		[UserFacingText("Error connecting to Unity Services. Please wait and try again. If the issue persists, please contact support.")]
		UIAuthenticationScreenHandler_OnServicesInitializedFailure,
		// Token: 0x0400004E RID: 78
		[UserFacingText("A password needs to be set from the Endless Platform before you can login to Endstar. Please go to <color=#00A3FF><size=80%><link=\"https://studio.endlessstudios.com/studio/profile/edit\">https://studio.endlessstudios.com/studio/profile/edit</link></color></size> and set a password on your account.")]
		UISignInScreenController_SignIn_PasswordNeedsSet,
		// Token: 0x0400004F RID: 79
		[UserFacingText("Error un-publishing the game. Please try again.")]
		UIGameAssetPublishModalController_UnpublishGame = 2000,
		// Token: 0x04000050 RID: 80
		[UserFacingText("Error publishing the game. Please try again.")]
		UIGameAssetPublishModalController_PublishGame,
		// Token: 0x04000051 RID: 81
		[UserFacingText("Error deleting the game. Please try again.")]
		UIGameInspectorScreenController_DeletingGame,
		// Token: 0x04000052 RID: 82
		[UserFacingText("Error creating the game. Please try again.")]
		UINewGameScreenController_CreateGame,
		// Token: 0x04000053 RID: 83
		[UserFacingText("Error with game, it has no levels.")]
		UINewGameScreenController_NoLevels,
		// Token: 0x04000054 RID: 84
		[UserFacingText("Error updating the game. Please try again.")]
		ProjectUpdate,
		// Token: 0x04000055 RID: 85
		[UserFacingText("Error resolving conflicts with an existing game save. Please try again.")]
		GameEditor_ResolvingGameSaveConflicts,
		// Token: 0x04000056 RID: 86
		[UserFacingText("There was an error retrieving games with a role for you. Please try again.")]
		UIMainMenuGameModelListModel_GetGamesWithRole,
		// Token: 0x04000057 RID: 87
		[UserFacingText("There was an error getting the published games. Please try again.")]
		UIMainMenuGameModelListModel_GetPublishedGames,
		// Token: 0x04000058 RID: 88
		[UserFacingText("There was an error getting the game. Please try again.")]
		UIAddScreenshotsToGameModalModel_GetGame,
		// Token: 0x04000059 RID: 89
		[UserFacingText("There was an error getting levels for the selected game. Please try again.")]
		LevelStateSelectionModalView_FetchLevelsFailure,
		// Token: 0x0400005A RID: 90
		[UserFacingText("Error un-publishing the asset. Please try again.")]
		UIPublishController_UnpublishAsset,
		// Token: 0x0400005B RID: 91
		[UserFacingText("Error publishing the asset. Please try again.")]
		UIPublishController_PublishAsset,
		// Token: 0x0400005C RID: 92
		[UserFacingText("Error loading the game. Please try again.")]
		UINewGameScreenController_LoadGame,
		// Token: 0x0400005D RID: 93
		[UserFacingText("Error updating the game. Please try again.")]
		ValidateGameState_UpdatingGame,
		// Token: 0x0400005E RID: 94
		[UserFacingText("Error updating the game. Please try again.")]
		ValidateGameState_LoadGame,
		// Token: 0x0400005F RID: 95
		[UserFacingText("Error updating the game's name. Please try again.")]
		UIGameInspectorScreenController_UpdatingGameName,
		// Token: 0x04000060 RID: 96
		[UserFacingText("Error updating the game's description. Please try again.")]
		UIGameInspectorScreenController_UpdatingGameDescription,
		// Token: 0x04000061 RID: 97
		[UserFacingText("Error updating the game's player count. Please try again.")]
		GameEditor_UpdatingGamePlayerCount,
		// Token: 0x04000062 RID: 98
		[UserFacingText("Error updating the game's name. Please try again.")]
		GameEditor_UpdatingGameName,
		// Token: 0x04000063 RID: 99
		[UserFacingText("Error updating the game's description. Please try again.")]
		GameEditor_UpdatingGameDescription,
		// Token: 0x04000064 RID: 100
		[UserFacingText("Error adding screenshots to the game. Please try again.")]
		GameEditor_AddingScreenshots,
		// Token: 0x04000065 RID: 101
		[UserFacingText("Error removing screenshots to the game. Please try again.")]
		GameEditor_RemovingScreenshots,
		// Token: 0x04000066 RID: 102
		[UserFacingText("Error reordering screenshots on the game. Please try again.")]
		GameEditor_ReorderingScreenshots,
		// Token: 0x04000067 RID: 103
		[UserFacingText("Error getting game data to resolve conflicts. Please try again.")]
		GameEditor_GetGameInitialResolveConflict,
		// Token: 0x04000068 RID: 104
		[UserFacingText("Error getting updated game data. Please try again.")]
		GameEditor_GetUpdatedGame = 2024,
		// Token: 0x04000069 RID: 105
		[UserFacingText("Error getting game data. Please try again.")]
		UIGameModelHandler_GetGame = 2024,
		// Token: 0x0400006A RID: 106
		[UserFacingText("Error deleting the level. Please try again.")]
		UIGameInspectorScreenController_DeletingLevel,
		// Token: 0x0400006B RID: 107
		[UserFacingText("Failed to create level from template. Please try again.")]
		UIGameEditorWindowController_LevelCreationFail,
		// Token: 0x0400006C RID: 108
		[UserFacingText("Failed to create level from template. Please try again.")]
		UINewGameScreenController_LevelCreationFail,
		// Token: 0x0400006D RID: 109
		[UserFacingText("There was an error updating all out of date assets in the game library. Please try again.")]
		GameEditor_UpdateAll,
		// Token: 0x0400006E RID: 110
		[UserFacingText("Error reordering levels on the game. Please try again.")]
		GameEditor_ReorderingLevels,
		// Token: 0x0400006F RID: 111
		UILevelAssetListModel_ApplyLevelOrderChange,
		// Token: 0x04000070 RID: 112
		[UserFacingText("Your Script and Prop were saved successfully, but a failure occured applying their new version to the active game. Please check the current version via the Game Library.")]
		UISaveScriptAndPropAndApplyToGameHandler_SaveScriptAndPropAndApplyToGame,
		// Token: 0x04000071 RID: 113
		[UserFacingText("Unknown error loading creator. Please try again, and report this if the error continues")]
		LoadingCreator_UnknownFailure,
		// Token: 0x04000072 RID: 114
		[UserFacingText("Unknown error loading into gameplay. Please try again, and report this if the error continues")]
		LoadingGameplay_UnknownFailure,
		// Token: 0x04000073 RID: 115
		[UserFacingText("Error getting updated game data. Please try again.")]
		GameEditor_GetUpdatedGame_GraphQLError,
		// Token: 0x04000074 RID: 116
		[UserFacingText("There was an error getting the user reported games. Please try again.")]
		UIMainMenuGameModelListModel_GetUserReportedGames,
		// Token: 0x04000075 RID: 117
		[UserFacingText("Error archiving the level. Please try again.")]
		UILevelAssetListModel_Archived = 3000,
		// Token: 0x04000076 RID: 118
		[UserFacingText("Error retrieving level revision. Please try again.")]
		UIRevisionListModel_RetrievingRevision,
		// Token: 0x04000077 RID: 119
		[UserFacingText("Error retrieving the level. Please try again.")]
		UILevelDestinationPropertyView_RetrievingLevelAsset,
		// Token: 0x04000078 RID: 120
		[UserFacingText("Error reverting the level. Please try again.")]
		LevelEditor_RevertLevel,
		// Token: 0x04000079 RID: 121
		[UserFacingText("Error with level state. It is invalid. Please try loading the level again.")]
		CreatorManager_InvalidLevelState,
		// Token: 0x0400007A RID: 122
		[UserFacingText("Error generating a patch of the active level for joining user. Please try again.")]
		LevelStatePatchGeneration,
		// Token: 0x0400007B RID: 123
		[UserFacingText("There was an error getting the levels from the game. Please try again.")]
		UIGameEditorWindowView_GetLevels,
		// Token: 0x0400007C RID: 124
		[UserFacingText("There was an error getting the level. Please try again.")]
		UILevelModelHandler_GetLevel,
		// Token: 0x0400007D RID: 125
		[UserFacingText("There was an error adding that level. Please try again.")]
		GameEditor_AddingLevel,
		// Token: 0x0400007E RID: 126
		[UserFacingText("Error updating the level. Please try again.")]
		UpdatingLevel,
		// Token: 0x0400007F RID: 127
		[UserFacingText("Retrieved malformed level data. Level may be corrupt. Please try again, and if the issue persists, contact support.")]
		LoadingGameplay_MalformedLevelCaught,
		// Token: 0x04000080 RID: 128
		[UserFacingText("The request to fetch the level has taken too long. Please try again.")]
		CreatorManager_InitialLevelFetchTimeout,
		// Token: 0x04000081 RID: 129
		[UserFacingText("The request to fetch the level has taken too long. Please try again.")]
		GameplayManager_LevelFetchTimeout,
		// Token: 0x04000082 RID: 130
		[UserFacingText("There was an error getting the level. Please try again.")]
		UIAddScreenshotsToLevelModalModel_GetLevel,
		// Token: 0x04000083 RID: 131
		[UserFacingText("There was an error getting the level. Please try again.")]
		UIAddScreenshotsToGameModalModel_GetLevel,
		// Token: 0x04000084 RID: 132
		[UserFacingText("There was an error getting the level. Please try again.")]
		UIScreenshotToolView_GetLevel,
		// Token: 0x04000085 RID: 133
		[UserFacingText("There was an error getting the level. Please try again.")]
		GetNextLevelName_GetLevel,
		// Token: 0x04000086 RID: 134
		[UserFacingText("Error updating collaborators. Please try again.")]
		CollaboratorUpdate = 4000,
		// Token: 0x04000087 RID: 135
		[UserFacingText("Error setting user role. Please try again.")]
		SetRoleOnAssetForUser,
		// Token: 0x04000088 RID: 136
		[UserFacingText("Error updating user role. Please try again.")]
		RoleUpdate,
		// Token: 0x04000089 RID: 137
		[UserFacingText("Error removing user from the asset. Please try again.")]
		DeleteUserFromAsset,
		// Token: 0x0400008A RID: 138
		[UserFacingText("Error refreshing user roles. Please try again.")]
		RefreshUserRoles,
		// Token: 0x0400008B RID: 139
		[UserFacingText("Error updating local user role. Please try again.")]
		LocalClientRoleChange,
		// Token: 0x0400008C RID: 140
		[UserFacingText("Error retrieving users with roles for asset.")]
		RetrievingUsersWithRolesForAsset,
		// Token: 0x0400008D RID: 141
		[UserFacingText("Error retrieving assets by type available to user.")]
		RetrievingAssetsByTypeAndRole,
		// Token: 0x0400008E RID: 142
		[UserFacingText("Error retrieving asset by publish type.")]
		RetrievingAssetByPublishType,
		// Token: 0x0400008F RID: 143
		[UserFacingText("Error retrieving users with roles for asset.")]
		UIUserRolesModalModel_RetrievingUsersWithRolesForAsset,
		// Token: 0x04000090 RID: 144
		[UserFacingText("Error verifying all users in party have roles for game.")]
		UIStartMatchHelper_RetrievingUsersWithRolesForAsset,
		// Token: 0x04000091 RID: 145
		[UserFacingText("Error getting Game Assets. Please try again.")]
		GettingGameLibraryAssets = 5000,
		// Token: 0x04000092 RID: 146
		[UserFacingText("Error un-publishing the Game Asset. Please try again.")]
		UnpublishGameLibraryAsset,
		// Token: 0x04000093 RID: 147
		[UserFacingText("Error publishing the Game Asset. Please try again.")]
		PublishGameLibraryAsset,
		// Token: 0x04000094 RID: 148
		[UserFacingText("Error removing terrain. Please try again.")]
		RemoveTerrainEntry,
		// Token: 0x04000095 RID: 149
		[UserFacingText("Error publishing the asset. Please try again.")]
		PublishAsset,
		// Token: 0x04000096 RID: 150
		[UserFacingText("Error un-publishing the asset. Please try again.")]
		UnpublishAsset,
		// Token: 0x04000097 RID: 151
		[UserFacingText("Game Library Terrain Usages are invalid. Please Try Again.")]
		InvalidTerrainUsageState,
		// Token: 0x04000098 RID: 152
		[UserFacingText("Error getting owned Game Assets. Please try again.")]
		UIOwnedGameAssetCloudPaginatedListModel_GettingOwnedGameAssets,
		// Token: 0x04000099 RID: 153
		[UserFacingText("Error getting published Game Assets. Please try again.")]
		UIPublishedGameAssetCloudPaginatedListModel_GettingPublishedGameAssets,
		// Token: 0x0400009A RID: 154
		[UserFacingText("The request to fetch the game has taken too long. Please try again.")]
		ValidateGameLibrary_GameFetchTimeout,
		// Token: 0x0400009B RID: 155
		[UserFacingText("Your client has failed to download the level data. Please try again.")]
		CreatorManager_ErrorGettingLevel,
		// Token: 0x0400009C RID: 156
		[UserFacingText("Your client has timed out while downloading the level data. Please try again.")]
		CreatorManager_ErrorGettingLevelTimeout,
		// Token: 0x0400009D RID: 157
		[UserFacingText("An error occured changing the version of that asset. Please try again.")]
		GameLibrary_VersionChangeFailed,
		// Token: 0x0400009E RID: 158
		[UserFacingText("Failed to create or add the abstract prop to your library.")]
		AbstractPropCreationFailure,
		// Token: 0x0400009F RID: 159
		[UserFacingText("An error occured changing the moderation flags of that asset. Please try again.")]
		UIAssetModerationModalModel_ApplyModerationFlagsAsync,
		// Token: 0x040000A0 RID: 160
		[UserFacingText("An error occured processing the moderation flags of that asset. Please try again.")]
		UIAssetModerationModalModel_ProcessFlagOperationsAsync,
		// Token: 0x040000A1 RID: 161
		[UserFacingText("An error occured retrieving the moderation flags of that asset. Please try again.")]
		UIAssetModerationModalModel_RequestAndViewExistingAssetModerationFlags,
		// Token: 0x040000A2 RID: 162
		[UserFacingText("An error occured retrieving the moderation flags. Please restart the session.")]
		UIAdminWindowDisplayHandler_GetModerationFlagsAsync,
		// Token: 0x040000A3 RID: 163
		[UserFacingText("There was an issue displaying that wire. Please try again.")]
		UIWirePropertyModifierView_DisplayExistingWire = 6000,
		// Token: 0x040000A4 RID: 164
		[UserFacingText("There was an issue loading an asset bundle from disk. Please try again.")]
		LoadingAssetBundle = 7000,
		// Token: 0x040000A5 RID: 165
		[UserFacingText("There was an error loading into the correct game state. Please exit the game and try again.")]
		ValidateGameLibraryGameState_StateChangeFinalizeLoop = 8000,
		// Token: 0x040000A6 RID: 166
		[UserFacingText("There was an error joining the match. Please exit the game and try again.")]
		ValidateGameLibraryGameState_IncorrectServerFollowupState,
		// Token: 0x040000A7 RID: 167
		[UserFacingText("There was an error getting the versions of the Prop you are trying to re-add to your Game Library. The prop may no longer exist.")]
		UIInspectorToolPanelController_GetVersionsToReAddToGameLibrary = 9000,
		// Token: 0x040000A8 RID: 168
		[UserFacingText("There was an error parsing the versions of the Prop you are trying to re-add to your Game Library. The prop may no longer exist.")]
		UIInspectorToolPanelController_NoVersions,
		// Token: 0x040000A9 RID: 169
		[UserFacingText("There was an error authenticating on the matchmaking server. Please try again.")]
		UIScreenCoverHandler_AuthenticationProcessFailed = 10000,
		// Token: 0x040000AA RID: 170
		[UserFacingText("There was an error allocating the match. Please try again.")]
		UIScreenCoverHandler_MatchAllocationError,
		// Token: 0x040000AB RID: 171
		[UserFacingText("There was an error activating the selected deep link.")]
		UnknownDeepLinkActivationError = 11000,
		// Token: 0x040000AC RID: 172
		[UserFacingText("There was an error activating the selected deep link.")]
		InspectPublishedAssetDeepLinkExecutionError_GetPublishedVersionsFailure,
		// Token: 0x040000AD RID: 173
		[UserFacingText("There was an error activating the selected deep link.")]
		InspectPublishedAssetDeepLinkExecutionError_GetTargetVersionFailure,
		// Token: 0x040000AE RID: 174
		[UserFacingText("There was an error activating the selected deep link.")]
		InspectPublishedAssetDeepLinkExecutionError_GetTargetAssetDataFailure,
		// Token: 0x040000AF RID: 175
		[UserFacingText("This game contains restricted content that is not allowed.")]
		LoadingIntoGame_ContentRestricted = 12000,
		// Token: 0x040000B0 RID: 176
		[UserFacingText("This game contains restricted content that is not allowed.")]
		RepopulatingGame_PropLibrary_ContentRestricted,
		// Token: 0x040000B1 RID: 177
		[UserFacingText("This game contains restricted content that is not allowed.")]
		RepopulatingGame_RuntimePalette_ContentRestricted,
		// Token: 0x040000B2 RID: 178
		[UserFacingText("This game contains restricted content that is not allowed.")]
		DeepLinkingGame_ContentRestricted,
		// Token: 0x040000B3 RID: 179
		[UserFacingText("Unable to verify content flags on the game.")]
		ValidateGameLibraryGameState_UnableToFetchContentFlags = 12050,
		// Token: 0x040000B4 RID: 180
		[UserFacingText("This game contains instances of the Rifleman NPC, which is restricted content on this account.")]
		SpawnNpc_ContentRestricted_Rifleman = 12100,
		// Token: 0x040000B5 RID: 181
		[UserFacingText("This game contains instances of the Grunt NPC, which is restricted content on this account.")]
		SpawnNpc_ContentRestricted_Grunt,
		// Token: 0x040000B6 RID: 182
		[UserFacingText("This game contains instances of the Zombie NPC, which is restricted content on this account.")]
		SpawnNpc_ContentRestricted_Zombie,
		// Token: 0x040000B7 RID: 183
		[UserFacingText("This game contains instances of the Rifleman NPC, which is restricted content on this account.")]
		SpawnNpcAtPosition_ContentRestricted_Rifleman,
		// Token: 0x040000B8 RID: 184
		[UserFacingText("This game contains instances of the Grunt NPC, which is restricted content on this account.")]
		SpawnNpcAtPosition_ContentRestricted_Grunt,
		// Token: 0x040000B9 RID: 185
		[UserFacingText("This game contains instances of the Zombie NPC, which is restricted content on this account.")]
		SpawnNpcAtPosition_ContentRestricted_Zombie,
		// Token: 0x040000BA RID: 186
		[UserFacingText("This game contains instances of the Rifleman NPC, which is restricted content on this account.")]
		NpcEntity_NpcClasSet_ContentRestricted_Rifleman,
		// Token: 0x040000BB RID: 187
		[UserFacingText("This game contains instances of the Grunt NPC, which is restricted content on this account.")]
		NpcEntity_NpcClasSet_ContentRestricted_Grunt,
		// Token: 0x040000BC RID: 188
		[UserFacingText("This game contains instances of the Zombie NPC, which is restricted content on this account.")]
		NpcEntity_NpcClasSet_ContentRestricted_Zombie,
		// Token: 0x040000BD RID: 189
		[UserFacingText("This game contains instances of the Rifleman NPC, which is restricted content on this account.")]
		NpcEntity_EndlessAwake_ContentRestricted_Rifleman,
		// Token: 0x040000BE RID: 190
		[UserFacingText("This game contains instances of the Grunt NPC, which is restricted content on this account.")]
		NpcEntity_EndlessAwake_ContentRestricted_Grunt,
		// Token: 0x040000BF RID: 191
		[UserFacingText("This game contains instances of the Zombie NPC, which is restricted content on this account.")]
		NpcEntity_EndlessAwake_ContentRestricted_Zombie,
		// Token: 0x040000C0 RID: 192
		[UserFacingText("Your client has timed out while retrieving content flags for content in this game. Please try again.")]
		ValidateGameLibraryGameState_UnableToFetchContentFlags_Timeout,
		// Token: 0x040000C1 RID: 193
		[UserFacingText("There was an error getting your list of friends.")]
		UIFriendshipList_RequestListTask = 13000,
		// Token: 0x040000C2 RID: 194
		[UserFacingText("There was an error getting your friend requests.")]
		UIFriendRequestList_RequestListTask,
		// Token: 0x040000C3 RID: 195
		[UserFacingText("There was an error getting your sent friend requests.")]
		UISentFriendRequestList_RequestListTask,
		// Token: 0x040000C4 RID: 196
		[UserFacingText("There was an error getting your blocked users.")]
		UIBlockedUserList_RequestListTask
	}
}
