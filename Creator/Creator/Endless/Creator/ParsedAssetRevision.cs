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

namespace Endless.Creator
{
	// Token: 0x02000089 RID: 137
	public class ParsedAssetRevision
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000206 RID: 518 RVA: 0x0000F816 File Offset: 0x0000DA16
		// (set) Token: 0x06000207 RID: 519 RVA: 0x0000F81E File Offset: 0x0000DA1E
		public string[] Results { get; private set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000208 RID: 520 RVA: 0x0000F827 File Offset: 0x0000DA27
		public bool IsCompleted
		{
			get
			{
				return this.Results != null;
			}
		}

		// Token: 0x06000209 RID: 521 RVA: 0x0000F834 File Offset: 0x0000DA34
		public ParsedAssetRevision(string assetId, string version, Action<string[]> onCompletedCallback, Action<Exception> onFailCallback)
		{
			this.onCompletedCallback = onCompletedCallback;
			this.onFailCallback = onFailCallback;
			Action<object> action = delegate(object result)
			{
				this.HandleAsset(JsonConvert.DeserializeObject<Asset>(result.ToString()));
			};
			this.GetAssetAsync(assetId, version, action, onFailCallback);
		}

		// Token: 0x0600020A RID: 522 RVA: 0x0000F870 File Offset: 0x0000DA70
		private async void GetAssetAsync(string assetId, string version, Action<object> onSuccessCallback, Action<Exception> onFailCallback)
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, version, null, false, 10);
			if (graphQlResult.HasErrors)
			{
				if (onFailCallback != null)
				{
					onFailCallback(graphQlResult.GetErrorMessage(0));
				}
			}
			else if (onSuccessCallback != null)
			{
				onSuccessCallback(graphQlResult.GetDataMember());
			}
		}

		// Token: 0x0600020B RID: 523 RVA: 0x0000F8C0 File Offset: 0x0000DAC0
		public ParsedAssetRevision(Asset asset, Action<string[]> onCompletedCallback, Action<Exception> onFailCallback)
		{
			this.onCompletedCallback = onCompletedCallback;
			this.onFailCallback = onFailCallback;
			this.HandleAsset(asset);
		}

		// Token: 0x0600020C RID: 524 RVA: 0x0000F8DD File Offset: 0x0000DADD
		private void HandleAsset(Asset asset)
		{
			this.ParseAssetAsync(asset);
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000F8E8 File Offset: 0x0000DAE8
		private async void ParseAssetAsync(Asset asset)
		{
			Dictionary<ChangeType, ParsedAssetRevision.ParsedChangeType> dictionary = new Dictionary<ChangeType, ParsedAssetRevision.ParsedChangeType>();
			foreach (ChangeData changeData in asset.RevisionMetaData.Changes)
			{
				if (!dictionary.ContainsKey(changeData.ChangeType))
				{
					dictionary.Add(changeData.ChangeType, new ParsedAssetRevision.ParsedChangeType(changeData.ChangeType));
				}
				dictionary[changeData.ChangeType].AddUser(changeData.UserId);
				if (!string.IsNullOrWhiteSpace(changeData.Metadata))
				{
					dictionary[changeData.ChangeType].AddMetaString(changeData.Metadata);
				}
			}
			string[] array = await Task.WhenAll<string>(dictionary.Values.Select((ParsedAssetRevision.ParsedChangeType c) => c.ParseChange()));
			this.Results = array;
			this.onCompletedCallback(this.Results);
		}

		// Token: 0x04000267 RID: 615
		private Action<string[]> onCompletedCallback;

		// Token: 0x04000268 RID: 616
		private Action<Exception> onFailCallback;

		// Token: 0x0200008A RID: 138
		private class ParsedChangeType
		{
			// Token: 0x0600020F RID: 527 RVA: 0x0000F93A File Offset: 0x0000DB3A
			public ParsedChangeType(ChangeType changeType)
			{
				this.changeType = changeType;
			}

			// Token: 0x06000210 RID: 528 RVA: 0x0000F95F File Offset: 0x0000DB5F
			internal void AddUser(int userId)
			{
				this.userIds.Add(userId);
			}

			// Token: 0x06000211 RID: 529 RVA: 0x0000F96D File Offset: 0x0000DB6D
			internal void AddMetaString(string metastring)
			{
				this.metastrings.Add(metastring);
			}

			// Token: 0x06000212 RID: 530 RVA: 0x0000F97C File Offset: 0x0000DB7C
			internal async Task<string> ParseChange()
			{
				Task<string[]> nameTask = Task.WhenAll<string>(this.userIds.Select((int e) => MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(e)));
				Task<string[]> metastringTask = null;
				string metaresultString = null;
				if (this.metastrings.Count > 0)
				{
					metastringTask = Task.WhenAll<string>(this.metastrings.Select((string e) => this.ResolveMetadata(this.changeType, e)));
				}
				DateTime startTime = DateTime.UtcNow;
				while (!nameTask.IsCompleted || (metastringTask != null && !metastringTask.IsCompleted))
				{
					if ((DateTime.UtcNow - startTime).TotalSeconds > 5.0)
					{
						if (!nameTask.IsCompleted)
						{
							Debug.LogError("An error occured fetching revision data for users " + ParsedAssetRevision.ParsedChangeType.GetFormattedList(this.userIds.Select((int id) => id.ToString()).ToArray<string>()));
						}
						else if (metastringTask != null && !metastringTask.IsCompleted)
						{
							Debug.LogException(new Exception(string.Format("An error occured fetching metastring data for type {0}: {1}", this.changeType, ParsedAssetRevision.ParsedChangeType.GetFormattedList(this.metastrings.ToArray()))));
						}
						return "An error occured fetching revision data.";
					}
					await Task.Delay(100);
				}
				if (this.metastrings.Count > 0)
				{
					metaresultString = ParsedAssetRevision.ParsedChangeType.GetFormattedList(metastringTask.Result.Where((string s) => !string.IsNullOrEmpty(s)).ToArray<string>());
				}
				ChangeTypeAttribute attributeOfType = this.changeType.GetAttributeOfType<ChangeTypeAttribute>();
				string text = string.Empty;
				if (attributeOfType != null)
				{
					text = attributeOfType.ResolveMetaString(metaresultString);
				}
				else
				{
					text = "did something entirely unknown!";
				}
				return ParsedAssetRevision.ParsedChangeType.GetFormattedList(nameTask.Result) + " " + text;
			}

			// Token: 0x06000213 RID: 531 RVA: 0x0000F9C0 File Offset: 0x0000DBC0
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
					stringBuilder.Append(inputStrings[inputStrings.Length - 1]);
				}
				return stringBuilder.ToString();
			}

			// Token: 0x06000214 RID: 532 RVA: 0x0000FA48 File Offset: 0x0000DC48
			private async Task<string> ResolveMetadata(ChangeType changeType, string metaData)
			{
				string text;
				if (string.IsNullOrWhiteSpace(metaData))
				{
					text = string.Empty;
				}
				else if (changeType != ChangeType.AssetVersionUpdated)
				{
					if (changeType == ChangeType.ChildAssetUpdated)
					{
						Asset asset = JsonConvert.DeserializeObject<Asset>((await EndlessServices.Instance.CloudService.GetAssetAsync(metaData, "", null, false, 10)).GetDataMember().ToString());
						text = asset.Name + " (" + asset.AssetType + ")";
					}
					else
					{
						Debug.LogException(new NotImplementedException());
						text = string.Empty;
					}
				}
				else
				{
					text = metaData;
				}
				return text;
			}

			// Token: 0x0400026A RID: 618
			private ChangeType changeType;

			// Token: 0x0400026B RID: 619
			private List<int> userIds = new List<int>();

			// Token: 0x0400026C RID: 620
			private List<string> metastrings = new List<string>();
		}
	}
}
