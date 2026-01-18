using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Creator.UI
{
	// Token: 0x020001A0 RID: 416
	public class UIAssetModerationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000610 RID: 1552 RVA: 0x0001F357 File Offset: 0x0001D557
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x06000611 RID: 1553 RVA: 0x0001F35F File Offset: 0x0001D55F
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x0001F367 File Offset: 0x0001D567
		private UIGameAsset GameAsset
		{
			get
			{
				return this.view.GameAsset;
			}
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x0001F374 File Offset: 0x0001D574
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.confirmButton.onClick.AddListener(new UnityAction(this.ApplyModerationFlags));
			CancellationTokenSourceUtility.RecreateTokenSource(ref this.applyModerationFlagsCancellationTokenSource);
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x0001F3C0 File Offset: 0x0001D5C0
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.applyModerationFlagsCancellationTokenSource);
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x0001F3E8 File Offset: 0x0001D5E8
		private async void ApplyModerationFlags()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyModerationFlags", Array.Empty<object>());
			}
			if (UIAssetModerationModalController.ContainsSameValues(this.uiModerationFlagsDropdown.Value, this.view.ExistingAssetModerationFlags))
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			}
			else
			{
				IReadOnlyList<ModerationFlag> value = this.uiModerationFlagsDropdown.Value;
				await this.ApplyModerationFlagsAsync(value, this.applyModerationFlagsCancellationTokenSource.Token);
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0001F420 File Offset: 0x0001D620
		private async Task ApplyModerationFlagsAsync(IReadOnlyList<ModerationFlag> targetFlags, CancellationToken cancellationToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyModerationFlagsAsync", new object[] { targetFlags, cancellationToken });
			}
			this.confirmButton.interactable = false;
			this.OnLoadingStarted.Invoke();
			List<ModerationFlag> flagsToRemove = new List<ModerationFlag>();
			foreach (ModerationFlag moderationFlag in ((IEnumerable<ModerationFlag>)this.view.ExistingAssetModerationFlags))
			{
				if (!targetFlags.Contains(moderationFlag))
				{
					flagsToRemove.Add(moderationFlag);
				}
			}
			try
			{
				string reason = this.reasonInputField.text;
				if (targetFlags.Any<ModerationFlag>())
				{
					await this.ProcessFlagOperationsAsync(targetFlags, true, reason, cancellationToken);
				}
				await this.ProcessFlagOperationsAsync(flagsToRemove, false, reason, cancellationToken);
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				reason = null;
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_ApplyModerationFlagsAsync, ex, true, false);
			}
			finally
			{
				this.OnLoadingEnded.Invoke();
				this.confirmButton.interactable = true;
			}
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x0001F474 File Offset: 0x0001D674
		private async Task ProcessFlagOperationsAsync(IReadOnlyList<ModerationFlag> flags, bool isAddOperation, string reason, CancellationToken cancellationToken)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ProcessFlagOperationsAsync", new object[] { flags, isAddOperation, reason, cancellationToken });
			}
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				string[] array = flags.Select((ModerationFlag flag) => flag.Code).ToArray<string>();
				if (isAddOperation)
				{
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetBulkObjectFlags(this.GameAsset.AssetID, array, reason, true);
					if (graphQlResult == null)
					{
						throw new NullReferenceException("GraphQLResult is null");
					}
					if (graphQlResult.HasErrors)
					{
						throw graphQlResult.GetErrorMessage(0);
					}
				}
				else
				{
					foreach (string text in array)
					{
						GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.RemoveObjectFlag(this.GameAsset.AssetID, text, true);
						if (graphQlResult == null)
						{
							throw new NullReferenceException("GraphQLResult is null");
						}
						if (graphQlResult.HasErrors)
						{
							throw graphQlResult.GetErrorMessage(0);
						}
					}
					string[] array2 = null;
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_ProcessFlagOperationsAsync, ex, true, false);
			}
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x0001F4D8 File Offset: 0x0001D6D8
		private static bool ContainsSameValues(IReadOnlyList<ModerationFlag> firstList, IReadOnlyList<ModerationFlag> secondList)
		{
			if (firstList == null && secondList == null)
			{
				return true;
			}
			if (firstList == null || secondList == null)
			{
				return false;
			}
			if (firstList.Count != secondList.Count)
			{
				return false;
			}
			if (firstList.Count == 0)
			{
				return true;
			}
			Dictionary<ModerationFlag, int> dictionary = new Dictionary<ModerationFlag, int>();
			foreach (ModerationFlag moderationFlag in firstList)
			{
				if (!dictionary.TryAdd(moderationFlag, 1))
				{
					Dictionary<ModerationFlag, int> dictionary2 = dictionary;
					ModerationFlag moderationFlag2 = moderationFlag;
					int num = dictionary2[moderationFlag2];
					dictionary2[moderationFlag2] = num + 1;
				}
			}
			foreach (ModerationFlag moderationFlag3 in secondList)
			{
				if (!dictionary.ContainsKey(moderationFlag3))
				{
					return false;
				}
				Dictionary<ModerationFlag, int> dictionary3 = dictionary;
				ModerationFlag moderationFlag2 = moderationFlag3;
				int num = dictionary3[moderationFlag2];
				dictionary3[moderationFlag2] = num - 1;
				if (dictionary[moderationFlag3] < 0)
				{
					return false;
				}
			}
			using (Dictionary<ModerationFlag, int>.ValueCollection.Enumerator enumerator2 = dictionary.Values.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					if (enumerator2.Current != 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x04000553 RID: 1363
		[SerializeField]
		private UIAssetModerationModalView view;

		// Token: 0x04000554 RID: 1364
		[SerializeField]
		private UIInputField reasonInputField;

		// Token: 0x04000555 RID: 1365
		[FormerlySerializedAs("moderationFlagsDropdown")]
		[SerializeField]
		private UIModerationFlagDropdown uiModerationFlagsDropdown;

		// Token: 0x04000556 RID: 1366
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000557 RID: 1367
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000558 RID: 1368
		private CancellationTokenSource applyModerationFlagsCancellationTokenSource;
	}
}
