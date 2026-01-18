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

namespace Endless.FileManagement
{
	// Token: 0x02000042 RID: 66
	public class LoadedFileManager : MonoBehaviourSingleton<LoadedFileManager>
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00005B06 File Offset: 0x00003D06
		private LocalFileDatabase FileDatabase
		{
			get
			{
				return MonoBehaviourSingleton<LocalFileDatabase>.Instance;
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005B10 File Offset: 0x00003D10
		public async void GetAssetBundle(global::UnityEngine.Object accessor, int fileInstanceId, Action<AssetBundle> onCompleted)
		{
			AssetBundle assetBundle = await this.GetAssetBundleAsync(accessor, fileInstanceId);
			onCompleted(assetBundle);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005B60 File Offset: 0x00003D60
		public async Task<AssetBundle> GetAssetBundleAsync(global::UnityEngine.Object accessor, int fileInstanceId)
		{
			AssetBundle assetBundle;
			if (this.loadedFiles.ContainsKey(fileInstanceId))
			{
				this.AddAccesssor(accessor, fileInstanceId);
				assetBundle = this.loadedFiles[fileInstanceId] as AssetBundle;
			}
			else if (this.pendingFileLoads.Contains(fileInstanceId))
			{
				this.AddAccesssor(accessor, fileInstanceId);
				while (this.pendingFileLoads.Contains(fileInstanceId))
				{
					await Task.Yield();
				}
				if (this.loadedFiles[fileInstanceId] == null)
				{
					this.ReleaseAccess(accessor, fileInstanceId);
				}
				assetBundle = this.loadedFiles[fileInstanceId] as AssetBundle;
			}
			else
			{
				this.pendingFileLoads.Add(fileInstanceId);
				string filePathForImmediateLoad = this.FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
				if (!string.IsNullOrEmpty(filePathForImmediateLoad))
				{
					try
					{
						AssetBundleCreateRequest assetBundleLoad = AssetBundle.LoadFromFileAsync(filePathForImmediateLoad);
						while (!assetBundleLoad.isDone)
						{
							await Task.Yield();
						}
						if (assetBundleLoad.assetBundle == null)
						{
							throw new LocalAssetBundleLoadException(string.Format("Filepath existed for file instance id ({0}) but AssetBundle was unable to load from file.", fileInstanceId), null);
						}
						this.loadedFiles.Add(fileInstanceId, assetBundleLoad.assetBundle);
						this.AddAccesssor(accessor, fileInstanceId);
						this.pendingFileLoads.Remove(fileInstanceId);
						return this.loadedFiles[fileInstanceId] as AssetBundle;
					}
					catch (LocalAssetBundleLoadException)
					{
						throw;
					}
					catch (Exception ex)
					{
						throw new LocalAssetBundleLoadException(string.Format("Filepath existed for file instance id ({0}) but AssetBundle was unable to load from file.", fileInstanceId), ex);
					}
				}
				try
				{
					byte[] array = await LoadedFileManager.DownloadBytes(fileInstanceId);
					this.FileDatabase.CreateNewFileEntry(fileInstanceId, ".assetBundle", array);
					AssetBundleCreateRequest assetBundleLoad = AssetBundle.LoadFromMemoryAsync(array);
					while (!assetBundleLoad.isDone)
					{
						await Task.Yield();
					}
					this.loadedFiles.Add(fileInstanceId, assetBundleLoad.assetBundle);
					this.AddAccesssor(accessor, fileInstanceId);
					this.pendingFileLoads.Remove(fileInstanceId);
					assetBundle = this.loadedFiles[fileInstanceId] as AssetBundle;
				}
				catch
				{
					this.pendingFileLoads.Remove(fileInstanceId);
					throw;
				}
			}
			return assetBundle;
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005BB4 File Offset: 0x00003DB4
		public async Task<bool> EnsureAssetBundleExists(int fileInstanceId)
		{
			bool flag;
			if (this.loadedFiles.ContainsKey(fileInstanceId))
			{
				flag = true;
			}
			else if (this.pendingFileLoads.Contains(fileInstanceId))
			{
				while (this.pendingFileLoads.Contains(fileInstanceId))
				{
					await Task.Yield();
				}
				if (!string.IsNullOrEmpty(this.FileDatabase.GetFilePathForImmediateLoad(fileInstanceId)))
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				this.pendingFileLoads.Add(fileInstanceId);
				if (!string.IsNullOrEmpty(this.FileDatabase.GetFilePathForImmediateLoad(fileInstanceId)))
				{
					flag = true;
				}
				else
				{
					try
					{
						byte[] array = await LoadedFileManager.DownloadBytes(fileInstanceId);
						this.FileDatabase.CreateNewFileEntry(fileInstanceId, ".assetBundle", array);
						this.pendingFileLoads.Remove(fileInstanceId);
						flag = true;
					}
					catch
					{
						this.pendingFileLoads.Remove(fileInstanceId);
						flag = false;
					}
				}
			}
			return flag;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005C00 File Offset: 0x00003E00
		public async void GetTexture2D(global::UnityEngine.Object accessor, int fileInstanceId, string fileExtension, Action<int, Texture2D> onCompleted)
		{
			Texture2D texture2D = await this.GetTexture2DAsync(accessor, fileInstanceId, fileExtension);
			onCompleted(fileInstanceId, texture2D);
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005C58 File Offset: 0x00003E58
		public async Task<Texture2D> GetTexture2DAsync(global::UnityEngine.Object accessor, int fileInstanceId, string fileExtension)
		{
			return await this.GetWebFileAsync<Texture2D>(accessor, fileInstanceId, fileExtension, new Func<string, UnityWebRequest>(UnityWebRequestTexture.GetTexture), new Func<UnityWebRequest, Texture2D>(DownloadHandlerTexture.GetContent));
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00005CB4 File Offset: 0x00003EB4
		private static async Task<byte[]> DownloadBytes(int fileInstanceId)
		{
			UploadFileInstance fileInstance;
			try
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetFileInstanceByIdAsync(fileInstanceId, false);
				if (graphQlResult.HasErrors)
				{
					throw graphQlResult.GetErrorMessage(0);
				}
				fileInstance = JsonConvert.DeserializeObject<UploadFileInstance>(graphQlResult.GetDataMember().ToString());
			}
			catch (Exception ex)
			{
				throw new DownloadBytesException(string.Format("Failed to get file id {0}!", fileInstanceId), ex);
			}
			byte[] data;
			try
			{
				UnityWebRequest retrievedFileRequest = UnityWebRequest.Get(fileInstance.FileUrl);
				await retrievedFileRequest.SendWithRetry(3);
				if (retrievedFileRequest.result != UnityWebRequest.Result.Success)
				{
					throw new DownloadBytesException(string.Format("Failed to download bytes for file id {0}\nURL: {1}\n{2}", fileInstanceId, fileInstance.FileUrl, retrievedFileRequest.error), null);
				}
				data = retrievedFileRequest.downloadHandler.data;
			}
			catch (Exception ex2)
			{
				throw new DownloadBytesException(string.Format("Failed to download bytes for file id {0}\nURL: {1}", fileInstanceId, fileInstance.FileUrl), ex2);
			}
			return data;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00005CF8 File Offset: 0x00003EF8
		public async Task<T> GetWebFileAsync<T>(global::UnityEngine.Object accessor, int fileInstanceId, string fileExtension, Func<string, UnityWebRequest> creationFunc, Func<UnityWebRequest, T> resolverFunc) where T : global::UnityEngine.Object
		{
			T t;
			if (this.loadedFiles.ContainsKey(fileInstanceId))
			{
				this.AddAccesssor(accessor, fileInstanceId);
				t = this.loadedFiles[fileInstanceId] as T;
			}
			else if (this.pendingFileLoads.Contains(fileInstanceId))
			{
				this.AddAccesssor(accessor, fileInstanceId);
				while (this.pendingFileLoads.Contains(fileInstanceId))
				{
					await Task.Yield();
				}
				global::UnityEngine.Object @object = this.loadedFiles[fileInstanceId];
				if (@object == null)
				{
					this.ReleaseAccess(accessor, fileInstanceId);
				}
				t = @object as T;
			}
			else
			{
				this.pendingFileLoads.Add(fileInstanceId);
				string text = this.FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
				if (!string.IsNullOrEmpty(text))
				{
					text = "file://" + text;
					using (UnityWebRequest request = creationFunc(text))
					{
						try
						{
							await request.SendWithRetry(3);
							if (request.result == UnityWebRequest.Result.Success)
							{
								T t2 = resolverFunc(request);
								this.loadedFiles.Add(fileInstanceId, t2);
								this.AddAccesssor(accessor, fileInstanceId);
								this.pendingFileLoads.Remove(fileInstanceId);
								return t2;
							}
							Debug.LogException(new HttpRequestException(request.error ?? ""));
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
							this.pendingFileLoads.Remove(fileInstanceId);
						}
					}
					UnityWebRequest request = null;
				}
				else
				{
					try
					{
						byte[] array = await LoadedFileManager.DownloadBytes(fileInstanceId);
						this.FileDatabase.CreateNewFileEntry(fileInstanceId, fileExtension, array);
						this.pendingFileLoads.Remove(fileInstanceId);
						return await this.GetWebFileAsync<T>(accessor, fileInstanceId, fileExtension, creationFunc, resolverFunc);
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						this.pendingFileLoads.Remove(fileInstanceId);
					}
				}
				t = default(T);
			}
			return t;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00005D68 File Offset: 0x00003F68
		public async Task<AudioClip> GetAudioFileAsync(global::UnityEngine.Object accessor, int fileInstanceId, AudioType audioType)
		{
			string text;
			if (audioType == AudioType.MPEG)
			{
				text = ".mp3";
			}
			else
			{
				text = "." + audioType.ToString();
			}
			return await this.GetWebFileAsync<AudioClip>(accessor, fileInstanceId, text, (string filePath) => UnityWebRequestMultimedia.GetAudioClip(filePath, audioType), new Func<UnityWebRequest, AudioClip>(DownloadHandlerAudioClip.GetContent));
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00005DC4 File Offset: 0x00003FC4
		public async Task EnsureAudioClipIsDownloaded(int fileInstanceId, AudioType audioType)
		{
			if (!this.loadedFiles.ContainsKey(fileInstanceId))
			{
				string filePath = this.FileDatabase.GetFilePathForImmediateLoad(fileInstanceId);
				if (string.IsNullOrEmpty(filePath))
				{
					if (this.pendingFileLoads.Contains(fileInstanceId))
					{
						while (this.pendingFileLoads.Contains(fileInstanceId))
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
						this.pendingFileLoads.Add(fileInstanceId);
					}
					try
					{
						byte[] array = await LoadedFileManager.DownloadBytes(fileInstanceId);
						this.FileDatabase.CreateNewFileEntry(fileInstanceId, ".wav", array);
						this.pendingFileLoads.Remove(fileInstanceId);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						this.pendingFileLoads.Remove(fileInstanceId);
					}
				}
			}
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00005E10 File Offset: 0x00004010
		private void AddAccesssor(global::UnityEngine.Object accessor, int fileInstanceId)
		{
			if (!this.accessorsToFileInstances.ContainsKey(accessor))
			{
				this.accessorsToFileInstances.Add(accessor, new HashSet<int> { fileInstanceId });
			}
			else
			{
				this.accessorsToFileInstances[accessor].Add(fileInstanceId);
			}
			if (!this.fileAccessors.ContainsKey(fileInstanceId))
			{
				this.fileAccessors.Add(fileInstanceId, new HashSet<global::UnityEngine.Object> { accessor });
				return;
			}
			this.fileAccessors[fileInstanceId].Add(accessor);
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00005E94 File Offset: 0x00004094
		public void ReleaseAccess(global::UnityEngine.Object accessor, int fileInstanceId)
		{
			if (this.fileAccessors.ContainsKey(fileInstanceId))
			{
				this.fileAccessors[fileInstanceId].Remove(accessor);
				if (this.fileAccessors[fileInstanceId].Count == 0)
				{
					global::UnityEngine.Object @object;
					if (this.loadedFiles.TryGetValue(fileInstanceId, out @object))
					{
						AssetBundle assetBundle = @object as AssetBundle;
						if (assetBundle != null && assetBundle != null)
						{
							assetBundle.Unload(false);
						}
					}
					this.loadedFiles.Remove(fileInstanceId);
					this.fileAccessors.Remove(fileInstanceId);
				}
			}
			if (this.accessorsToFileInstances.ContainsKey(accessor))
			{
				this.accessorsToFileInstances[accessor].Remove(fileInstanceId);
				if (this.accessorsToFileInstances[accessor].Count == 0)
				{
					this.accessorsToFileInstances.Remove(accessor);
				}
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00005F5C File Offset: 0x0000415C
		public void CleanupAccess()
		{
			int[] array = this.fileAccessors.Keys.ToArray<int>();
			for (int i = array.Length - 1; i >= 0; i--)
			{
				if (this.fileAccessors[array[i]].Count == 0)
				{
					this.loadedFiles.Remove(array[i]);
					this.fileAccessors.Remove(array[i]);
				}
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00005FC0 File Offset: 0x000041C0
		public void ReleaseAllAccesses(global::UnityEngine.Object accessor)
		{
			if (!this.accessorsToFileInstances.ContainsKey(accessor))
			{
				return;
			}
			foreach (int num in new HashSet<int>(this.accessorsToFileInstances[accessor]))
			{
				this.ReleaseAccess(accessor, num);
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00006030 File Offset: 0x00004230
		private void Update()
		{
			if (EndlessInput.GetKeyDown(Key.F4))
			{
				Debug.Log("--Dumping loaded file accessors");
				foreach (KeyValuePair<global::UnityEngine.Object, HashSet<int>> keyValuePair in this.accessorsToFileInstances)
				{
					Dictionary<Type, int> typeCount = new Dictionary<Type, int>();
					string text = (keyValuePair.Key ? keyValuePair.Key.name : "NULL");
					foreach (int num in keyValuePair.Value)
					{
						Type type = this.loadedFiles[num].GetType();
						if (!typeCount.TryAdd(type, 1))
						{
							Dictionary<Type, int> typeCount2 = typeCount;
							Type type2 = type;
							int num2 = typeCount2[type2];
							typeCount2[type2] = num2 + 1;
						}
					}
					string[] array = typeCount.Keys.Select((Type key) => string.Format("{0} ({1})", key.Name, typeCount[key])).ToArray<string>();
					string text2 = string.Join(",", array);
					Debug.Log(string.Format("Accessor: {0} has {1} files accessed: {2}", text, keyValuePair.Value.Count, text2), keyValuePair.Key);
				}
			}
		}

		// Token: 0x040000BB RID: 187
		private Dictionary<int, global::UnityEngine.Object> loadedFiles = new Dictionary<int, global::UnityEngine.Object>();

		// Token: 0x040000BC RID: 188
		private Dictionary<global::UnityEngine.Object, HashSet<int>> accessorsToFileInstances = new Dictionary<global::UnityEngine.Object, HashSet<int>>();

		// Token: 0x040000BD RID: 189
		private Dictionary<int, HashSet<global::UnityEngine.Object>> fileAccessors = new Dictionary<int, HashSet<global::UnityEngine.Object>>();

		// Token: 0x040000BE RID: 190
		private AmazonS3Client client;

		// Token: 0x040000BF RID: 191
		private string bucketName;

		// Token: 0x040000C0 RID: 192
		private List<int> pendingFileLoads = new List<int>();
	}
}
