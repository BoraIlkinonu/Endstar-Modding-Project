using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004D1 RID: 1233
	public class AudioLibrary
	{
		// Token: 0x06001E84 RID: 7812 RVA: 0x000850D5 File Offset: 0x000832D5
		public AudioLibrary(global::UnityEngine.Object fileAccessor)
		{
			this.fileAccessor = fileAccessor;
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsRepopulateRequired(Game newGame, Game oldGame)
		{
			return true;
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x000850F0 File Offset: 0x000832F0
		public async Task RepopulateAudio(CancellationToken cancellationToken)
		{
			this.UnloadAssetsNotInGameLibrary();
			await this.PreloadData(cancellationToken, null);
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x0008513C File Offset: 0x0008333C
		public List<RuntimeAudioInfo> UnloadAssetsNotInGameLibrary()
		{
			List<RuntimeAudioInfo> list = new List<RuntimeAudioInfo>();
			using (Dictionary<AssetReference, RuntimeAudioInfo>.Enumerator enumerator = this.loadedAudio.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<AssetReference, RuntimeAudioInfo> entry = enumerator.Current;
					if (!MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == entry.Value.AudioAsset.AssetID))
					{
						list.Add(entry.Value);
					}
				}
			}
			foreach (RuntimeAudioInfo runtimeAudioInfo in list)
			{
				this.UnloadAudio(runtimeAudioInfo);
			}
			return list;
		}

		// Token: 0x06001E88 RID: 7816 RVA: 0x00085214 File Offset: 0x00083414
		private void UnloadAudio(RuntimeAudioInfo audioAssetInfoToRemove)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this.fileAccessor, audioAssetInfoToRemove.AudioAsset.IconFileInstanceId);
			this.loadedAudio.Remove(audioAssetInfoToRemove.AudioAsset.ToAssetReference());
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x00085248 File Offset: 0x00083448
		public async Task PreloadData(CancellationToken cancellationToken, Action<int, int> audioLoadingUpdate = null)
		{
			AudioLibrary.<>c__DisplayClass7_0 CS$<>8__locals1 = new AudioLibrary.<>c__DisplayClass7_0();
			IReadOnlyList<AssetReference> audioToPreload = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.AudioReferences;
			BulkAssetCacheResult<AudioAsset> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<AudioAsset>(audioToPreload.ToArray<AssetReference>());
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			if (!cancellationToken.IsCancellationRequested)
			{
				CS$<>8__locals1.iconTextures = new Texture2D[CS$<>8__locals1.bulkResult.Assets.Count];
				await TaskUtilities.ProcessTasksWithSimultaneousCap(CS$<>8__locals1.bulkResult.Assets.Count, 10, new Func<int, Task>(CS$<>8__locals1.<PreloadData>g__LoadTexture|0), cancellationToken, false);
				if (cancellationToken.IsCancellationRequested)
				{
					foreach (AudioAsset audioAsset2 in CS$<>8__locals1.bulkResult.Assets)
					{
						MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, audioAsset2.IconFileInstanceId);
					}
				}
				else
				{
					for (int index = 0; index < CS$<>8__locals1.bulkResult.Assets.Count; index++)
					{
						AudioAsset audioAsset = CS$<>8__locals1.bulkResult.Assets[index];
						RuntimeAudioInfo oldInfo = null;
						RuntimeAudioInfo runtimeAudioInfo;
						if (!this.loadedAudio.TryGetValue(audioAsset.ToAssetReference(), out runtimeAudioInfo))
						{
							this.TryGetRuntimeAudioInfo(audioAsset.AssetID, out oldInfo);
							await MonoBehaviourSingleton<LoadedFileManager>.Instance.EnsureAudioClipIsDownloaded(audioAsset.AudioFileInstanceId, audioAsset.AudioType);
							if (cancellationToken.IsCancellationRequested)
							{
								break;
							}
							Sprite sprite = null;
							try
							{
								Texture2D texture2D = CS$<>8__locals1.iconTextures[index];
								if (cancellationToken.IsCancellationRequested)
								{
									MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this.fileAccessor, audioAsset.IconFileInstanceId);
									break;
								}
								sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero);
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
								if (cancellationToken.IsCancellationRequested)
								{
									break;
								}
							}
							RuntimeAudioInfo runtimeAudioInfo2 = new RuntimeAudioInfo();
							runtimeAudioInfo2.Icon = sprite;
							runtimeAudioInfo2.AudioAsset = audioAsset;
							if (oldInfo != null)
							{
								this.UnloadAudio(oldInfo);
							}
							this.loadedAudio.Add(audioAsset.ToAssetReference(), runtimeAudioInfo2);
							if (audioLoadingUpdate != null)
							{
								audioLoadingUpdate(this.loadedAudio.Count, audioToPreload.Count);
							}
							audioAsset = null;
							oldInfo = null;
						}
					}
				}
			}
		}

		// Token: 0x06001E8A RID: 7818 RVA: 0x0008529C File Offset: 0x0008349C
		public bool TryGetRuntimeAudioInfo(SerializableGuid assetId, out RuntimeAudioInfo metadata)
		{
			AssetReference assetReference = this.loadedAudio.Keys.FirstOrDefault((AssetReference reference) => reference.AssetID == assetId);
			if (assetReference == null)
			{
				metadata = null;
				return false;
			}
			return this.loadedAudio.TryGetValue(assetReference, out metadata);
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x000852EE File Offset: 0x000834EE
		public bool TryGetRuntimeAudioInfo(AssetReference assetReference, out RuntimeAudioInfo metadata)
		{
			return this.loadedAudio.TryGetValue(assetReference, out metadata);
		}

		// Token: 0x06001E8C RID: 7820 RVA: 0x00085300 File Offset: 0x00083500
		public void UnloadAll()
		{
			AssetReference[] array = this.loadedAudio.Keys.ToArray<AssetReference>();
			for (int i = 0; i < array.Length; i++)
			{
				this.UnloadAudio(this.loadedAudio[array[i]]);
			}
		}

		// Token: 0x06001E8D RID: 7821 RVA: 0x00085340 File Offset: 0x00083540
		public IReadOnlyList<AudioReference> GetLoadedAudioAsAudioReferences()
		{
			AudioReference[] array = new AudioReference[this.loadedAudio.Count];
			int num = 0;
			foreach (KeyValuePair<AssetReference, RuntimeAudioInfo> keyValuePair in this.loadedAudio)
			{
				AudioReference audioReference = ReferenceFactory.CreateAudioReference(keyValuePair.Value.AudioAsset.AssetID);
				array[num] = audioReference;
				num++;
			}
			return array;
		}

		// Token: 0x04001779 RID: 6009
		private Dictionary<AssetReference, RuntimeAudioInfo> loadedAudio = new Dictionary<AssetReference, RuntimeAudioInfo>();

		// Token: 0x0400177A RID: 6010
		private global::UnityEngine.Object fileAccessor;
	}
}
