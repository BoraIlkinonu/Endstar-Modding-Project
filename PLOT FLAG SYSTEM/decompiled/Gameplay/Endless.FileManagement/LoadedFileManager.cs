using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.S3;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.FileManagement;
using Endless.UnityExtensions;
using Newtonsoft.Json;
using Runtime.Assets;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

namespace Endless.FileManagement;

public class LoadedFileManager : MonoBehaviourSingleton<LoadedFileManager>
{
	private Dictionary<int, UnityEngine.Object> loadedFiles = new Dictionary<int, UnityEngine.Object>();

	private Dictionary<UnityEngine.Object, HashSet<int>> accessorsToFileInstances = new Dictionary<UnityEngine.Object, HashSet<int>>();

	private Dictionary<int, HashSet<UnityEngine.Object>> fileAccessors = new Dictionary<int, HashSet<UnityEngine.Object>>();

	private AmazonS3Client client;

	private string bucketName;

	private List<int> pendingFileLoads = new List<int>();

	private LocalFileDatabase FileDatabase => MonoBehaviourSingleton<LocalFileDatabase>.Instance;

	public async void GetAssetBundle(UnityEngine.Object accessor, int fileInstanceId, Action<AssetBundle> onCompleted)
	{
		onCompleted(await GetAssetBundleAsync(accessor, fileInstanceId));
	}

	public async Task<AssetBundle> GetAssetBundleAsync(UnityEngine.Object accessor, int fileInstanceId)
	{
		if (loadedFiles.ContainsKey(fileInstanceId))
		{
			AddAccesssor(accessor, fileInstanceId);
			return loadedFiles[fileInstanceId] as AssetBundle;
		}
		if (pendingFileLoads.Contains(fileInstanceId))
		{
			AddAccesssor(accessor, fileInstanceId);
			while (pendingFileLoads.Contains(fileInstanceId))
			{
				await Task.Yield();
			}
			if (loadedFiles[fileInstanceId] == null)
			{
				ReleaseAccess(accessor, fileInstanceId);
			}
			return loadedFiles[fileInstanceId] as AssetBundle;
		}
		pendingFileLoads.Add(fileInstanceId);
		string filePathForImmediateLoad = FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
		if (!string.IsNullOrEmpty(filePathForImmediateLoad))
		{
			try
			{
				AssetBundleCreateRequest assetBundleLoad = AssetBundle.LoadFromFileAsync(filePathForImmediateLoad);
				while (!assetBundleLoad.isDone)
				{
					await Task.Yield();
				}
				if ((object)assetBundleLoad.assetBundle == null)
				{
					throw new LocalAssetBundleLoadException($"Filepath existed for file instance id ({fileInstanceId}) but AssetBundle was unable to load from file.");
				}
				loadedFiles.Add(fileInstanceId, assetBundleLoad.assetBundle);
				AddAccesssor(accessor, fileInstanceId);
				pendingFileLoads.Remove(fileInstanceId);
				return loadedFiles[fileInstanceId] as AssetBundle;
			}
			catch (LocalAssetBundleLoadException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new LocalAssetBundleLoadException($"Filepath existed for file instance id ({fileInstanceId}) but AssetBundle was unable to load from file.", innerException);
			}
		}
		try
		{
			byte[] array = await DownloadBytes(fileInstanceId);
			FileDatabase.CreateNewFileEntry(fileInstanceId, ".assetBundle", array);
			AssetBundleCreateRequest assetBundleLoad = AssetBundle.LoadFromMemoryAsync(array);
			while (!assetBundleLoad.isDone)
			{
				await Task.Yield();
			}
			loadedFiles.Add(fileInstanceId, assetBundleLoad.assetBundle);
			AddAccesssor(accessor, fileInstanceId);
			pendingFileLoads.Remove(fileInstanceId);
			return loadedFiles[fileInstanceId] as AssetBundle;
		}
		catch
		{
			pendingFileLoads.Remove(fileInstanceId);
			throw;
		}
	}

	public async Task<bool> EnsureAssetBundleExists(int fileInstanceId)
	{
		if (loadedFiles.ContainsKey(fileInstanceId))
		{
			return true;
		}
		if (pendingFileLoads.Contains(fileInstanceId))
		{
			while (pendingFileLoads.Contains(fileInstanceId))
			{
				await Task.Yield();
			}
			if (!string.IsNullOrEmpty(FileDatabase.GetFilePathForImmediateLoad(fileInstanceId)))
			{
				return false;
			}
			return true;
		}
		pendingFileLoads.Add(fileInstanceId);
		if (!string.IsNullOrEmpty(FileDatabase.GetFilePathForImmediateLoad(fileInstanceId)))
		{
			return true;
		}
		try
		{
			byte[] bytes = await DownloadBytes(fileInstanceId);
			FileDatabase.CreateNewFileEntry(fileInstanceId, ".assetBundle", bytes);
			pendingFileLoads.Remove(fileInstanceId);
			return true;
		}
		catch
		{
			pendingFileLoads.Remove(fileInstanceId);
			return false;
		}
	}

