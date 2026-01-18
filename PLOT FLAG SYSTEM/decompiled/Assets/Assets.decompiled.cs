using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyVersion("0.0.0.0")]
[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	private struct MonoScriptData
	{
		public byte[] FilePathsData;

		public byte[] TypesData;

		public int TotalTypes;

		public int TotalFiles;

		public bool IsEditorOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MonoScriptData Get()
	{
		return new MonoScriptData
		{
			FilePathsData = new byte[682]
			{
				0, 0, 0, 2, 0, 0, 0, 39, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 82, 117, 110, 116, 105, 109,
				101, 92, 65, 115, 115, 101, 116, 115, 92, 65,
				115, 115, 101, 116, 46, 99, 115, 0, 0, 0,
				1, 0, 0, 0, 43, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 82, 117, 110, 116, 105, 109, 101, 92, 65,
				115, 115, 101, 116, 115, 92, 65, 115, 115, 101,
				116, 76, 105, 115, 116, 46, 99, 115, 0, 0,
				0, 1, 0, 0, 0, 48, 92, 65, 115, 115,
				101, 116, 115, 92, 83, 99, 114, 105, 112, 116,
				115, 92, 82, 117, 110, 116, 105, 109, 101, 92,
				65, 115, 115, 101, 116, 115, 92, 65, 115, 115,
				101, 116, 82, 101, 102, 101, 114, 101, 110, 99,
				101, 46, 99, 115, 0, 0, 0, 1, 0, 0,
				0, 44, 92, 65, 115, 115, 101, 116, 115, 92,
				83, 99, 114, 105, 112, 116, 115, 92, 82, 117,
				110, 116, 105, 109, 101, 92, 65, 115, 115, 101,
				116, 115, 92, 67, 104, 97, 110, 103, 101, 84,
				121, 112, 101, 46, 99, 115, 0, 0, 0, 3,
				0, 0, 0, 47, 92, 65, 115, 115, 101, 116,
				115, 92, 83, 99, 114, 105, 112, 116, 115, 92,
				82, 117, 110, 116, 105, 109, 101, 92, 65, 115,
				115, 101, 116, 115, 92, 67, 108, 111, 117, 100,
				85, 112, 108, 111, 97, 100, 101, 114, 46, 99,
				115, 0, 0, 0, 3, 0, 0, 0, 60, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 82, 117, 110, 116, 105,
				109, 101, 92, 65, 115, 115, 101, 116, 115, 92,
				67, 114, 101, 97, 116, 101, 70, 105, 108, 101,
				85, 112, 108, 111, 97, 100, 76, 105, 110, 107,
				82, 101, 115, 117, 108, 116, 46, 99, 115, 0,
				0, 0, 1, 0, 0, 0, 50, 92, 65, 115,
				115, 101, 116, 115, 92, 83, 99, 114, 105, 112,
				116, 115, 92, 82, 117, 110, 116, 105, 109, 101,
				92, 65, 115, 115, 101, 116, 115, 92, 67, 117,
				114, 97, 116, 101, 100, 71, 97, 109, 101, 115,
				76, 105, 115, 116, 46, 99, 115, 0, 0, 0,
				2, 0, 0, 0, 51, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 82, 117, 110, 116, 105, 109, 101, 92, 65,
				115, 115, 101, 116, 115, 92, 70, 105, 108, 101,
				65, 115, 115, 101, 116, 73, 110, 115, 116, 97,
				110, 99, 101, 46, 99, 115, 0, 0, 0, 1,
				0, 0, 0, 50, 92, 65, 115, 115, 101, 116,
				115, 92, 83, 99, 114, 105, 112, 116, 115, 92,
				82, 117, 110, 116, 105, 109, 101, 92, 65, 115,
				115, 101, 116, 115, 92, 71, 97, 109, 101, 65,
				115, 115, 101, 116, 80, 114, 101, 118, 105, 101,
				119, 46, 99, 115, 0, 0, 0, 1, 0, 0,
				0, 52, 92, 65, 115, 115, 101, 116, 115, 92,
				83, 99, 114, 105, 112, 116, 115, 92, 82, 117,
				110, 116, 105, 109, 101, 92, 65, 115, 115, 101,
				116, 115, 92, 76, 105, 98, 114, 97, 114, 121,
				67, 111, 110, 116, 101, 110, 116, 80, 97, 99,
				107, 46, 99, 115, 0, 0, 0, 2, 0, 0,
				0, 50, 92, 65, 115, 115, 101, 116, 115, 92,
				83, 99, 114, 105, 112, 116, 115, 92, 82, 117,
				110, 116, 105, 109, 101, 92, 65, 115, 115, 101,
				116, 115, 92, 82, 101, 118, 105, 115, 105, 111,
				110, 77, 101, 116, 97, 68, 97, 116, 97, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 52,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 82, 117, 110, 116,
				105, 109, 101, 92, 65, 115, 115, 101, 116, 115,
				92, 85, 112, 108, 111, 97, 100, 70, 105, 108,
				101, 73, 110, 115, 116, 97, 110, 99, 101, 46,
				99, 115
			},
			TypesData = new byte[694]
			{
				0, 0, 0, 0, 24, 69, 110, 100, 108, 101,
				115, 115, 46, 65, 115, 115, 101, 116, 115, 124,
				65, 115, 115, 101, 116, 67, 111, 114, 101, 0,
				0, 0, 0, 20, 69, 110, 100, 108, 101, 115,
				115, 46, 65, 115, 115, 101, 116, 115, 124, 65,
				115, 115, 101, 116, 0, 0, 0, 0, 24, 69,
				110, 100, 108, 101, 115, 115, 46, 65, 115, 115,
				101, 116, 115, 124, 65, 115, 115, 101, 116, 76,
				105, 115, 116, 0, 0, 0, 0, 29, 69, 110,
				100, 108, 101, 115, 115, 46, 65, 115, 115, 101,
				116, 115, 124, 65, 115, 115, 101, 116, 82, 101,
				102, 101, 114, 101, 110, 99, 101, 0, 0, 0,
				0, 34, 69, 110, 100, 108, 101, 115, 115, 46,
				65, 115, 115, 101, 116, 115, 124, 67, 104, 97,
				110, 103, 101, 84, 121, 112, 101, 65, 116, 116,
				114, 105, 98, 117, 116, 101, 0, 0, 0, 0,
				31, 69, 110, 100, 108, 101, 115, 115, 46, 65,
				115, 115, 101, 116, 115, 124, 70, 105, 108, 101,
				85, 112, 108, 111, 97, 100, 82, 101, 115, 117,
				108, 116, 0, 0, 0, 0, 29, 69, 110, 100,
				108, 101, 115, 115, 46, 65, 115, 115, 101, 116,
				115, 124, 70, 105, 108, 101, 85, 112, 108, 111,
				97, 100, 68, 97, 116, 97, 0, 0, 0, 0,
				28, 69, 110, 100, 108, 101, 115, 115, 46, 65,
				115, 115, 101, 116, 115, 124, 67, 108, 111, 117,
				100, 85, 112, 108, 111, 97, 100, 101, 114, 0,
				0, 0, 0, 41, 69, 110, 100, 108, 101, 115,
				115, 46, 65, 115, 115, 101, 116, 115, 124, 65,
				100, 100, 105, 116, 105, 111, 110, 97, 108, 83,
				51, 83, 101, 99, 117, 114, 105, 116, 121, 70,
				105, 101, 108, 100, 115, 0, 0, 0, 0, 58,
				69, 110, 100, 108, 101, 115, 115, 46, 65, 115,
				115, 101, 116, 115, 124, 83, 116, 114, 105, 110,
				103, 84, 111, 65, 100, 100, 105, 116, 105, 111,
				110, 97, 108, 83, 51, 83, 101, 99, 117, 114,
				105, 116, 121, 70, 105, 101, 108, 100, 115, 67,
				111, 110, 118, 101, 114, 116, 101, 114, 0, 0,
				0, 0, 41, 69, 110, 100, 108, 101, 115, 115,
				46, 65, 115, 115, 101, 116, 115, 124, 67, 114,
				101, 97, 116, 101, 70, 105, 108, 101, 85, 112,
				108, 111, 97, 100, 76, 105, 110, 107, 82, 101,
				115, 117, 108, 116, 0, 0, 0, 0, 31, 69,
				110, 100, 108, 101, 115, 115, 46, 65, 115, 115,
				101, 116, 115, 124, 67, 117, 114, 97, 116, 101,
				100, 71, 97, 109, 101, 115, 76, 105, 115, 116,
				0, 0, 0, 0, 32, 69, 110, 100, 108, 101,
				115, 115, 46, 65, 115, 115, 101, 116, 115, 124,
				70, 105, 108, 101, 65, 115, 115, 101, 116, 73,
				110, 115, 116, 97, 110, 99, 101, 0, 0, 0,
				0, 24, 69, 110, 100, 108, 101, 115, 115, 46,
				65, 115, 115, 101, 116, 115, 124, 70, 105, 108,
				101, 65, 115, 115, 101, 116, 0, 0, 0, 0,
				31, 69, 110, 100, 108, 101, 115, 115, 46, 65,
				115, 115, 101, 116, 115, 124, 71, 97, 109, 101,
				65, 115, 115, 101, 116, 80, 114, 101, 118, 105,
				101, 119, 0, 0, 0, 0, 33, 69, 110, 100,
				108, 101, 115, 115, 46, 65, 115, 115, 101, 116,
				115, 124, 76, 105, 98, 114, 97, 114, 121, 67,
				111, 110, 116, 101, 110, 116, 80, 97, 99, 107,
				0, 0, 0, 0, 25, 69, 110, 100, 108, 101,
				115, 115, 46, 65, 115, 115, 101, 116, 115, 124,
				67, 104, 97, 110, 103, 101, 68, 97, 116, 97,
				0, 0, 0, 0, 31, 69, 110, 100, 108, 101,
				115, 115, 46, 65, 115, 115, 101, 116, 115, 124,
				82, 101, 118, 105, 115, 105, 111, 110, 77, 101,
				116, 97, 68, 97, 116, 97, 0, 0, 0, 0,
				33, 82, 117, 110, 116, 105, 109, 101, 46, 65,
				115, 115, 101, 116, 115, 124, 85, 112, 108, 111,
				97, 100, 70, 105, 108, 101, 73, 110, 115, 116,
				97, 110, 99, 101
			},
			TotalFiles = 12,
			TotalTypes = 19,
			IsEditorOnly = false
		};
	}
}
namespace Runtime.Assets
{
	[Serializable]
	public class UploadFileInstance
	{
		[JsonProperty("name")]
		public string Name { get; private set; }

		[JsonProperty("mime_type")]
		public string MimeType { get; private set; }

		[JsonProperty("file_url")]
		public string FileUrl { get; private set; }

		[JsonProperty("file_id")]
		public int FileId { get; private set; }
	}
}
namespace Endless.Assets
{
	[Serializable]
	public class AssetCore
	{
		[JsonProperty]
		public string Name;

		[JsonProperty("asset_id")]
		public string AssetID;

		[JsonProperty("asset_version")]
		public string AssetVersion;

		[JsonProperty("asset_type")]
		public string AssetType = "Unknown";

		public virtual AssetReference ToAssetReference()
		{
			return new AssetReference
			{
				AssetType = AssetType,
				AssetVersion = AssetVersion,
				AssetID = AssetID,
				UpdateParentVersion = false
			};
		}

		public override string ToString()
		{
			return "{ Name: " + Name + ", AssetID: " + AssetID + ", AssetVersion: " + AssetVersion + ", AssetType: " + AssetType + " }";
		}
	}
	[Serializable]
	public class Asset : AssetCore
	{
		[JsonProperty]
		public string Description;

		[JsonProperty("internal_version")]
		[HideInInspector]
		public string InternalVersion = "0.0.0";

		[JsonProperty("revision_meta_data")]
		public RevisionMetaData RevisionMetaData = new RevisionMetaData();

		public virtual object GetAnonymousObjectForUpload()
		{
			Asset obj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this), GetType()) as Asset;
			obj.AssetVersion = "";
			return obj;
		}

		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", "Description", Description, "InternalVersion", InternalVersion, "RevisionMetaData", RevisionMetaData, "AssetCore", base.ToString());
		}
	}
	public abstract class AssetList : Asset
	{
		[SerializeField]
		[JsonProperty]
		protected List<AssetReference> assets = new List<AssetReference>();

		[SerializeField]
		[JsonProperty]
		protected int iconFileInstanceId;

		[JsonIgnore]
		public IReadOnlyList<AssetReference> Assets => assets;
	}
	[Serializable]
	public class AssetReference
	{
		[JsonProperty("asset_ref_id")]
		public string AssetID;

		[JsonProperty("asset_ref_version")]
		public string AssetVersion;

		[JsonProperty("asset_type")]
		public string AssetType = "Unknown";

		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion;

		public override bool Equals(object obj)
		{
			if (obj is AssetReference assetReference && AssetID == assetReference.AssetID && AssetVersion == assetReference.AssetVersion)
			{
				return AssetType == assetReference.AssetType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(AssetID, AssetVersion, AssetType);
		}

		public static bool operator ==(AssetReference a, AssetReference b)
		{
			if ((object)a == b)
			{
				return true;
			}
			if ((object)a == null)
			{
				return false;
			}
			if ((object)b == null)
			{
				return false;
			}
			return a.Equals(b);
		}

		public static bool operator !=(AssetReference a, AssetReference b)
		{
			return !(a == b);
		}

		public override string ToString()
		{
			return AssetID + " (" + AssetVersion + ") - " + AssetType;
		}
	}
	public class ChangeTypeAttribute : UserFacingTextAttribute
	{
		private const string METASTRING_KEY = "%metastring%";

		public ChangeTypeAttribute(string userFacingText)
			: base(userFacingText)
		{
		}

		public string ResolveMetaString(string metaString)
		{
			return base.UserFacingText.Replace("%metastring%", metaString);
		}
	}
	public enum ChangeType
	{
		[ChangeType("created the asset!")]
		AssetCreated = 0,
		[ChangeType("updated asset version to %metastring%")]
		AssetVersionUpdated = 1,
		[ChangeType("updated child asset(s): %metastring%")]
		ChildAssetUpdated = 2,
		AutomaticAssetUpgrade = 3,
		BadDataAutoFix = 4,
		[ChangeType("painted some terrain.")]
		TerrainPainted = 100,
		[ChangeType("erased some terrain.")]
		TerrainErased = 101,
		[ChangeType("erased some props.")]
		PropErase = 200,
		[ChangeType("painted some props.")]
		PropPainted = 201,
		[ChangeType("pasted some copied props.")]
		PropPasted = 202,
		[ChangeType("moved some props around.")]
		PropMoved = 203,
		[ChangeType("renamed some props.")]
		PropLabelChanged = 204,
		[ChangeType("changed some prop values.")]
		PropMemberChanged = 205,
		[ChangeType("created some new wires.")]
		WireCreated = 300,
		[ChangeType("updated some wire details.")]
		WireUpdated = 301,
		[ChangeType("deleted some wires.")]
		WireDeleted = 302,
		[ChangeType("deleted some wire reroutes.")]
		WireRerouteDeleted = 303,
		[ChangeType("added some wire reroutes.")]
		WireRerouteAdded = 304,
		[ChangeType("recolored some wires.")]
		WireColorUpdated = 305,
		[ChangeType("updated the level name.")]
		LevelNameUpdated = 900,
		[ChangeType("updated the level description.")]
		LevelDescriptionUpdated = 901,
		[ChangeType("reordered the spawn points.")]
		LevelUpdatedSpawnPositionOrder = 902,
		[ChangeType("archived the level.")]
		LevelArchived = 903,
		[ChangeType("added screenshots to the level.")]
		LevelScreenshotsAdded = 904,
		[ChangeType("removed screenshots from the level.")]
		LevelScreenshotsRemoved = 905,
		[ChangeType("reordered screenshots for the level.")]
		LevelScreenshotsReorder = 906,
		[ChangeType("updated the game name.")]
		GameNameUpdated = 1000,
		[ChangeType("updated the game description.")]
		GameDescriptionUpdated = 1001,
		[ChangeType("updated the game's player count.")]
		GamePlayerCountUpdated = 1002,
		[ChangeType("added a new level.")]
		GameNewLevelAdded = 1003,
		[ChangeType("added screenshots to the game.")]
		GameScreenshotsAdded = 1004,
		[ChangeType("removed screenshots from the game.")]
		GameScreenshotsRemoved = 1005,
		[ChangeType("reordered screenshots for the game.")]
		GameScreenshotsReorder = 1006,
		[ChangeType("reordered levels for the game.")]
		GameLevelReorder = 1007,
		GameLibraryPropVersionChanged = 1500,
		GameLibraryPropAdded = 1510,
		GameLibraryPropRemoved = 1511,
		GameLibraryTerrainVersionChanged = 1520,
		GameLibraryTerrainAdded = 1530,
		GameLibraryTerrainRemoved = 1531,
		GameLibraryAudioVersionChanged = 1540,
		GameLibraryAudioAdded = 1541,
		GameLibraryAudioRemoved = 1542,
		GameLibraryUpdateAll = 1550,
		TerrainCreated = 1600,
		TerrainRemoved = 1601,
		TerrainUpdated = 1602,
		PropCreated = 1700,
		PropRemoved = 1701,
		PropUpdated = 1702,
		ScriptCreated = 1800,
		ScriptRemoved = 1801,
		ScriptUpdated = 1802,
		ParticleCreated = 1900,
		ParticleRemoved = 1901,
		ParticleUpdated = 1902,
		AudioUploaded = 2000,
		AudioFileUpdated = 2001
	}
	public class FileUploadResult
	{
		public int FileInstanceId = -1;

		public Exception Exception;

		public bool HasErrors => Exception != null;
	}
	public class FileUploadData
	{
		public byte[] Bytes;

		public string Filename;

		public string MimeType;
	}
	public static class CloudUploader
	{
		public static async void UploadFileBytes(EndlessCloudService endlessCloudService, FileUploadData fileUploadData, string folder = "endstar", Action<int> successCallback = null, Action<Exception> failureCallback = null)
		{
			FileUploadResult fileUploadResult = await UploadFileBytesAsync(endlessCloudService, fileUploadData, folder);
			if (fileUploadResult.HasErrors)
			{
				failureCallback?.Invoke(fileUploadResult.Exception);
			}
			else
			{
				successCallback?.Invoke(fileUploadResult.FileInstanceId);
			}
		}

		public static async Task<FileUploadResult> UploadFileBytesAsync(EndlessCloudService endlessCloudService, FileUploadData fileUploadData, string folder = "endstar")
		{
			_ = 2;
			try
			{
				GraphQlResult graphQlResult = await endlessCloudService.CreateFileUploadLinkAsync(fileUploadData.Filename, fileUploadData.MimeType, folder, fileUploadData.Bytes.Length);
				if (graphQlResult.HasErrors)
				{
					return new FileUploadResult
					{
						Exception = graphQlResult.GetErrorMessage()
					};
				}
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
				await unityWebRequest.SendWithRetry();
				if (unityWebRequest.result != UnityWebRequest.Result.Success)
				{
					return new FileUploadResult
					{
						Exception = new Exception(unityWebRequest.error)
					};
				}
				GraphQlResult graphQlResult2 = await endlessCloudService.MarkFileInstanceUploadedAsync(createFileUploadLinkResult.FileInstanceId);
				if (graphQlResult2.HasErrors)
				{
					return new FileUploadResult
					{
						Exception = graphQlResult2.GetErrorMessage()
					};
				}
				return new FileUploadResult
				{
					FileInstanceId = createFileUploadLinkResult.FileInstanceId
				};
			}
			catch (Exception exception)
			{
				return new FileUploadResult
				{
					Exception = exception
				};
			}
		}

		public static async void BatchUploadFileBytes(EndlessCloudService endlessCloudService, FileUploadData[] fileUploadData, string folder = "endstar", Action<int[]> successCallback = null, Action<Exception[]> failureCallback = null)
		{
			FileUploadResult[] source = await BatchUploadFileBytesAsync(endlessCloudService, fileUploadData, folder);
			if (source.Any((FileUploadResult result) => result.HasErrors))
			{
				failureCallback?.Invoke((from result in source
					where result.HasErrors
					select result.Exception).ToArray());
			}
			else
			{
				successCallback?.Invoke(source.Select((FileUploadResult result) => result.FileInstanceId).ToArray());
			}
		}

		public static async Task<FileUploadResult[]> BatchUploadFileBytesAsync(EndlessCloudService endlessCloudService, FileUploadData[] fileUploadData, string folder = "endstar")
		{
			return await Task.WhenAll(fileUploadData.Select((FileUploadData upload) => UploadFileBytesAsync(endlessCloudService, upload, folder)));
		}
	}
	[Serializable]
	public class AdditionalS3SecurityFields
	{
		[JsonProperty("Content-Type")]
		public string ContentType { get; private set; }

		[JsonProperty("bucket")]
		public string Bucket { get; private set; }

		[JsonProperty("X-Amz-Algorithm")]
		public string XAmzAlgorithm { get; private set; }

		[JsonProperty("X-Amz-Credential")]
		public string XAmzCredential { get; private set; }

		[JsonProperty("X-Amz-Date")]
		public string XAmzDate { get; private set; }

		[JsonProperty("key")]
		public string Key { get; private set; }

		[JsonProperty("Policy")]
		public string Policy { get; private set; }

		[JsonProperty("X-Amz-Signature")]
		public string XAmzSignature { get; private set; }
	}
	public class StringToAdditionalS3SecurityFieldsConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (reader.TokenType == JsonToken.String && reader.Value is string value)
			{
				return JsonConvert.DeserializeObject<AdditionalS3SecurityFields>(value);
			}
			throw new JsonException($"Expected string when reading AdditionalS3SecurityFields type, got '{reader.TokenType}' <{reader.Value}>.");
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(AdditionalS3SecurityFields);
		}
	}
	[Serializable]
	public class CreateFileUploadLinkResult
	{
		[JsonProperty("secure_upload_url")]
		public string SecureUploadURL { get; private set; }

		[JsonProperty("additional_s3_security_fields")]
		[JsonConverter(typeof(StringToAdditionalS3SecurityFieldsConverter))]
		public AdditionalS3SecurityFields AdditionalS3SecurityFields { get; private set; }

		[JsonProperty("file_id")]
		public int FileId { get; private set; }

		[JsonProperty("file_instance_id")]
		public int FileInstanceId { get; private set; }
	}
	public class CuratedGamesList : AssetList
	{
		private const string ASSET_TYPE = "gameList";

		public static CuratedGamesList FromReferences(string name, string description, SerializableGuid assetId, IEnumerable<AssetReference> references)
		{
			return new CuratedGamesList
			{
				Name = name,
				Description = description,
				AssetType = "gameList",
				AssetID = assetId,
				assets = new List<AssetReference>(references)
			};
		}
	}
	[Serializable]
	public class FileAssetInstance
	{
		[JsonProperty("label")]
		public string Label;

		[JsonProperty("asset_file_instance_id")]
		public int AssetFileInstanceId;
	}
	[Serializable]
	public class FileAsset : Asset
	{
		[JsonProperty("file_instances")]
		public List<FileAssetInstance> FileInstances = new List<FileAssetInstance>();

		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion => false;

		public FileAsset()
		{
			AssetType = "screenshot";
		}

		public override object GetAnonymousObjectForUpload()
		{
			FileAsset? fileAsset = JsonConvert.DeserializeObject<FileAsset>(JsonConvert.SerializeObject(this));
			fileAsset.AssetVersion = string.Empty;
			return fileAsset;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(base.ToString());
			stringBuilder.Append($"\nFile Count: {FileInstances.Count}");
			for (int i = 0; i < FileInstances.Count; i++)
			{
				stringBuilder.Append($"\n File: Label - {FileInstances[i].Label} Instance Id - {FileInstances[i].AssetFileInstanceId}");
			}
			return stringBuilder.ToString();
		}
	}
	public class GameAssetPreview : Asset
	{
		[JsonProperty("screenshot")]
		private FileAssetInstance screenshot;

		[JsonProperty("iconFileInstanceId")]
		private int iconFileInstanceId;

		[JsonIgnore]
		public static string AssetReturnArgs => "{ asset_id asset_version asset_type iconFileInstanceId Name Description internal_version revision_meta_data { revision_timestamp changes } screenshot { asset_file_instance_id } }";

		[JsonIgnore]
		public int IconFileInstanceId
		{
			get
			{
				if (screenshot != null)
				{
					return screenshot.AssetFileInstanceId;
				}
				return iconFileInstanceId;
			}
		}
	}
	public class LibraryContentPack : AssetList
	{
		private const string ASSET_TYPE = "libraryContentPack";

		public static LibraryContentPack FromReferences(string name, string description, SerializableGuid assetId, IEnumerable<AssetReference> references)
		{
			return new LibraryContentPack
			{
				Name = name,
				Description = description,
				AssetType = "libraryContentPack",
				AssetID = assetId,
				assets = new List<AssetReference>(references)
			};
		}
	}
	[Serializable]
	public class ChangeData
	{
		[JsonProperty("change_type")]
		private int changeType;

		[JsonProperty("metadata")]
		private string metadata;

		[JsonIgnore]
		public ChangeType ChangeType
		{
			get
			{
				return (ChangeType)changeType;
			}
			set
			{
				changeType = (int)value;
			}
		}

		[JsonProperty("user_id")]
		public int UserId { get; set; }

		[JsonIgnore]
		public string Metadata
		{
			get
			{
				return metadata;
			}
			set
			{
				metadata = value;
			}
		}

		public override bool Equals(object obj)
		{
			ChangeData changeData = (ChangeData)obj;
			if (ChangeType.Equals(changeData.ChangeType) && UserId.Equals(changeData.UserId))
			{
				return Metadata == changeData.Metadata;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = UserId.GetHashCode() ^ ChangeType.GetHashCode();
			if (Metadata != null)
			{
				num ^= Metadata.GetHashCode();
			}
			return num;
		}

		public ChangeData Copy()
		{
			return new ChangeData
			{
				changeType = changeType,
				metadata = metadata,
				UserId = UserId
			};
		}
	}
	[Serializable]
	public class RevisionMetaData
	{
		[JsonProperty("revision_timestamp")]
		public long RevisionTimestamp { get; set; }

		[JsonProperty("changes")]
		public HashSet<ChangeData> Changes { get; } = new HashSet<ChangeData>();

		public RevisionMetaData()
		{
			RevisionTimestamp = DateTime.UtcNow.Ticks;
		}

		public RevisionMetaData(HashSet<ChangeData> initialChanges)
		{
			RevisionTimestamp = DateTime.UtcNow.Ticks;
			Changes = initialChanges;
		}

		public RevisionMetaData Copy()
		{
			RevisionMetaData revisionMetaData = new RevisionMetaData
			{
				RevisionTimestamp = RevisionTimestamp
			};
			foreach (ChangeData change in Changes)
			{
				revisionMetaData.Changes.Add(change.Copy());
			}
			return revisionMetaData;
		}
	}
}
