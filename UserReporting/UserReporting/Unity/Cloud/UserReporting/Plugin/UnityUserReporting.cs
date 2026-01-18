using System;
using Unity.Cloud.UserReporting.Client;
using UnityEngine;

namespace Unity.Cloud.UserReporting.Plugin
{
	// Token: 0x02000023 RID: 35
	public static class UnityUserReporting
	{
		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000EF RID: 239 RVA: 0x000048B7 File Offset: 0x00002AB7
		// (set) Token: 0x060000F0 RID: 240 RVA: 0x000048CA File Offset: 0x00002ACA
		public static UserReportingClient CurrentClient
		{
			get
			{
				if (UnityUserReporting.currentClient == null)
				{
					UnityUserReporting.Configure();
				}
				return UnityUserReporting.currentClient;
			}
			private set
			{
				UnityUserReporting.currentClient = value;
			}
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000048D2 File Offset: 0x00002AD2
		public static void Configure(string endpoint, string projectIdentifier, IUserReportingPlatform platform, UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.CurrentClient = new UserReportingClient(endpoint, projectIdentifier, platform, configuration);
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x000048E2 File Offset: 0x00002AE2
		public static void Configure(string endpoint, string projectIdentifier, UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.CurrentClient = new UserReportingClient(endpoint, projectIdentifier, UnityUserReporting.GetPlatform(), configuration);
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x000048F6 File Offset: 0x00002AF6
		public static void Configure(string projectIdentifier, UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", projectIdentifier, UnityUserReporting.GetPlatform(), configuration);
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00004909 File Offset: 0x00002B09
		public static void Configure(string projectIdentifier)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", projectIdentifier, UnityUserReporting.GetPlatform(), new UserReportingClientConfiguration());
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00004920 File Offset: 0x00002B20
		public static void Configure()
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", Application.cloudProjectId, UnityUserReporting.GetPlatform(), new UserReportingClientConfiguration());
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x0000493B File Offset: 0x00002B3B
		public static void Configure(UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", Application.cloudProjectId, UnityUserReporting.GetPlatform(), configuration);
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00004952 File Offset: 0x00002B52
		public static void Configure(string projectIdentifier, IUserReportingPlatform platform, UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", projectIdentifier, platform, configuration);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00004961 File Offset: 0x00002B61
		public static void Configure(IUserReportingPlatform platform, UserReportingClientConfiguration configuration)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", Application.cloudProjectId, platform, configuration);
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00004974 File Offset: 0x00002B74
		public static void Configure(IUserReportingPlatform platform)
		{
			UnityUserReporting.Configure("https://userreporting.cloud.unity3d.com", Application.cloudProjectId, platform, new UserReportingClientConfiguration());
		}

		// Token: 0x060000FA RID: 250 RVA: 0x0000498B File Offset: 0x00002B8B
		private static IUserReportingPlatform GetPlatform()
		{
			return new UnityUserReportingPlatform();
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00004992 File Offset: 0x00002B92
		public static void Use(UserReportingClient client)
		{
			if (client != null)
			{
				UnityUserReporting.CurrentClient = client;
			}
		}

		// Token: 0x04000082 RID: 130
		private static UserReportingClient currentClient;
	}
}
