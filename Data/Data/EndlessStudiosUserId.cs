using System;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Data
{
	// Token: 0x02000003 RID: 3
	[CreateAssetMenu(menuName = "ScriptableObject/UserIds/Endless Studios User Id")]
	public sealed class EndlessStudiosUserId : ScriptableObject
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000020C0 File Offset: 0x000002C0
		public int InternalId
		{
			get
			{
				EndlessStudiosUserId.EnsureReady();
				NetworkEnvironment currentEnvironment = EndlessStudiosUserId.GetCurrentEnvironment();
				return this.GetInternalId(currentEnvironment);
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000004 RID: 4 RVA: 0x000020E0 File Offset: 0x000002E0
		public string PublicId
		{
			get
			{
				EndlessStudiosUserId.EnsureReady();
				NetworkEnvironment currentEnvironment = EndlessStudiosUserId.GetCurrentEnvironment();
				return this.GetPublicId(currentEnvironment);
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002100 File Offset: 0x00000300
		public User User
		{
			get
			{
				EndlessStudiosUserId.EnsureReady();
				NetworkEnvironment currentEnvironment = EndlessStudiosUserId.GetCurrentEnvironment();
				int internalId = this.GetInternalId(currentEnvironment);
				string publicId = this.GetPublicId(currentEnvironment);
				if (this.cachedUser != null)
				{
					NetworkEnvironment? networkEnvironment = this.cachedEnv;
					NetworkEnvironment networkEnvironment2 = currentEnvironment;
					if (((networkEnvironment.GetValueOrDefault() == networkEnvironment2) & (networkEnvironment != null)) && this.cachedId == internalId && this.cachedPublicId == publicId)
					{
						return this.cachedUser;
					}
				}
				this.cachedUser = new User(internalId, publicId, string.Empty);
				this.cachedEnv = new NetworkEnvironment?(currentEnvironment);
				this.cachedId = internalId;
				this.cachedPublicId = publicId;
				return this.cachedUser;
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000219F File Offset: 0x0000039F
		private static void EnsureReady()
		{
			if (!Application.isPlaying)
			{
				throw new InvalidOperationException("EndlessStudiosUserId can only be accessed in play mode.");
			}
			if (!MatchmakingClientController.Instance)
			{
				throw new InvalidOperationException("EndlessStudiosUserId does not have access to MatchmakingClientController.");
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000021CA File Offset: 0x000003CA
		private static NetworkEnvironment GetCurrentEnvironment()
		{
			return MatchmakingClientController.Instance.NetworkEnvironment;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000021D8 File Offset: 0x000003D8
		private int GetInternalId(NetworkEnvironment environment)
		{
			switch (environment)
			{
			case NetworkEnvironment.DEV:
				return this.internalIdDev;
			case NetworkEnvironment.STAGING:
				return this.internalIdStaging;
			case NetworkEnvironment.PROD:
				return this.internalIdProd;
			default:
				DebugUtility.LogException(new Exception(string.Format("{0} has no support for {1} value '{2}'. Falling back to {3}.", new object[]
				{
					"EndlessStudiosUserId",
					"NetworkEnvironment",
					environment,
					NetworkEnvironment.PROD
				})), this);
				return this.GetInternalId(NetworkEnvironment.PROD);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002254 File Offset: 0x00000454
		private string GetPublicId(NetworkEnvironment environment)
		{
			switch (environment)
			{
			case NetworkEnvironment.DEV:
				return this.publicIdDev;
			case NetworkEnvironment.STAGING:
				return this.publicIdStaging;
			case NetworkEnvironment.PROD:
				return this.publicIdProd;
			default:
				DebugUtility.LogException(new Exception(string.Format("{0} has no support for {1} value '{2}'. Falling back to {3}.", new object[]
				{
					"EndlessStudiosUserId",
					"NetworkEnvironment",
					environment,
					NetworkEnvironment.PROD
				})), this);
				return this.GetPublicId(NetworkEnvironment.PROD);
			}
		}

		// Token: 0x04000001 RID: 1
		private const NetworkEnvironment UNSUPPORTED_ENVIRONMENT_FALLBACK = NetworkEnvironment.PROD;

		// Token: 0x04000002 RID: 2
		[Header("Internal Ids")]
		[SerializeField]
		private int internalIdDev;

		// Token: 0x04000003 RID: 3
		[SerializeField]
		private int internalIdStaging;

		// Token: 0x04000004 RID: 4
		[SerializeField]
		private int internalIdProd;

		// Token: 0x04000005 RID: 5
		[Header("Public Ids")]
		[SerializeField]
		private string publicIdDev = string.Empty;

		// Token: 0x04000006 RID: 6
		[SerializeField]
		private string publicIdStaging = string.Empty;

		// Token: 0x04000007 RID: 7
		[SerializeField]
		private string publicIdProd = string.Empty;

		// Token: 0x04000008 RID: 8
		[NonSerialized]
		private User cachedUser;

		// Token: 0x04000009 RID: 9
		[NonSerialized]
		private NetworkEnvironment? cachedEnv;

		// Token: 0x0400000A RID: 10
		[NonSerialized]
		private int cachedId;

		// Token: 0x0400000B RID: 11
		[NonSerialized]
		private string cachedPublicId;
	}
}
