using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200019A RID: 410
	public class UIVersionListModel : UIBaseCloudListModel<string>
	{
		// Token: 0x060005FD RID: 1533 RVA: 0x0001EA10 File Offset: 0x0001CC10
		public void Initialize(SerializableGuid assetId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { assetId });
			}
			if (this.assetId != assetId)
			{
				this.CancelAllOperations();
				this.versionTimestampCache.Clear();
				if (this.Count > 0)
				{
					this.Clear(true);
				}
			}
			this.assetId = assetId;
			this.RequestAsync(null);
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x0001EA80 File Offset: 0x0001CC80
		public override Task RequestAsync(Action requestSuccessAction)
		{
			UIVersionListModel.<RequestAsync>d__6 <RequestAsync>d__;
			<RequestAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<RequestAsync>d__.<>4__this = this;
			<RequestAsync>d__.requestSuccessAction = requestSuccessAction;
			<RequestAsync>d__.<>1__state = -1;
			<RequestAsync>d__.<>t__builder.Start<UIVersionListModel.<RequestAsync>d__6>(ref <RequestAsync>d__);
			return <RequestAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x0001EACC File Offset: 0x0001CCCC
		public Task GetTimestampAsync(string version, Action<string, DateTime> callback)
		{
			UIVersionListModel.<GetTimestampAsync>d__7 <GetTimestampAsync>d__;
			<GetTimestampAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<GetTimestampAsync>d__.<>4__this = this;
			<GetTimestampAsync>d__.version = version;
			<GetTimestampAsync>d__.callback = callback;
			<GetTimestampAsync>d__.<>1__state = -1;
			<GetTimestampAsync>d__.<>t__builder.Start<UIVersionListModel.<GetTimestampAsync>d__7>(ref <GetTimestampAsync>d__);
			return <GetTimestampAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x0001EB20 File Offset: 0x0001CD20
		public void CancelAllOperations()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelAllOperations", Array.Empty<object>());
			}
			CancellationTokenSource cancellationTokenSource = this.requestCancellationTokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = this.requestCancellationTokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			this.requestCancellationTokenSource = null;
			foreach (CancellationTokenSource cancellationTokenSource3 in this.timestampRequestTokens.Values)
			{
				cancellationTokenSource3.Cancel();
				cancellationTokenSource3.Dispose();
			}
			this.timestampRequestTokens.Clear();
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x0001EBC8 File Offset: 0x0001CDC8
		protected override void OnRequestSuccess(object result)
		{
			base.OnRequestSuccess(result);
			string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(result);
			this.Set(new List<string>(parsedAndOrderedVersions), false);
			this.Select(0, true);
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x0001EBF8 File Offset: 0x0001CDF8
		protected override void HandleError(Exception exception)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleError", new object[] { exception.Message });
			}
			base.OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIVersionListModel_RetrievingAssetVersions, exception, true, false);
		}

		// Token: 0x04000539 RID: 1337
		private const string REVISION_META_DATA_KEY = "revision_meta_data";

		// Token: 0x0400053A RID: 1338
		private readonly Dictionary<string, DateTime> versionTimestampCache = new Dictionary<string, DateTime>();

		// Token: 0x0400053B RID: 1339
		private readonly Dictionary<string, CancellationTokenSource> timestampRequestTokens = new Dictionary<string, CancellationTokenSource>();

		// Token: 0x0400053C RID: 1340
		private SerializableGuid assetId = SerializableGuid.Empty;

		// Token: 0x0400053D RID: 1341
		private CancellationTokenSource requestCancellationTokenSource;
	}
}