	public async void GetTexture2D(UnityEngine.Object accessor, int fileInstanceId, string fileExtension, Action<int, Texture2D> onCompleted)
	{
		onCompleted(fileInstanceId, await GetTexture2DAsync(accessor, fileInstanceId, fileExtension));
	}

	public async Task<Texture2D> GetTexture2DAsync(UnityEngine.Object accessor, int fileInstanceId, string fileExtension)
	{
		return await GetWebFileAsync(accessor, fileInstanceId, fileExtension, UnityWebRequestTexture.GetTexture, DownloadHandlerTexture.GetContent);
	}

	private static async Task<byte[]> DownloadBytes(int fileInstanceId)
	{
		UploadFileInstance fileInstance;
		try
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetFileInstanceByIdAsync(fileInstanceId);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage();
			}
			fileInstance = JsonConvert.DeserializeObject<UploadFileInstance>(graphQlResult.GetDataMember().ToString());
		}
		catch (Exception innerException)
		{
			throw new DownloadBytesException($"Failed to get file id {fileInstanceId}!", innerException);
		}
		try
		{
			UnityWebRequest retrievedFileRequest = UnityWebRequest.Get(fileInstance.FileUrl);
			await retrievedFileRequest.SendWithRetry();
			if (retrievedFileRequest.result != UnityWebRequest.Result.Success)
			{
				throw new DownloadBytesException($"Failed to download bytes for file id {fileInstanceId}\nURL: {fileInstance.FileUrl}\n{retrievedFileRequest.error}");
			}
			return retrievedFileRequest.downloadHandler.data;
		}
		catch (Exception innerException2)
		{
			throw new DownloadBytesException($"Failed to download bytes for file id {fileInstanceId}\nURL: {fileInstance.FileUrl}", innerException2);
		}
	}

	public async Task<T> GetWebFileAsync<T>(UnityEngine.Object accessor, int fileInstanceId, string fileExtension, Func<string, UnityWebRequest> creationFunc, Func<UnityWebRequest, T> resolverFunc) where T : UnityEngine.Object
	{
		if (loadedFiles.ContainsKey(fileInstanceId))
		{
			AddAccesssor(accessor, fileInstanceId);
			return loadedFiles[fileInstanceId] as T;
		}
		if (pendingFileLoads.Contains(fileInstanceId))
		{
			AddAccesssor(accessor, fileInstanceId);
			while (pendingFileLoads.Contains(fileInstanceId))
			{
				await Task.Yield();
			}
			UnityEngine.Object obj = loadedFiles[fileInstanceId];
			if (obj == null)
			{
				ReleaseAccess(accessor, fileInstanceId);
			}
			return obj as T;
		}
		pendingFileLoads.Add(fileInstanceId);
		string filePathForImmediateLoad = FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
		if (!string.IsNullOrEmpty(filePathForImmediateLoad))
		{
			filePathForImmediateLoad = "file://" + filePathForImmediateLoad;
			using UnityWebRequest request = creationFunc(filePathForImmediateLoad);
			_ = 1;
			try
			{
				await request.SendWithRetry();
				if (request.result == UnityWebRequest.Result.Success)
				{
					T val = resolverFunc(request);
					loadedFiles.Add(fileInstanceId, val);
					AddAccesssor(accessor, fileInstanceId);
					pendingFileLoads.Remove(fileInstanceId);
					return val;
				}
				Debug.LogException(new HttpRequestException(request.error ?? ""));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				pendingFileLoads.Remove(fileInstanceId);
			}
		}
		else
		{
			try
			{
				byte[] bytes = await DownloadBytes(fileInstanceId);
				FileDatabase.CreateNewFileEntry(fileInstanceId, fileExtension, bytes);
				pendingFileLoads.Remove(fileInstanceId);
				return await GetWebFileAsync(accessor, fileInstanceId, fileExtension, creationFunc, resolverFunc);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
				pendingFileLoads.Remove(fileInstanceId);
			}
		}
		return null;
	}

	public async Task<AudioClip> GetAudioFileAsync(UnityEngine.Object accessor, int fileInstanceId, AudioType audioType)
	{
		string fileExtension = ((audioType != AudioType.MPEG) ? ("." + audioType) : ".mp3");
		return await GetWebFileAsync(accessor, fileInstanceId, fileExtension, (string filePath) => UnityWebRequestMultimedia.GetAudioClip(filePath, audioType), DownloadHandlerAudioClip.GetContent);
	}

	public async Task EnsureAudioClipIsDownloaded(int fileInstanceId, AudioType audioType)
	{
		if (loadedFiles.ContainsKey(fileInstanceId))
		{
			return;
		}
		string filePath = FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
		if (!string.IsNullOrEmpty(filePath))
		{
			return;
		}
		if (pendingFileLoads.Contains(fileInstanceId))
		{
			while (pendingFileLoads.Contains(fileInstanceId))
			{
				await Task.Yield();
			}
			if (!string.IsNullOrEmpty(filePath))
			{
				return;
			}
		}
		else
		{
			pendingFileLoads.Add(fileInstanceId);
		}
		try
		{
			byte[] bytes = await DownloadBytes(fileInstanceId);
			FileDatabase.CreateNewFileEntry(fileInstanceId, ".wav", bytes);
			pendingFileLoads.Remove(fileInstanceId);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			pendingFileLoads.Remove(fileInstanceId);
		}
	}

	private void AddAccesssor(UnityEngine.Object accessor, int fileInstanceId)
	{
		if (!accessorsToFileInstances.ContainsKey(accessor))
		{
			accessorsToFileInstances.Add(accessor, new HashSet<int> { fileInstanceId });
		}
		else
		{
			accessorsToFileInstances[accessor].Add(fileInstanceId);
		}
		if (!fileAccessors.ContainsKey(fileInstanceId))
		{
			fileAccessors.Add(fileInstanceId, new HashSet<UnityEngine.Object> { accessor });
		}
		else
		{
			fileAccessors[fileInstanceId].Add(accessor);
		}
	}

	public void ReleaseAccess(UnityEngine.Object accessor, int fileInstanceId)
	{
		if (fileAccessors.ContainsKey(fileInstanceId))
		{
			fileAccessors[fileInstanceId].Remove(accessor);
			if (fileAccessors[fileInstanceId].Count == 0)
			{
				if (loadedFiles.TryGetValue(fileInstanceId, out var value) && value is AssetBundle assetBundle && assetBundle != null)
				{
					assetBundle.Unload(unloadAllLoadedObjects: false);
				}
				loadedFiles.Remove(fileInstanceId);
				fileAccessors.Remove(fileInstanceId);
			}
		}
		if (accessorsToFileInstances.ContainsKey(accessor))
		{
			accessorsToFileInstances[accessor].Remove(fileInstanceId);
			if (accessorsToFileInstances[accessor].Count == 0)
			{
				accessorsToFileInstances.Remove(accessor);
			}
		}
	}

	public void CleanupAccess()
	{
		int[] array = fileAccessors.Keys.ToArray();
		for (int num = array.Length - 1; num >= 0; num--)
		{
			if (fileAccessors[array[num]].Count == 0)
			{
				loadedFiles.Remove(array[num]);
				fileAccessors.Remove(array[num]);
			}
		}
	}

	public void ReleaseAllAccesses(UnityEngine.Object accessor)
	{
		if (!accessorsToFileInstances.ContainsKey(accessor))
		{
			return;
		}
		foreach (int item in new HashSet<int>(accessorsToFileInstances[accessor]))
		{
			ReleaseAccess(accessor, item);
		}
	}

	private void Update()
	{
		if (!EndlessInput.GetKeyDown(Key.F4))
		{
			return;
		}
		Debug.Log("--Dumping loaded file accessors");
		foreach (KeyValuePair<UnityEngine.Object, HashSet<int>> accessorsToFileInstance in accessorsToFileInstances)
		{
			Dictionary<Type, int> typeCount = new Dictionary<Type, int>();
			string arg = (accessorsToFileInstance.Key ? accessorsToFileInstance.Key.name : "NULL");
			foreach (int item in accessorsToFileInstance.Value)
			{
				Type type = loadedFiles[item].GetType();
				if (!typeCount.TryAdd(type, 1))
				{
					typeCount[type]++;
				}
			}
			string[] value = typeCount.Keys.Select((Type key) => $"{key.Name} ({typeCount[key]})").ToArray();
			string arg2 = string.Join(",", value);
			Debug.Log($"Accessor: {arg} has {accessorsToFileInstance.Value.Count} files accessed: {arg2}", accessorsToFileInstance.Key);
		}
	}
}
