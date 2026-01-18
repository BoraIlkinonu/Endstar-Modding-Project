using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public abstract class UIBaseSocialListModel<T>
{
	protected readonly List<T> items = new List<T>();

	protected bool isLoading;

	public bool VerboseLogging;

	public IReadOnlyList<T> Items => items;

	protected abstract Task<GraphQlResult> RequestListTask { get; }

	protected abstract ErrorCodes GetDataErrorCode { get; }

	public event Action OnModelChanged;

	public async Task<List<T>> RequestListAsync()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("RequestListAsync");
		}
		if (isLoading)
		{
			return new List<T>();
		}
		isLoading = true;
		GraphQlResult graphQlResult = await RequestListTask;
		if (graphQlResult.HasErrors)
		{
			Exception errorMessage = graphQlResult.GetErrorMessage();
			ErrorHandler.HandleError(GetDataErrorCode, errorMessage);
			return new List<T>();
		}
		T[] range = ExtractData(graphQlResult);
		AddExtractedData(range);
		isLoading = false;
		OnLoadComplete();
		return items;
	}

	protected virtual void OnLoadComplete()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("OnLoadComplete");
		}
		InvokeOnModelChanged();
	}

	protected virtual void AddExtractedData(T[] range)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AddExtractedData", "range", range.Length));
		}
		items.AddRange(range);
	}

	protected abstract T[] ExtractData(GraphQlResult graphQlResult);

	public void Add(T item)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Add", "item", item));
		}
		items.Add(item);
		InvokeOnModelChanged();
	}

	public void Remove(T item)
	{
		if (VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Remove", "item", item));
		}
		if (items.Contains(item))
		{
			items.Remove(item);
			InvokeOnModelChanged();
			return;
		}
		DebugUtility.LogWarning(string.Format("{0} ( {1}: {2} ) | {3} does not contain {4}", "Remove", "item", item, "items", "item"));
	}

	public virtual void Clear()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Clear");
		}
		items.Clear();
	}

	public abstract bool RemoveItemWithId(int itemId);

	protected void InvokeOnModelChanged()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("InvokeOnModelChanged");
		}
		this.OnModelChanged?.Invoke();
	}
}
