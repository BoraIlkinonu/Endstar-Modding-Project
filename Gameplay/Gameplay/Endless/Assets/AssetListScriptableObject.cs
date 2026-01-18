using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Assets
{
	// Token: 0x0200004F RID: 79
	public abstract class AssetListScriptableObject<T> : ScriptableObject, IAssetListEditorHooks where T : AssetList
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x0600013C RID: 316 RVA: 0x0000767A File Offset: 0x0000587A
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600013D RID: 317 RVA: 0x00007682 File Offset: 0x00005882
		public string Description
		{
			get
			{
				return this.description;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000768A File Offset: 0x0000588A
		[ContextMenu("Copy asset list Id")]
		public void CopyListAssetId()
		{
			GUIUtility.systemCopyBuffer = this.assetListId.ToString();
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000076A4 File Offset: 0x000058A4
		[return: TupleElementNames(new string[] { "asset", "versions" })]
		public async Task<List<ValueTuple<AssetCore, List<PublishedVersion>>>> GetData()
		{
			List<Task<GraphQlResult>> tasks = new List<Task<GraphQlResult>>();
			Dictionary<Task<GraphQlResult>, AssetCore> taskIdMap = new Dictionary<Task<GraphQlResult>, AssetCore>();
			foreach (AssetCore assetCore in this.assets)
			{
				Task<GraphQlResult> publishedVersionsOfAssetAsync = EndlessServices.Instance.CloudService.GetPublishedVersionsOfAssetAsync(assetCore.AssetID, false);
				taskIdMap.Add(publishedVersionsOfAssetAsync, assetCore);
				tasks.Add(publishedVersionsOfAssetAsync);
			}
			await Task.WhenAll<GraphQlResult>(tasks);
			List<ValueTuple<AssetCore, List<PublishedVersion>>> list = new List<ValueTuple<AssetCore, List<PublishedVersion>>>();
			foreach (Task<GraphQlResult> task in tasks)
			{
				GraphQlResult result = task.Result;
				if (!result.HasErrors)
				{
					List<PublishedVersion> list2 = JsonConvert.DeserializeObject<List<PublishedVersion>>(result.GetDataMember().ToString());
					list.Add(new ValueTuple<AssetCore, List<PublishedVersion>>(taskIdMap[task], list2));
				}
			}
			return list;
		}

		// Token: 0x04000109 RID: 265
		[SerializeField]
		protected new string name;

		// Token: 0x0400010A RID: 266
		[SerializeField]
		protected string description;

		// Token: 0x0400010B RID: 267
		[SerializeField]
		protected List<AssetCore> assets = new List<AssetCore>();

		// Token: 0x0400010C RID: 268
		[SerializeField]
		protected SerializableGuid assetListId;

		// Token: 0x0400010D RID: 269
		private bool isSaving;
	}
}
