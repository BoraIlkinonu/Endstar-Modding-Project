using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.GraphQl;
using Endless.Shared;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator;

public class ParsedAssetRevision
{
	private class ParsedChangeType
	{
		private ChangeType changeType;

		private List<int> userIds = new List<int>();

		private List<string> metastrings = new List<string>();

		public ParsedChangeType(ChangeType changeType)
		{
			this.changeType = changeType;
		}

		internal void AddUser(int userId)
		{
			userIds.Add(userId);
		}

		internal void AddMetaString(string metastring)
		{
			metastrings.Add(metastring);
		}

		internal async Task<string> ParseChange()
		{
			Task<string[]> nameTask = Task.WhenAll(userIds.Select((int e) => MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(e)));
			Task<string[]> metastringTask = null;
			string metaresultString = null;
			if (metastrings.Count > 0)
			{
				metastringTask = Task.WhenAll(metastrings.Select((string e) => ResolveMetadata(changeType, e)));
			}
			DateTime startTime = DateTime.UtcNow;
			while (!nameTask.IsCompleted || (metastringTask != null && !metastringTask.IsCompleted))
			{
				if ((DateTime.UtcNow - startTime).TotalSeconds > 5.0)
				{
					if (!nameTask.IsCompleted)
					{
						Debug.LogError("An error occured fetching revision data for users " + GetFormattedList(userIds.Select((int id) => id.ToString()).ToArray()));
					}
					else if (metastringTask != null && !metastringTask.IsCompleted)
					{
						Debug.LogException(new Exception($"An error occured fetching metastring data for type {changeType}: {GetFormattedList(metastrings.ToArray())}"));
					}
					return "An error occured fetching revision data.";
				}
				await Task.Delay(100);
			}
			if (metastrings.Count > 0)
			{
				string[] inputStrings = metastringTask.Result.Where((string s) => !string.IsNullOrEmpty(s)).ToArray();
				metaresultString = GetFormattedList(inputStrings);
			}
			ChangeTypeAttribute attributeOfType = changeType.GetAttributeOfType<ChangeTypeAttribute>();
			_ = string.Empty;
			return string.Concat(str2: (attributeOfType == null) ? "did something entirely unknown!" : attributeOfType.ResolveMetaString(metaresultString), str0: GetFormattedList(nameTask.Result), str1: " ");
		}

		private static string GetFormattedList(string[] inputStrings)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(inputStrings[0]);
			if (inputStrings.Length == 2)
			{
				stringBuilder.Append(" and ");
				stringBuilder.Append(inputStrings[1]);
			}
			else if (inputStrings.Length > 2)
			{
				for (int i = 1; i < inputStrings.Length - 1; i++)
				{
					stringBuilder.Append(", ");
					stringBuilder.Append(inputStrings[i]);
				}
				stringBuilder.Append(" and ");
				stringBuilder.Append(inputStrings[^1]);
			}
			return stringBuilder.ToString();
		}

		private async Task<string> ResolveMetadata(ChangeType changeType, string metaData)
		{
			if (string.IsNullOrWhiteSpace(metaData))
			{
				return string.Empty;
			}
			switch (changeType)
			{
			case ChangeType.ChildAssetUpdated:
			{
				Asset asset = JsonConvert.DeserializeObject<Asset>((await EndlessServices.Instance.CloudService.GetAssetAsync(metaData)).GetDataMember().ToString());
				return asset.Name + " (" + asset.AssetType + ")";
			}
			case ChangeType.AssetVersionUpdated:
				return metaData;
			default:
				Debug.LogException(new NotImplementedException());
				return string.Empty;
			}
		}
	}

	private Action<string[]> onCompletedCallback;

	private Action<Exception> onFailCallback;

	public string[] Results { get; private set; }

	public bool IsCompleted => Results != null;

	public ParsedAssetRevision(string assetId, string version, Action<string[]> onCompletedCallback, Action<Exception> onFailCallback)
	{
		this.onCompletedCallback = onCompletedCallback;
		this.onFailCallback = onFailCallback;
		Action<object> onSuccessCallback = delegate(object result)
		{
			HandleAsset(JsonConvert.DeserializeObject<Asset>(result.ToString()));
		};
		GetAssetAsync(assetId, version, onSuccessCallback, onFailCallback);
	}

	private async void GetAssetAsync(string assetId, string version, Action<object> onSuccessCallback, Action<Exception> onFailCallback)
	{
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, version);
		if (graphQlResult.HasErrors)
		{
			onFailCallback?.Invoke(graphQlResult.GetErrorMessage());
		}
		else
		{
			onSuccessCallback?.Invoke(graphQlResult.GetDataMember());
		}
	}

	public ParsedAssetRevision(Asset asset, Action<string[]> onCompletedCallback, Action<Exception> onFailCallback)
	{
		this.onCompletedCallback = onCompletedCallback;
		this.onFailCallback = onFailCallback;
		HandleAsset(asset);
	}

	private void HandleAsset(Asset asset)
	{
		ParseAssetAsync(asset);
	}

	private async void ParseAssetAsync(Asset asset)
	{
		Dictionary<ChangeType, ParsedChangeType> dictionary = new Dictionary<ChangeType, ParsedChangeType>();
		foreach (ChangeData change in asset.RevisionMetaData.Changes)
		{
			if (!dictionary.ContainsKey(change.ChangeType))
			{
				dictionary.Add(change.ChangeType, new ParsedChangeType(change.ChangeType));
			}
			dictionary[change.ChangeType].AddUser(change.UserId);
			if (!string.IsNullOrWhiteSpace(change.Metadata))
			{
				dictionary[change.ChangeType].AddMetaString(change.Metadata);
			}
		}
		Results = await Task.WhenAll(dictionary.Values.Select((ParsedChangeType c) => c.ParseChange()));
		onCompletedCallback(Results);
	}
}
