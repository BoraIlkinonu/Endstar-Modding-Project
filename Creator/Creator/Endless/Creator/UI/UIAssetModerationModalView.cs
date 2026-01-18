using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Creator.UI
{
	// Token: 0x020001A5 RID: 421
	public class UIAssetModerationModalView : UIBaseModalView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x06000623 RID: 1571 RVA: 0x0001FD1E File Offset: 0x0001DF1E
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x06000624 RID: 1572 RVA: 0x0001FD26 File Offset: 0x0001DF26
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x06000625 RID: 1573 RVA: 0x0001FD2E File Offset: 0x0001DF2E
		// (set) Token: 0x06000626 RID: 1574 RVA: 0x0001FD36 File Offset: 0x0001DF36
		public UIGameAsset GameAsset { get; private set; }

		// Token: 0x170000A3 RID: 163
		// (get) Token: 0x06000627 RID: 1575 RVA: 0x0001FD3F File Offset: 0x0001DF3F
		public List<Moderation> ExistingAssetModerations { get; } = new List<Moderation>();

		// Token: 0x170000A4 RID: 164
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x0001FD47 File Offset: 0x0001DF47
		public List<ModerationFlag> ExistingAssetModerationFlags { get; } = new List<ModerationFlag>();

		// Token: 0x06000629 RID: 1577 RVA: 0x0001FD4F File Offset: 0x0001DF4F
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.getAppliedAssetModerationFlagCancellationTokenSource);
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x0001FD74 File Offset: 0x0001DF74
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.GameAsset = (UIGameAsset)modalData[0];
			this.gameAssetSummary.View(this.GameAsset);
			this.reasonInputField.Clear(true);
			CancellationTokenSourceUtility.RecreateTokenSource(ref this.getAppliedAssetModerationFlagCancellationTokenSource);
			this.RequestAndViewExistingAssetModerationFlagsAsync(this.getAppliedAssetModerationFlagCancellationTokenSource.Token);
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0001FDF4 File Offset: 0x0001DFF4
		public override void Close()
		{
			base.Close();
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.getAppliedAssetModerationFlagCancellationTokenSource);
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0001FE08 File Offset: 0x0001E008
		private async Task RequestAndViewExistingAssetModerationFlagsAsync(CancellationToken cancellationToken)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RequestAndViewExistingAssetModerationFlagsAsync", new object[] { cancellationToken });
			}
			this.OnLoadingStarted.Invoke();
			this.ExistingAssetModerations.Clear();
			this.ExistingAssetModerationFlags.Clear();
			try
			{
				if (cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException();
				}
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetObjectFlags(this.GameAsset.AssetID, true);
				if (graphQlResult.HasErrors)
				{
					throw graphQlResult.GetErrorMessage(0);
				}
				Debug.Log(string.Format("GetAssetObjectFlags response {0}", graphQlResult.GetDataMember()));
				var <>f__AnonymousType = new
				{
					identifier = string.Empty,
					moderations = Array.Empty<Moderation>()
				};
				var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
				if (<>f__AnonymousType2 == null)
				{
					throw new NullReferenceException("Unable to parse GetAssetObjectFlags result into valid structure containing moderations.");
				}
				foreach (Moderation moderation in <>f__AnonymousType2.moderations)
				{
					this.ExistingAssetModerations.Add(moderation);
					this.ExistingAssetModerationFlags.Add(moderation.flag);
				}
				this.uiModerationFlagsDropdown.SetValue(this.ExistingAssetModerationFlags, false);
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_RequestAndViewExistingAssetModerationFlags, ex, true, false);
			}
			finally
			{
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x04000573 RID: 1395
		[Header("UIAssetModerationModalView")]
		[SerializeField]
		private UIGameAssetSummaryView gameAssetSummary;

		// Token: 0x04000574 RID: 1396
		[SerializeField]
		private UIInputField reasonInputField;

		// Token: 0x04000575 RID: 1397
		[FormerlySerializedAs("moderationFlagsDropdown")]
		[SerializeField]
		private UIModerationFlagDropdown uiModerationFlagsDropdown;

		// Token: 0x04000576 RID: 1398
		private CancellationTokenSource getAppliedAssetModerationFlagCancellationTokenSource;
	}
}
