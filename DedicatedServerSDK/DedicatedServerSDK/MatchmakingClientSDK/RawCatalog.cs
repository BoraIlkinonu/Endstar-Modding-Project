using System;
using System.Runtime.CompilerServices;

namespace MatchmakingClientSDK
{
	// Token: 0x02000007 RID: 7
	[NullableContext(1)]
	[Nullable(0)]
	public static class RawCatalog
	{
		// Token: 0x04000039 RID: 57
		public const string serviceLabel = "action";

		// Token: 0x0400003A RID: 58
		public const string methodLabel = "method";

		// Token: 0x0400003B RID: 59
		public const string requestIdLabel = "requestId";

		// Token: 0x0400003C RID: 60
		public const string dataLabel = "data";

		// Token: 0x0400003D RID: 61
		public const string oldLabel = "old";

		// Token: 0x0400003E RID: 62
		public const string newLabel = "new";

		// Token: 0x0400003F RID: 63
		public const string addedLabel = "added";

		// Token: 0x04000040 RID: 64
		public const string changedLabel = "changed";

		// Token: 0x04000041 RID: 65
		public const string removedLabel = "removed";

		// Token: 0x04000042 RID: 66
		public const string userNameLabel = "userName";

		// Token: 0x04000043 RID: 67
		public const string userIdLabel = "userId";

		// Token: 0x04000044 RID: 68
		public const string groupIdLabel = "groupId";

		// Token: 0x04000045 RID: 69
		public const string eventsLabel = "events";

		// Token: 0x04000046 RID: 70
		public const string notificationLabel = "notification";

		// Token: 0x04000047 RID: 71
		public const string chatLabel = "chat";

		// Token: 0x04000048 RID: 72
		public const string chatChannelLabel = "channel";

		// Token: 0x04000049 RID: 73
		public const string userChatChannelLabel = "user";

		// Token: 0x0400004A RID: 74
		public const string groupChatChannelLabel = "group";

		// Token: 0x0400004B RID: 75
		public const string matchChatChannelLabel = "match";

		// Token: 0x0400004C RID: 76
		public const string chatMessageLabel = "message";

		// Token: 0x0400004D RID: 77
		public const string groupsLabel = "groups";

		// Token: 0x0400004E RID: 78
		public const string inviteLabel = "invite";

		// Token: 0x0400004F RID: 79
		public const string inviteIdLabel = "inviteId";

		// Token: 0x04000050 RID: 80
		public const string usersLabel = "users";

		// Token: 0x04000051 RID: 81
		public const string syncLabel = "sync";

		// Token: 0x04000052 RID: 82
		public const string heartbeatLabel = "heartbeat";

		// Token: 0x04000053 RID: 83
		public const string matchesLabel = "matches";

		// Token: 0x04000054 RID: 84
		public const string startLabel = "start";

		// Token: 0x04000055 RID: 85
		public const string joinLabel = "join";

		// Token: 0x04000056 RID: 86
		public const string changeLabel = "change";

		// Token: 0x04000057 RID: 87
		public const string leaveLabel = "leave";

		// Token: 0x04000058 RID: 88
		public const string changeMatchUsersLabel = "changeMatchUsers";

		// Token: 0x04000059 RID: 89
		public const string changeHostLabel = "changeHost";

		// Token: 0x0400005A RID: 90
		public const string codeLabel = "code";

		// Token: 0x0400005B RID: 91
		public const string errorLabel = "error";

		// Token: 0x0400005C RID: 92
		public const string errorMessageLabel = "errorMessage";

		// Token: 0x0400005D RID: 93
		public const string gameIdLabel = "gameId";

		// Token: 0x0400005E RID: 94
		public const string levelIdLabel = "levelId";

		// Token: 0x0400005F RID: 95
		public const string versionLabel = "version";

		// Token: 0x04000060 RID: 96
		public const string isEditSessionLabel = "isEditSession";

		// Token: 0x04000061 RID: 97
		public const string serverTypeLabel = "serverType";

		// Token: 0x04000062 RID: 98
		public const string userServerTypeLabel = "USER";

		// Token: 0x04000063 RID: 99
		public const string dedicatedServerTypeLabel = "DEDICATED";

		// Token: 0x04000064 RID: 100
		public const string matchIdLabel = "matchId";

		// Token: 0x04000065 RID: 101
		public const string allocationDataLabel = "allocationData";

		// Token: 0x04000066 RID: 102
		public const string allocateLabel = "allocate";

		// Token: 0x04000067 RID: 103
		public const string instanceIpLabel = "publicIp";

		// Token: 0x04000068 RID: 104
		public const string instanceLocalIpLabel = "localIp";

		// Token: 0x04000069 RID: 105
		public const string instanceNameLabel = "name";

		// Token: 0x0400006A RID: 106
		public const string instancePortLabel = "port";

		// Token: 0x0400006B RID: 107
		public const string instanceAuthKeyLabel = "key";

		// Token: 0x0400006C RID: 108
		public const string newHostGroupIdLabel = "newHostGroupId";

		// Token: 0x0400006D RID: 109
		public const string eventNameLabel = "eventName";

		// Token: 0x0400006E RID: 110
		public const string sequenceNumLabel = "sequenceNum";

		// Token: 0x0400006F RID: 111
		public const string userConnectedLabel = "userConnected";

		// Token: 0x04000070 RID: 112
		public const string userDisconnectedLabel = "userDisconnected";

		// Token: 0x04000071 RID: 113
		public const string userUpdatedLabel = "userUpdated";

		// Token: 0x04000072 RID: 114
		public const string groupCreatedLabel = "groupCreated";

		// Token: 0x04000073 RID: 115
		public const string groupUpdatedLabel = "groupUpdated";

		// Token: 0x04000074 RID: 116
		public const string groupDeletedLabel = "groupDeleted";

		// Token: 0x04000075 RID: 117
		public const string matchCreatedLabel = "matchCreated";

		// Token: 0x04000076 RID: 118
		public const string matchUpdatedLabel = "matchUpdated";

		// Token: 0x04000077 RID: 119
		public const string matchDeletedLabel = "matchDeleted";

		// Token: 0x04000078 RID: 120
		public const string groupMembersLabel = "groupMembers";

		// Token: 0x04000079 RID: 121
		public const string groupHostLabel = "groupHost";

		// Token: 0x0400007A RID: 122
		public const string matchHostLabel = "matchHost";

		// Token: 0x0400007B RID: 123
		public const string matchMembersLabel = "matchMembers";

		// Token: 0x0400007C RID: 124
		public const string notificationTypeLabel = "notificationType";

		// Token: 0x0400007D RID: 125
		public const string publicKeyField = "publicKey";

		// Token: 0x0400007E RID: 126
		public const string encryptedKeyField = "encryptedKey";
	}
}
