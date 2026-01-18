using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Data;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200026C RID: 620
	public class UIPublishModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06000A3D RID: 2621 RVA: 0x0002FEB8 File Offset: 0x0002E0B8
		// (remove) Token: 0x06000A3E RID: 2622 RVA: 0x0002FEF0 File Offset: 0x0002E0F0
		public event Action OnSynchronizeStart;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06000A3F RID: 2623 RVA: 0x0002FF28 File Offset: 0x0002E128
		// (remove) Token: 0x06000A40 RID: 2624 RVA: 0x0002FF60 File Offset: 0x0002E160
		public event Action<List<string>> OnVersionsSet;

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06000A41 RID: 2625 RVA: 0x0002FF98 File Offset: 0x0002E198
		// (remove) Token: 0x06000A42 RID: 2626 RVA: 0x0002FFD0 File Offset: 0x0002E1D0
		public event Action<string> OnBetaVersionSet;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06000A43 RID: 2627 RVA: 0x00030008 File Offset: 0x0002E208
		// (remove) Token: 0x06000A44 RID: 2628 RVA: 0x00030040 File Offset: 0x0002E240
		public event Action<string> OnPublicVersionSet;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06000A45 RID: 2629 RVA: 0x00030078 File Offset: 0x0002E278
		// (remove) Token: 0x06000A46 RID: 2630 RVA: 0x000300B0 File Offset: 0x0002E2B0
		public event Action<Roles> OnClientGameRoleSet;

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x000300E5 File Offset: 0x0002E2E5
		// (set) Token: 0x06000A48 RID: 2632 RVA: 0x000300ED File Offset: 0x0002E2ED
		public List<string> Versions { get; private set; } = new List<string>();

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000A49 RID: 2633 RVA: 0x000300F6 File Offset: 0x0002E2F6
		// (set) Token: 0x06000A4A RID: 2634 RVA: 0x000300FE File Offset: 0x0002E2FE
		public string BetaVersion { get; private set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x00030107 File Offset: 0x0002E307
		// (set) Token: 0x06000A4C RID: 2636 RVA: 0x0003010F File Offset: 0x0002E30F
		public string PublicVersion { get; private set; }

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x00030118 File Offset: 0x0002E318
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000A4E RID: 2638 RVA: 0x00030120 File Offset: 0x0002E320
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000A4F RID: 2639 RVA: 0x00030128 File Offset: 0x0002E328
		public string GetVersion(UIPublishStates publishState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetVersion", new object[] { publishState });
			}
			if (publishState == UIPublishStates.Beta)
			{
				return this.BetaVersion;
			}
			if (publishState != UIPublishStates.Public)
			{
				DebugUtility.LogNoEnumSupportError<UIPublishStates>(this, "GetVersion", publishState, new object[] { publishState });
				return null;
			}
			return this.PublicVersion;
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x0003018C File Offset: 0x0002E38C
		public async void Synchronize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			if (!this.subscribedAssetId.IsEmpty)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.UpdateClientGameRole));
			}
			this.Versions.Clear();
			this.BetaVersion = null;
			this.PublicVersion = null;
			this.OnLoadingStarted.Invoke();
			Action onSynchronizeStart = this.OnSynchronizeStart;
			if (onSynchronizeStart != null)
			{
				onSynchronizeStart();
			}
			this.subscribedAssetId = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.UpdateClientGameRole));
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, false);
			if (graphQlResult.HasErrors)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UIPublishModel_RetrievingAssetVersions, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				this.Versions = JsonConvert.DeserializeObject<string[]>(graphQlResult.GetDataMember().ToString()).ToList<string>();
				if (this.Versions == null)
				{
					ErrorHandler.HandleError(ErrorCodes.UIPublishModel_NullGameVersions, new NullReferenceException("Attempted to retrieve asset versions for game, but versions was null"), true, false);
				}
				else
				{
					this.Versions = this.Versions.OrderByDescending(new Func<string, Version>(Version.Parse)).ToList<string>();
					this.Versions.Insert(0, UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString());
					Action<List<string>> onVersionsSet = this.OnVersionsSet;
					if (onVersionsSet != null)
					{
						onVersionsSet(this.Versions);
					}
					GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetPublishedVersionsOfAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, false);
					if (graphQlResult2.HasErrors)
					{
						this.OnLoadingEnded.Invoke();
						ErrorHandler.HandleError(ErrorCodes.UIPublishModel_GetPublishedVersionsOfAsset, graphQlResult2.GetErrorMessage(0), true, false);
					}
					else
					{
						UIPublishModel.PublishedVersion[] array = JsonConvert.DeserializeObject<UIPublishModel.PublishedVersion[]>(graphQlResult2.GetDataMember().ToString());
						if (this.verboseLogging)
						{
							foreach (UIPublishModel.PublishedVersion publishedVersion in array)
							{
								DebugUtility.Log(publishedVersion.ToString(), this);
							}
						}
						bool flag = false;
						bool flag2 = false;
						foreach (UIPublishModel.PublishedVersion publishedVersion2 in array)
						{
							if (this.verboseLogging)
							{
								DebugUtility.Log(publishedVersion2.ToString(), this);
							}
							if (publishedVersion2.State == UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString())
							{
								this.BetaVersion = publishedVersion2.Asset_Version;
								flag = true;
							}
							if (publishedVersion2.State == UIPublishStates.Public.ToEndlessCloudServicesCompatibleString())
							{
								this.PublicVersion = publishedVersion2.Asset_Version;
								flag2 = true;
							}
							if (flag && flag2)
							{
								break;
							}
						}
						if (!flag)
						{
							this.BetaVersion = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
						}
						if (!flag2)
						{
							this.PublicVersion = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
						}
						Action<string> onBetaVersionSet = this.OnBetaVersionSet;
						if (onBetaVersionSet != null)
						{
							onBetaVersionSet(this.BetaVersion);
						}
						Action<string> onPublicVersionSet = this.OnPublicVersionSet;
						if (onPublicVersionSet != null)
						{
							onPublicVersionSet(this.PublicVersion);
						}
						this.OnLoadingEnded.Invoke();
					}
				}
			}
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x000301C4 File Offset: 0x0002E3C4
		private void UpdateClientGameRole(IReadOnlyList<UserRole> roles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateClientGameRole", new object[] { roles.Count });
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			Roles roleForUserId = roles.GetRoleForUserId(activeUserId);
			this.OnClientGameRoleSet(roleForUserId);
		}

		// Token: 0x04000888 RID: 2184
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000889 RID: 2185
		private SerializableGuid subscribedAssetId;

		// Token: 0x0200026D RID: 621
		public struct PublishedVersion
		{
			// Token: 0x06000A53 RID: 2643 RVA: 0x00030245 File Offset: 0x0002E445
			public override string ToString()
			{
				return "Asset_Version: " + this.Asset_Version + ", State: " + this.State;
			}

			// Token: 0x04000894 RID: 2196
			public string Asset_Version;

			// Token: 0x04000895 RID: 2197
			public string State;
		}
	}
}
