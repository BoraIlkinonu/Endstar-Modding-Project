using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001AE RID: 430
	public class UIGameAssetPublishModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x00020E36 File Offset: 0x0001F036
		// (set) Token: 0x0600065A RID: 1626 RVA: 0x00020E3E File Offset: 0x0001F03E
		public UIGameAssetPublishModalModel Model { get; private set; }

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600065B RID: 1627 RVA: 0x00020E47 File Offset: 0x0001F047
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600065C RID: 1628 RVA: 0x00020E4F File Offset: 0x0001F04F
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600065D RID: 1629 RVA: 0x00020E57 File Offset: 0x0001F057
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			if (modalData.Length != 1 || !(modalData[0] is UIGameAssetPublishModalModel))
			{
				DebugUtility.LogError("Invalid modalData provided to UIGameAssetPublishModalView.", this);
				return;
			}
			this.Model = modalData[0] as UIGameAssetPublishModalModel;
			this.AddListeners();
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x00020E90 File Offset: 0x0001F090
		public override void Close()
		{
			base.Close();
			this.RemoveListeners();
		}

		// Token: 0x0600065F RID: 1631 RVA: 0x00020EA0 File Offset: 0x0001F0A0
		private void ViewVersionsAndPublishedVersion(List<string> versions, List<PublishedVersion> publishedVersions)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewVersionsAndPublishedVersion", new object[] { versions.Count, publishedVersions.Count });
			}
			this.betaValues[0] = UIGameAssetPublishModalView.unpublishedKey;
			this.publicValues[0] = UIGameAssetPublishModalView.unpublishedKey;
			foreach (PublishedVersion publishedVersion in publishedVersions)
			{
				if (base.VerboseLogging)
				{
					DebugUtility.Log(string.Format("{0}: {1}", "publishedVersion", publishedVersion), this);
				}
				if (publishedVersion.State == UIGameAssetPublishModalView.betaKey)
				{
					this.betaValues[0] = publishedVersion.AssetVersion;
				}
				else if (publishedVersion.State == UIGameAssetPublishModalView.publicKey)
				{
					this.publicValues[0] = publishedVersion.AssetVersion;
				}
			}
			if (!versions.Contains(UIGameAssetPublishModalView.unpublishedKey))
			{
				versions.Insert(0, UIGameAssetPublishModalView.unpublishedKey);
			}
			if (base.VerboseLogging)
			{
				DebugUtility.DebugEnumerable<string>("versions", versions, this);
				DebugUtility.DebugEnumerable<char>("betaValues", this.betaValues[0], this);
				DebugUtility.DebugEnumerable<char>("publicValues", this.publicValues[0], this);
			}
			this.betaDropdown.SetOptionsAndValue(versions, this.betaValues[0], false);
			this.publicDropdown.SetOptionsAndValue(versions, this.publicValues[0], false);
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x00021014 File Offset: 0x0001F214
		private void AddListeners()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddListeners", Array.Empty<object>());
			}
			if (this.listenersAdded || this.Model == null)
			{
				return;
			}
			this.Model.OnLoadingStarted.AddListener(new UnityAction(this.OnLoadingStarted.Invoke));
			this.Model.OnLoadingEnded.AddListener(new UnityAction(this.OnLoadingEnded.Invoke));
			this.Model.OnModelChanged.AddListener(new UnityAction<List<string>, List<PublishedVersion>>(this.ViewVersionsAndPublishedVersion));
			this.listenersAdded = true;
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x000210B0 File Offset: 0x0001F2B0
		private void RemoveListeners()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveListeners", Array.Empty<object>());
			}
			if (!this.listenersAdded || this.Model == null)
			{
				return;
			}
			this.Model.OnLoadingStarted.RemoveListener(new UnityAction(this.OnLoadingStarted.Invoke));
			this.Model.OnLoadingEnded.RemoveListener(new UnityAction(this.OnLoadingEnded.Invoke));
			this.Model.OnModelChanged.RemoveListener(new UnityAction<List<string>, List<PublishedVersion>>(this.ViewVersionsAndPublishedVersion));
			this.listenersAdded = false;
		}

		// Token: 0x040005AD RID: 1453
		private static readonly string betaKey = UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString();

		// Token: 0x040005AE RID: 1454
		private static readonly string publicKey = UIPublishStates.Public.ToEndlessCloudServicesCompatibleString();

		// Token: 0x040005AF RID: 1455
		private static readonly string unpublishedKey = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

		// Token: 0x040005B0 RID: 1456
		[Header("UIGameAssetPublishModalView")]
		[SerializeField]
		private UIDropdownVersion betaDropdown;

		// Token: 0x040005B1 RID: 1457
		[SerializeField]
		private UIDropdownVersion publicDropdown;

		// Token: 0x040005B2 RID: 1458
		private readonly string[] betaValues = new string[] { UIGameAssetPublishModalView.unpublishedKey };

		// Token: 0x040005B3 RID: 1459
		private readonly string[] publicValues = new string[] { UIGameAssetPublishModalView.unpublishedKey };

		// Token: 0x040005B4 RID: 1460
		private bool listenersAdded;
	}
}
