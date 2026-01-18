using System;
using System.Linq;
using Endless.Assets;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000154 RID: 340
	public class UIRevisionListModel : UIBaseLocalFilterableListModel<string>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000525 RID: 1317 RVA: 0x0001C370 File Offset: 0x0001A570
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000526 RID: 1318 RVA: 0x0001C378 File Offset: 0x0001A578
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000527 RID: 1319 RVA: 0x0001C380 File Offset: 0x0001A580
		protected override Comparison<string> DefaultSort
		{
			get
			{
				return (string x, string y) => string.Compare(x, y, StringComparison.Ordinal);
			}
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x0001C3A1 File Offset: 0x0001A5A1
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
			this.parsedAssetRevision = null;
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0001C3D0 File Offset: 0x0001A5D0
		public void Initialize(Asset asset, string version)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { asset.AssetID, version });
			}
			this.OnLoadingStarted.Invoke();
			this.parsedAssetRevision = new ParsedAssetRevision(asset.AssetID, version, new Action<string[]>(this.OnRequestSuccess), new Action<Exception>(this.OnRequestError));
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0001C438 File Offset: 0x0001A638
		private void OnRequestSuccess(string[] result)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRequestSuccess", new object[] { StringUtility.CommaSeparate(result) });
			}
			this.OnLoadingEnded.Invoke();
			this.Set(result.ToList<string>(), true);
		}

		// Token: 0x0600052B RID: 1323 RVA: 0x0001C474 File Offset: 0x0001A674
		private void OnRequestError(Exception exception)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRequestError", new object[] { exception.Message });
			}
			this.OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.UIRevisionListModel_RetrievingRevision, exception, true, false);
		}

		// Token: 0x040004AB RID: 1195
		private ParsedAssetRevision parsedAssetRevision;
	}
}
