using System;
using Endless.Shared;

namespace Endless.Data.UI
{
	// Token: 0x0200000A RID: 10
	public enum UIScreenCoverTokens
	{
		// Token: 0x0400011B RID: 283
		[UserFacingText("Waiting for Endless Matchmaking service to initialize...")]
		MatchmakingClientControllerInitialization,
		// Token: 0x0400011C RID: 284
		[UserFacingText("Logging into the Endless Cloud service...")]
		WaitingForEndlessCloudServiceSignIn,
		// Token: 0x0400011D RID: 285
		[UserFacingText("Logging into the Endless Cloud service...")]
		WaitingForEndlessCloudServiceVerifyToken,
		// Token: 0x0400011E RID: 286
		[UserFacingText("Logging into the Endless Cloud service...")]
		WaitingForEndlessCloudServiceVerifyPlayerPrefsToken,
		// Token: 0x0400011F RID: 287
		[UserFacingText("Connecting to the Endless Matchmaking service...")]
		WaitingForMatchmakingConnection,
		// Token: 0x04000120 RID: 288
		[UserFacingText("Logging into the Endless Matchmaking service...")]
		WaitingForMatchmakingAuthentication,
		// Token: 0x04000121 RID: 289
		[UserFacingText("Waiting for Match...")]
		WaitingForMatchStarted,
		// Token: 0x04000122 RID: 290
		[UserFacingText("Waiting for Match...")]
		WaitingForMatchAllocation,
		// Token: 0x04000123 RID: 291
		[UserFacingText("Waiting for Game Mode..")]
		WaitingForGameStateOtherThanNoneOrDefault,
		// Token: 0x04000124 RID: 292
		[UserFacingText("Validating Game Library..")]
		ValidatingGameLibrary,
		// Token: 0x04000125 RID: 293
		[UserFacingText("Loading into Creator Mode...")]
		LoadingCreator,
		// Token: 0x04000126 RID: 294
		[UserFacingText("Loading into Gameplay Mode...")]
		LoadingGameplay,
		// Token: 0x04000127 RID: 295
		[UserFacingText("Waiting for other players to join...")]
		LoadedGameplayGameState,
		// Token: 0x04000128 RID: 296
		[UserFacingText("Migrating the host...")]
		MatchHostMigration,
		// Token: 0x04000129 RID: 297
		[UserFacingText("Ending Match...")]
		EndingMatch,
		// Token: 0x0400012A RID: 298
		[UserFacingText("Waiting for network to start gameplay...")]
		WaitingForNetworkToStartGameplay,
		// Token: 0x0400012B RID: 299
		[UserFacingText("Waiting for network to start creator mode...")]
		WaitingForNetworkToStartCreator
	}
}
