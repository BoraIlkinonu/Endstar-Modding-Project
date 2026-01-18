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

namespace Endless.Creator.UI;

public class UIAssetModerationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIAssetModerationModalView view;

	[SerializeField]
	private UIInputField reasonInputField;

	[FormerlySerializedAs("moderationFlagsDropdown")]
	[SerializeField]
	private UIModerationFlagDropdown uiModerationFlagsDropdown;

	[SerializeField]
	private UIButton confirmButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private CancellationTokenSource applyModerationFlagsCancellationTokenSource;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private UIGameAsset GameAsset => view.GameAsset;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		confirmButton.onClick.AddListener(ApplyModerationFlags);
		CancellationTokenSourceUtility.RecreateTokenSource(ref applyModerationFlagsCancellationTokenSource);
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		CancellationTokenSourceUtility.CancelAndCleanup(ref applyModerationFlagsCancellationTokenSource);
	}

	private async void ApplyModerationFlags()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyModerationFlags");
		}
		if (ContainsSameValues(uiModerationFlagsDropdown.Value, view.ExistingAssetModerationFlags))
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			return;
		}
		IReadOnlyList<ModerationFlag> value = uiModerationFlagsDropdown.Value;
		await ApplyModerationFlagsAsync(value, applyModerationFlagsCancellationTokenSource.Token);
	}

	private async Task ApplyModerationFlagsAsync(IReadOnlyList<ModerationFlag> targetFlags, CancellationToken cancellationToken)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyModerationFlagsAsync", targetFlags, cancellationToken);
		}
		confirmButton.interactable = false;
		OnLoadingStarted.Invoke();
		List<ModerationFlag> flagsToRemove = new List<ModerationFlag>();
		foreach (ModerationFlag item in (IEnumerable<ModerationFlag>)view.ExistingAssetModerationFlags)
		{
			if (!targetFlags.Contains(item))
			{
				flagsToRemove.Add(item);
			}
		}
		try
		{
			string reason = reasonInputField.text;
			if (targetFlags.Any())
			{
				await ProcessFlagOperationsAsync(targetFlags, isAddOperation: true, reason, cancellationToken);
			}
			await ProcessFlagOperationsAsync(flagsToRemove, isAddOperation: false, reason, cancellationToken);
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_ApplyModerationFlagsAsync, exception);
		}
		finally
		{
			OnLoadingEnded.Invoke();
			confirmButton.interactable = true;
		}
	}

	private async Task ProcessFlagOperationsAsync(IReadOnlyList<ModerationFlag> flags, bool isAddOperation, string reason, CancellationToken cancellationToken)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ProcessFlagOperationsAsync", flags, isAddOperation, reason, cancellationToken);
		}
		try
		{
			cancellationToken.ThrowIfCancellationRequested();
			string[] array = flags.Select((ModerationFlag flag) => flag.Code).ToArray();
			if (isAddOperation)
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetBulkObjectFlags(GameAsset.AssetID, array, reason, debugQuery: true);
				if (graphQlResult != null)
				{
					if (graphQlResult.HasErrors)
					{
						throw graphQlResult.GetErrorMessage();
					}
					return;
				}
				throw new NullReferenceException("GraphQLResult is null");
			}
			string[] array2 = array;
			foreach (string flagCode in array2)
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.RemoveObjectFlag(GameAsset.AssetID, flagCode, debugQuery: true);
				if (graphQlResult != null)
				{
					if (graphQlResult.HasErrors)
					{
						throw graphQlResult.GetErrorMessage();
					}
					continue;
				}
				throw new NullReferenceException("GraphQLResult is null");
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIAssetModerationModalModel_ProcessFlagOperationsAsync, exception);
		}
	}

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
		foreach (ModerationFlag first in firstList)
		{
			if (!dictionary.TryAdd(first, 1))
			{
				dictionary[first]++;
			}
		}
		foreach (ModerationFlag second in secondList)
		{
			if (!dictionary.ContainsKey(second))
			{
				return false;
			}
			dictionary[second]--;
			if (dictionary[second] < 0)
			{
				return false;
			}
		}
		foreach (int value in dictionary.Values)
		{
			if (value != 0)
			{
				return false;
			}
		}
		return true;
	}
}
