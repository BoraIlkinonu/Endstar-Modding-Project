using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000268 RID: 616
	public class UIPublishController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x06000A2A RID: 2602 RVA: 0x0002F684 File Offset: 0x0002D884
		public void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.betaDropdown.OnValueChanged.AddListener(new UnityAction(this.PublishBeta));
			this.publicDropdown.OnValueChanged.AddListener(new UnityAction(this.PublishPublic));
			this.confirmationDictionary.Add(UIPublishStates.Beta, this.publishConfirmationBeta);
			this.confirmationDictionary.Add(UIPublishStates.Public, this.publishConfirmationPublic);
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000A2B RID: 2603 RVA: 0x0002F705 File Offset: 0x0002D905
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000A2C RID: 2604 RVA: 0x0002F70D File Offset: 0x0002D90D
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000A2D RID: 2605 RVA: 0x0002F715 File Offset: 0x0002D915
		private void PublishBeta()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PublishBeta", Array.Empty<object>());
			}
			this.SetVersion(UIPublishStates.Beta, this.betaDropdown.IndexOfValue);
		}

		// Token: 0x06000A2E RID: 2606 RVA: 0x0002F741 File Offset: 0x0002D941
		private void PublishPublic()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PublishPublic", Array.Empty<object>());
			}
			this.SetVersion(UIPublishStates.Public, this.publicDropdown.IndexOfValue);
		}

		// Token: 0x06000A2F RID: 2607 RVA: 0x0002F770 File Offset: 0x0002D970
		private void SetVersion(UIPublishStates publishingState, int dropdownIndex)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetVersion", new object[] { publishingState, dropdownIndex });
			}
			string text = this.model.Versions[dropdownIndex];
			string version = this.model.GetVersion(publishingState);
			bool flag = text == UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
			bool flag2 = !flag && version != UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", new object[] { "versionToPublish", text, "activeVersion", version, "isUnpublish", flag, "needsUnpublishingBeforePublish", flag2 }), this);
			}
			if (text == version)
			{
				return;
			}
			this.OnLoadingStarted.Invoke();
			if (flag)
			{
				this.Unpublish(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, version, delegate
				{
					this.OnLoadingEnded.Invoke();
					this.model.Synchronize();
				});
				return;
			}
			this.ConfirmPublish(publishingState, text, flag2);
		}

		// Token: 0x06000A30 RID: 2608 RVA: 0x0002F884 File Offset: 0x0002DA84
		private void ConfirmPublish(UIPublishStates publishingState, string version, bool needsUnpublishing)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmPublish", new object[] { publishingState, version, needsUnpublishing });
			}
			string assetId = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID;
			string text = string.Concat(new string[]
			{
				"Set v",
				version,
				" to ",
				publishingState.ToEndlessCloudServicesCompatibleString(),
				"?\n",
				this.confirmationDictionary[publishingState]
			});
			Action <>9__2;
			MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(text, delegate
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack(new object[] { false });
				if (needsUnpublishing)
				{
					string version2 = this.model.GetVersion(publishingState);
					UIPublishController <>4__this = this;
					string assetId2 = assetId;
					string text2 = version2;
					Action action;
					if ((action = <>9__2) == null)
					{
						action = (<>9__2 = delegate
						{
							this.Publish(assetId, version, publishingState);
						});
					}
					<>4__this.Unpublish(assetId2, text2, action);
					return;
				}
				this.Publish(assetId, version, publishingState);
			}, delegate
			{
				this.OnLoadingEnded.Invoke();
				this.model.Synchronize();
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			}, UIModalManagerStackActions.MaintainStack);
		}

		// Token: 0x06000A31 RID: 2609 RVA: 0x0002F97C File Offset: 0x0002DB7C
		private async void Unpublish(string assetId, string version, Action onSuccess)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Unpublish", new object[]
				{
					assetId,
					version,
					onSuccess.DebugIsNull()
				});
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString(), false);
			if (graphQlResult.HasErrors)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UIPublishController_UnpublishAsset, graphQlResult.GetErrorMessage(0), true, false);
				this.model.Synchronize();
			}
			else if (onSuccess != null)
			{
				onSuccess();
			}
		}

		// Token: 0x06000A32 RID: 2610 RVA: 0x0002F9CC File Offset: 0x0002DBCC
		private async void Publish(string assetId, string version, UIPublishStates publishingState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Publish", new object[] { assetId, version, publishingState });
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, publishingState.ToEndlessCloudServicesCompatibleString(), false);
			if (graphQlResult.HasErrors)
			{
				this.OnLoadingEnded.Invoke();
				ErrorHandler.HandleError(ErrorCodes.UIPublishController_PublishAsset, graphQlResult.GetErrorMessage(0), true, false);
				this.model.Synchronize();
			}
			else
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "Publish", "Success!", new object[] { assetId, version, publishingState });
				}
				this.OnLoadingEnded.Invoke();
				this.model.Synchronize();
			}
		}

		// Token: 0x0400086A RID: 2154
		[SerializeField]
		private UIPublishModel model;

		// Token: 0x0400086B RID: 2155
		[SerializeField]
		private UIPublishView view;

		// Token: 0x0400086C RID: 2156
		[SerializeField]
		private UIDropdownVersion betaDropdown;

		// Token: 0x0400086D RID: 2157
		[SerializeField]
		private UIDropdownVersion publicDropdown;

		// Token: 0x0400086E RID: 2158
		[SerializeField]
		[TextArea]
		private string publishConfirmationBeta = "Publishing will make your game available to all players in the " + UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString() + " channel.";

		// Token: 0x0400086F RID: 2159
		[SerializeField]
		[TextArea]
		private string publishConfirmationPublic = "Publishing will make your game available to all players in the " + UIPublishStates.Public.ToEndlessCloudServicesCompatibleString() + " channel.";

		// Token: 0x04000870 RID: 2160
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000871 RID: 2161
		private readonly Dictionary<UIPublishStates, string> confirmationDictionary = new Dictionary<UIPublishStates, string>();
	}
}
