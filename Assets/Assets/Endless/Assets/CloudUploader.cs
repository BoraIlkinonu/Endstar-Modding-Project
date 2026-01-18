using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Endless.Assets
{
	// Token: 0x0200000C RID: 12
	public static class CloudUploader
	{
		// Token: 0x0600001F RID: 31 RVA: 0x00002390 File Offset: 0x00000590
		public static async void UploadFileBytes(EndlessCloudService endlessCloudService, FileUploadData fileUploadData, string folder = "endstar", Action<int> successCallback = null, Action<Exception> failureCallback = null)
		{
			FileUploadResult fileUploadResult = await CloudUploader.UploadFileBytesAsync(endlessCloudService, fileUploadData, folder);
			if (fileUploadResult.HasErrors)
			{
				if (failureCallback != null)
				{
					failureCallback(fileUploadResult.Exception);
				}
			}
			else if (successCallback != null)
			{
				successCallback(fileUploadResult.FileInstanceId);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x000023E8 File Offset: 0x000005E8
		public static async Task<FileUploadResult> UploadFileBytesAsync(EndlessCloudService endlessCloudService, FileUploadData fileUploadData, string folder = "endstar")
		{
			FileUploadResult fileUploadResult;
			try
			{
				GraphQlResult graphQlResult = await endlessCloudService.CreateFileUploadLinkAsync(fileUploadData.Filename, fileUploadData.MimeType, folder, fileUploadData.Bytes.Length, false);
				if (graphQlResult.HasErrors)
				{
					fileUploadResult = new FileUploadResult
					{
						Exception = graphQlResult.GetErrorMessage(0)
					};
				}
				else
				{
					CreateFileUploadLinkResult createFileUploadLinkResult = JsonConvert.DeserializeObject<CreateFileUploadLinkResult>(graphQlResult.GetDataMember().ToString());
					List<IMultipartFormSection> list = new List<IMultipartFormSection>();
					list.Add(new MultipartFormDataSection("X-Amz-Credential", createFileUploadLinkResult.AdditionalS3SecurityFields.XAmzCredential));
					list.Add(new MultipartFormDataSection("X-Amz-Signature", createFileUploadLinkResult.AdditionalS3SecurityFields.XAmzSignature));
					list.Add(new MultipartFormDataSection("X-Amz-Algorithm", createFileUploadLinkResult.AdditionalS3SecurityFields.XAmzAlgorithm));
					list.Add(new MultipartFormDataSection("X-Amz-Date", createFileUploadLinkResult.AdditionalS3SecurityFields.XAmzDate));
					list.Add(new MultipartFormDataSection("Policy", createFileUploadLinkResult.AdditionalS3SecurityFields.Policy));
					list.Add(new MultipartFormDataSection("key", createFileUploadLinkResult.AdditionalS3SecurityFields.Key));
					list.Add(new MultipartFormDataSection("bucket", createFileUploadLinkResult.AdditionalS3SecurityFields.Bucket));
					list.Add(new MultipartFormDataSection("Content-Type", createFileUploadLinkResult.AdditionalS3SecurityFields.ContentType));
					list.Add(new MultipartFormFileSection("file", fileUploadData.Bytes, fileUploadData.Filename, fileUploadData.MimeType));
					UnityWebRequest unityWebRequest = UnityWebRequest.Post(createFileUploadLinkResult.SecureUploadURL, list);
					await unityWebRequest.SendWithRetry(3);
					if (unityWebRequest.result != UnityWebRequest.Result.Success)
					{
						fileUploadResult = new FileUploadResult
						{
							Exception = new Exception(unityWebRequest.error)
						};
					}
					else
					{
						GraphQlResult graphQlResult2 = await endlessCloudService.MarkFileInstanceUploadedAsync(createFileUploadLinkResult.FileInstanceId, false);
						if (graphQlResult2.HasErrors)
						{
							fileUploadResult = new FileUploadResult
							{
								Exception = graphQlResult2.GetErrorMessage(0)
							};
						}
						else
						{
							fileUploadResult = new FileUploadResult
							{
								FileInstanceId = createFileUploadLinkResult.FileInstanceId
							};
						}
					}
				}
			}
			catch (Exception ex)
			{
				fileUploadResult = new FileUploadResult
				{
					Exception = ex
				};
			}
			return fileUploadResult;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000243C File Offset: 0x0000063C
		public static async void BatchUploadFileBytes(EndlessCloudService endlessCloudService, FileUploadData[] fileUploadData, string folder = "endstar", Action<int[]> successCallback = null, Action<Exception[]> failureCallback = null)
		{
			FileUploadResult[] array = await CloudUploader.BatchUploadFileBytesAsync(endlessCloudService, fileUploadData, folder);
			if (array.Any((FileUploadResult result) => result.HasErrors))
			{
				if (failureCallback != null)
				{
					failureCallback((from result in array
						where result.HasErrors
						select result.Exception).ToArray<Exception>());
				}
			}
			else if (successCallback != null)
			{
				successCallback(array.Select((FileUploadResult result) => result.FileInstanceId).ToArray<int>());
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002494 File Offset: 0x00000694
		public static async Task<FileUploadResult[]> BatchUploadFileBytesAsync(EndlessCloudService endlessCloudService, FileUploadData[] fileUploadData, string folder = "endstar")
		{
			return await Task.WhenAll<FileUploadResult>(fileUploadData.Select((FileUploadData upload) => CloudUploader.UploadFileBytesAsync(endlessCloudService, upload, folder)));
		}
	}
}
