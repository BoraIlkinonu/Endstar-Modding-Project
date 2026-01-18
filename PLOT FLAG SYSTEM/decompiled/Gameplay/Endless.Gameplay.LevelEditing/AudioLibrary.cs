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

namespace Endless.Gameplay.LevelEditing;

public class AudioLibrary
{
	private Dictionary<AssetReference, RuntimeAudioInfo> loadedAudio = new Dictionary<AssetReference, RuntimeAudioInfo>();

	private UnityEngine.Object fileAccessor;

	public AudioLibrary(UnityEngine.Object fileAccessor)
	{
		this.fileAccessor = fileAccessor;
	}

	public bool IsRepopulateRequired(Game newGame, Game oldGame)
	{
		return true;
	}

	public async Task RepopulateAudio(CancellationToken cancellationToken)
	{
		UnloadAssetsNotInGameLibrary();
		await PreloadData(cancellationToken);
	}

	public List<RuntimeAudioInfo> UnloadAssetsNotInGameLibrary()
	{
		List<RuntimeAudioInfo> list = new List<RuntimeAudioInfo>();
		foreach (KeyValuePair<AssetReference, RuntimeAudioInfo> entry in loadedAudio)
		{
			if (!MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == entry.Value.AudioAsset.AssetID))
			{
				list.Add(entry.Value);
			}
		}
		foreach (RuntimeAudioInfo item in list)
		{
			UnloadAudio(item);
		}
		return list;
	}

	private void UnloadAudio(RuntimeAudioInfo audioAssetInfoToRemove)
	{
		MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(fileAccessor, audioAssetInfoToRemove.AudioAsset.IconFileInstanceId);
		loadedAudio.Remove(audioAssetInfoToRemove.AudioAsset.ToAssetReference());
	}

	public async Task PreloadData(CancellationToken cancellationToken, Action<int, int> audioLoadingUpdate = null)
	{
		IReadOnlyList<AssetReference> audioToPreload = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.AudioReferences;
		BulkAssetCacheResult<AudioAsset> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<AudioAsset>(audioToPreload.ToArray());
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}
		Texture2D[] iconTextures = new Texture2D[bulkResult.Assets.Count];
		await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, LoadTexture, cancellationToken);
		if (cancellationToken.IsCancellationRequested)
		{
			foreach (AudioAsset asset in bulkResult.Assets)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, asset.IconFileInstanceId);
			}
			return;
		}
		for (int index = 0; index < bulkResult.Assets.Count; index++)
		{
			AudioAsset audioAsset = bulkResult.Assets[index];
			RuntimeAudioInfo oldInfo = null;
			if (loadedAudio.TryGetValue(audioAsset.ToAssetReference(), out var _))
			{
				continue;
			}
			TryGetRuntimeAudioInfo(audioAsset.AssetID, out oldInfo);
			await MonoBehaviourSingleton<LoadedFileManager>.Instance.EnsureAudioClipIsDownloaded(audioAsset.AudioFileInstanceId, audioAsset.AudioType);
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}
			Sprite icon = null;
			try
			{
				Texture2D texture2D = iconTextures[index];
				if (cancellationToken.IsCancellationRequested)
				{
					MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(fileAccessor, audioAsset.IconFileInstanceId);
					break;
				}
				icon = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}
			}
			RuntimeAudioInfo value2 = new RuntimeAudioInfo
			{
				Icon = icon,
				AudioAsset = audioAsset
			};
			if (oldInfo != null)
			{
				UnloadAudio(oldInfo);
			}
			loadedAudio.Add(audioAsset.ToAssetReference(), value2);
			audioLoadingUpdate?.Invoke(loadedAudio.Count, audioToPreload.Count);
		}
		async Task LoadTexture(int num)
		{
			Texture2D texture2D2 = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, bulkResult.Assets[num].IconFileInstanceId, "png");
			iconTextures[num] = texture2D2;
		}
	}

	public bool TryGetRuntimeAudioInfo(SerializableGuid assetId, out RuntimeAudioInfo metadata)
	{
		AssetReference assetReference = loadedAudio.Keys.FirstOrDefault((AssetReference reference) => (SerializableGuid)reference.AssetID == assetId);
		if (assetReference == null)
		{
			metadata = null;
			return false;
		}
		return loadedAudio.TryGetValue(assetReference, out metadata);
	}

	public bool TryGetRuntimeAudioInfo(AssetReference assetReference, out RuntimeAudioInfo metadata)
	{
		return loadedAudio.TryGetValue(assetReference, out metadata);
	}

	public void UnloadAll()
	{
		AssetReference[] array = loadedAudio.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			UnloadAudio(loadedAudio[array[i]]);
		}
	}

	public IReadOnlyList<AudioReference> GetLoadedAudioAsAudioReferences()
	{
		AudioReference[] array = new AudioReference[loadedAudio.Count];
		int num = 0;
		foreach (KeyValuePair<AssetReference, RuntimeAudioInfo> item in loadedAudio)
		{
			AudioReference audioReference = ReferenceFactory.CreateAudioReference(item.Value.AudioAsset.AssetID);
			array[num] = audioReference;
			num++;
		}
		return array;
	}
}
