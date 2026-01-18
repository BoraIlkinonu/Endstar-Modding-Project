using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Shared.Utilities;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace Endless.Matchmaking
{
	// Token: 0x02000031 RID: 49
	public class EndlessCloudService
	{
		// Token: 0x0600012A RID: 298 RVA: 0x00005DCC File Offset: 0x00003FCC
		public async Task<GraphQlResult> CreateAssetAsync(object asset, bool debugQuery = false, int timeout = 10)
		{
			object obj = new { asset };
			return await GraphQlRequest.Request(QueryType.Mutation, "createAsset", obj, null, this.AuthToken, debugQuery, timeout, false);
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00005E28 File Offset: 0x00004028
		public async Task<GraphQlResult> GetAssetAsync(SerializableGuid assetId, string version = "", AssetParams assetParams = null, bool debugQuery = false, int timeout = 10)
		{
			if (assetParams == null)
			{
				assetParams = new AssetParams(null, false, null);
			}
			object obj = new
			{
				asset_id = assetId,
				asset_version = version,
				asset_query = assetParams.AssetQueryFilter,
				populate_refs = assetParams.PopulateRefs
			};
			GraphQlResult graphQlResult;
			if (assetParams.AssetReturnArgs != null)
			{
				graphQlResult = GraphQlResult.CreateErrorResult(new ArgumentException(string.Format("You cannot specify return args on this call! value is : \"{0}\" null? {1}", assetParams.AssetReturnArgs, assetParams.AssetReturnArgs == null)));
			}
			else
			{
				graphQlResult = await this.GetAssetAsync(obj, null, debugQuery, timeout);
			}
			return graphQlResult;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00005E98 File Offset: 0x00004098
		private async Task<GraphQlResult> GetAssetAsync(object inputArgs, string returnArgs = null, bool debugQuery = false, int timeOut = 10)
		{
			return await GraphQlRequest.Request(QueryType.Query, "getAssetByIdAndVersion", inputArgs, returnArgs, this.AuthToken, debugQuery, timeOut, false);
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00005EFC File Offset: 0x000040FC
		public async Task<GraphQlResult> DoesAssetExist(string assetId, bool debugQuery = false, int timeOut = 10)
		{
			object obj = new
			{
				asset_id = assetId
			};
			return await GraphQlRequest.Request(QueryType.Query, "isAssetExist", obj, null, this.AuthToken, debugQuery, timeOut, false);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00005F58 File Offset: 0x00004158
		public async Task<GraphQlResult[]> GetAssetsAsync(SerializableGuid[] assetIds, bool debugQueries = false)
		{
			return await Task.WhenAll<GraphQlResult>(assetIds.Select((SerializableGuid id) => this.GetAssetAsync(id, "", null, debugQueries, 10)));
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00005FAC File Offset: 0x000041AC
		public async Task<GraphQlResult> GetAssetsByTypeAvailableForRoleAsync(string type, int roleId, AssetParams assetParams = null, PaginationParams paginationParams = null, bool applyModerationFilter = true, bool debugQuery = false)
		{
			if (assetParams == null)
			{
				assetParams = new AssetParams(null, false, null);
			}
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			string text = "query " + assetParams.AssetQueryFilter + " " + assetParams.AssetReturnArgs;
			object obj = new
			{
				asset_type = type,
				role_id = roleId,
				offset = paginationParams.Offset,
				limit = paginationParams.Limit,
				asset_query = text,
				populate_refs = assetParams.PopulateRefs
			};
			string text2 = "data, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getAssetsByTypeAvailableForRole", obj, text2, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000130 RID: 304 RVA: 0x0000601C File Offset: 0x0000421C
		public async Task<GraphQlResult> GetAssetsByTypeAsync(string type, AssetParams assetParams = null, PaginationParams paginationParams = null, bool applyModerationFilter = true, bool debugQuery = false, int timeout = 10)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			if (assetParams == null)
			{
				assetParams = new AssetParams(null, false, null);
			}
			string text = "query " + assetParams.AssetQueryFilter + " " + assetParams.AssetReturnArgs;
			object obj;
			if (EndlessCloudService.IsModerator || EndlessCloudService.IsAdmin)
			{
				obj = new
				{
					asset_type = type,
					populate_refs = assetParams.PopulateRefs,
					asset_query = text,
					moderation_filter = applyModerationFilter,
					offset = paginationParams.Offset,
					limit = paginationParams.Limit
				};
			}
			else
			{
				obj = new
				{
					asset_type = type,
					populate_refs = assetParams.PopulateRefs,
					asset_query = text,
					offset = paginationParams.Offset,
					limit = paginationParams.Limit
				};
			}
			string text2 = "data, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getAssetsByType", obj, text2, this.AuthToken, debugQuery, timeout, false);
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00006094 File Offset: 0x00004294
		public async Task<GraphQlResult> UpdateAssetAsync(object asset, bool useThreadForInputSerialization = false, bool debugQuery = false)
		{
			object obj = new { asset };
			return await GraphQlRequest.Request(QueryType.Mutation, "updateAsset", obj, null, this.AuthToken, debugQuery, 60, useThreadForInputSerialization);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x000060F0 File Offset: 0x000042F0
		public async Task<GraphQlResult> UpdateAssetAsync(object asset, SerializableGuid ancestorId, bool useThreadForInputSerialization = false, bool debugQuery = false)
		{
			object obj = new
			{
				asset = asset,
				ancestor_asset_id_for_checking_rights = ancestorId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "updateAsset", obj, null, this.AuthToken, debugQuery, 60, useThreadForInputSerialization);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00006154 File Offset: 0x00004354
		public async Task<GraphQlResult> DeleteAssetAsync(SerializableGuid id, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = id
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "deleteAsset", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000134 RID: 308 RVA: 0x000061A8 File Offset: 0x000043A8
		public async Task<GraphQlResult> GetVersionsAsync(SerializableGuid assetId, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = assetId.ToString()
			};
			return await GraphQlRequest.Request(QueryType.Query, "getAssetVersionsById", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000135 RID: 309 RVA: 0x000061FC File Offset: 0x000043FC
		public async Task<GraphQlResult> GetAssetsByTypeAndPublishStateAsync(string assetType, string publishingState, AssetParams assetParams = null, PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (assetParams == null)
			{
				assetParams = new AssetParams(null, false, null);
			}
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			string text = "query " + assetParams.AssetQueryFilter + " " + assetParams.AssetReturnArgs;
			object obj = new
			{
				asset_type = assetType,
				publishingState = publishingState,
				offset = paginationParams.Offset,
				limit = paginationParams.Limit,
				asset_query = text,
				populate_refs = false
			};
			string text2 = "data, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getAssetsByTypeAndPublishState", obj, text2, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0000626C File Offset: 0x0000446C
		public async Task<GraphQlResult> SetPublishStateOnAssetAsync(SerializableGuid assetId, string version, string publishingState, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = assetId,
				asset_version = version,
				state = publishingState
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "setPublishStateOnAsset", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000137 RID: 311 RVA: 0x000062D0 File Offset: 0x000044D0
		public async Task<GraphQlResult> GetPublishedVersionsOfAssetAsync(SerializableGuid assetId, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = assetId
			};
			return await GraphQlRequest.Request(QueryType.Query, "getPublishedVersionsOfAsset", obj, "asset_version,state", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00006324 File Offset: 0x00004524
		public async Task<GraphQlResult> GetBulkAssets([TupleElementNames(new string[] { "AssetID", "AssetVersion" })] ValueTuple<SerializableGuid, string>[] assetIdentifiers, AssetParams assetParams = null, bool debugQuery = true)
		{
			if (assetParams == null)
			{
				assetParams = new AssetParams(null, false, null);
			}
			object[] array = new object[assetIdentifiers.Length];
			for (int i = 0; i < assetIdentifiers.Length; i++)
			{
				array[i] = new
				{
					asset_id = assetIdentifiers[i].Item1,
					asset_version = assetIdentifiers[i].Item2
				};
			}
			string text = "query " + assetParams.AssetQueryFilter + " " + assetParams.AssetReturnArgs;
			object obj = new
			{
				assets = array,
				asset_query = text,
				populate_refs = assetParams.PopulateRefs
			};
			return await GraphQlRequest.Request(QueryType.Query, "getAssetsByIdAndVersion", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00006380 File Offset: 0x00004580
		public async Task<GraphQlResult> CreateModerationFlag(int code, ModerationCategory category, ModerationAction action, string description, bool isActive, bool debugQuery = false)
		{
			object obj = new
			{
				code = code,
				category = this.ModerationCategoryToString(category),
				action = this.ModerationActionToString(action),
				description = description,
				is_active = isActive
			};
			string text = ModerationFlag.Query ?? "";
			return await GraphQlRequest.Request(QueryType.Mutation, "createModerationFlag", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013A RID: 314 RVA: 0x000063F8 File Offset: 0x000045F8
		public async Task<GraphQlResult> UpdateModerationFlag(int id, int code, ModerationCategory category, ModerationAction action, string description, bool isActive, bool debugQuery = false)
		{
			object obj = new
			{
				id = id,
				code = code,
				category = this.ModerationCategoryToString(category),
				action = this.ModerationActionToString(action),
				description = description,
				is_active = isActive
			};
			string text = ModerationFlag.Query ?? "";
			return await GraphQlRequest.Request(QueryType.Mutation, "updateModerationFlag", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00006478 File Offset: 0x00004678
		public async Task<GraphQlResult> DeleteModerationFlag(int id, bool debugQuery = false)
		{
			object obj = new { id };
			string text = ModerationFlag.Query ?? "";
			return await GraphQlRequest.Request(QueryType.Mutation, "deleteModerationFlag", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013C RID: 316 RVA: 0x000064CC File Offset: 0x000046CC
		public async Task<GraphQlResult> GetAllModerationFlags(bool debugQuery = false)
		{
			string text = "id code action description is_active";
			return await GraphQlRequest.Request(QueryType.Query, "getAllModerationFlags", null, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00006518 File Offset: 0x00004718
		public async Task<GraphQlResult> GetModerationFlagById(int id, bool debugQuery = false)
		{
			object obj = new { id };
			string text = ModerationFlag.Query ?? "";
			return await GraphQlRequest.Request(QueryType.Query, "getModerationFlagById", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013E RID: 318 RVA: 0x0000656C File Offset: 0x0000476C
		public async Task<GraphQlResult> GetAssetObjectFlags(string assetId, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				identifier = assetId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset")
			};
			string text = "identifier moderations { id reason moderator { id username} flag { id code category action description is_active } }";
			return await GraphQlRequest.Request(QueryType.Query, "getObjectFlags", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000065C0 File Offset: 0x000047C0
		public async Task<GraphQlResult> GetBulkObjectFlags(string[] assetIds, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				objects = assetIds.Select((string id) => new
				{
					identifier = id,
					entity_type = new EndlessCloudService.EntityTypeEnum("asset")
				}).ToArray()
			};
			string text = "results { identifier moderations { id reason moderator { id username } flag { id code category action description } } }";
			return await GraphQlRequest.Request(QueryType.Query, "getBulkObjectFlags", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00006614 File Offset: 0x00004814
		public async Task<GraphQlResult> SetObjectFlag(string assetId, string flagCode, string reason, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				identifier = assetId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset"),
				flag_code = flagCode,
				reason = reason
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "setObjectFlag", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00006678 File Offset: 0x00004878
		public async Task<GraphQlResult> SetBulkObjectFlags(string assetId, string[] flagCodes, string reason, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				identifier = assetId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset"),
				flag_codes = flagCodes,
				reason = reason
			};
			string text = "id";
			return await GraphQlRequest.Request(QueryType.Mutation, "setBulkObjectFlags", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x000066DC File Offset: 0x000048DC
		public async Task<GraphQlResult> SetFlagOnBulkObjects(string[] assetIds, string flagCode, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_code = flagCode,
				objects = assetIds.Select((string id) => new
				{
					identifier = id,
					entity_type = new EndlessCloudService.EntityTypeEnum("asset")
				}).ToArray()
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "setFlagOnBulkObjects", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00006738 File Offset: 0x00004938
		public async Task<GraphQlResult> RemoveObjectFlag(string assetId, string flagCode, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				identifier = assetId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset"),
				flag_code = flagCode
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeObjectFlag", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00006794 File Offset: 0x00004994
		public async Task<GraphQlResult> RemoveFlagFromBulkObjects(string[] assetIds, string flagCode, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_code = flagCode,
				objects = assetIds.Select((string id) => new
				{
					identifier = id,
					entity_type = new EndlessCloudService.EntityTypeEnum("asset")
				}).ToArray()
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeFlagOnBulkObjects", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000145 RID: 325 RVA: 0x000067F0 File Offset: 0x000049F0
		public async Task<GraphQlResult> ClearAllObjectFlags(string assetId, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				identifier = assetId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset")
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "clearAllObjectFlags", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00006844 File Offset: 0x00004A44
		public async Task<GraphQlResult> GetUserModerationSettings(PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			var <>f__AnonymousType = new
			{
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getUserModerationSettings", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00006898 File Offset: 0x00004A98
		public async Task<GraphQlResult> AddUserModerationSettings(int flagId, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_id = flagId
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "addUserModerationSettings", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000148 RID: 328 RVA: 0x000068EC File Offset: 0x00004AEC
		public async Task<GraphQlResult> AddBulkUserModerationSettings(int[] flagIds, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_ids = flagIds
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "addBulkUserModerationSettings", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00006940 File Offset: 0x00004B40
		public async Task<GraphQlResult> RemoveUserModerationSettings(int flagId, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_id = flagId
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeUserModerationSettings", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014A RID: 330 RVA: 0x00006994 File Offset: 0x00004B94
		public async Task<GraphQlResult> RemoveBulkUserModerationSettings(int[] flagIds, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				flag_ids = flagIds
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeBulkUserModerationSettings", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x000069E8 File Offset: 0x00004BE8
		public async Task<GraphQlResult> GetGroupModerationPolicy(int groupId, PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			var <>f__AnonymousType = new
			{
				gorup_id = groupId,
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getGroupModerationPolicy", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00006A44 File Offset: 0x00004C44
		public async Task<GraphQlResult> AddGroupModerationPolicy(int groupId, int flagId, bool enforceOnMembers, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				gorup_id = groupId,
				flag_id = flagId,
				enforce_on_members = enforceOnMembers
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "addGroupModerationPolicy", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00006AA8 File Offset: 0x00004CA8
		public async Task<GraphQlResult> AddBulkGroupModerationPolicy(int groupId, int[] flagIds, bool enforceOnMembers, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				gorup_id = groupId,
				flag_ids = flagIds,
				enforce_on_members = enforceOnMembers
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "addBulkGroupModerationPolicy", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00006B0C File Offset: 0x00004D0C
		public async Task<GraphQlResult> RemoveGroupModerationPolicy(int groupId, int flagId, bool enforceOnMembers, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				gorup_id = groupId,
				flag_id = flagId
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeGroupModerationPolicy", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x00006B68 File Offset: 0x00004D68
		public async Task<GraphQlResult> RemoveBulkGroupModerationPolicy(int groupId, int[] flagIds, bool enforceOnMembers, bool debugQuery = false)
		{
			var <>f__AnonymousType = new
			{
				gorup_id = groupId,
				flag_ids = flagIds
			};
			string text = "";
			return await GraphQlRequest.Request(QueryType.Mutation, "removeBulkGroupModerationPolicy", <>f__AnonymousType, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00006BC4 File Offset: 0x00004DC4
		public async Task<GraphQlResult> GetRestrictedFlagsForUser(bool debugQuery = false)
		{
			string text = "blocked_flags {  id code action description is_active }";
			return await GraphQlRequest.Request(QueryType.Query, "getRestrictedFlagsForUser", null, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00006C10 File Offset: 0x00004E10
		private string ModerationCategoryToString(ModerationCategory category)
		{
			switch (category)
			{
			case ModerationCategory.Content:
				return "content";
			case ModerationCategory.Regional:
				return "regional";
			case ModerationCategory.Age:
				return "age";
			case ModerationCategory.Platform:
				return "platform";
			default:
				throw new ArgumentOutOfRangeException("category", category, null);
			}
		}

		// Token: 0x06000152 RID: 338 RVA: 0x00006C60 File Offset: 0x00004E60
		private string ModerationActionToString(ModerationAction action)
		{
			switch (action)
			{
			case ModerationAction.ServerHide:
				return "server_hide";
			case ModerationAction.ServerRestrict:
				return "server_restrict";
			case ModerationAction.ServerDelete:
				return "server_delete";
			case ModerationAction.ClientBlock:
				return "client_block";
			case ModerationAction.ClientBlur:
				return "client_blur";
			case ModerationAction.ClientReplace:
				return "client_replace";
			case ModerationAction.ClientWarn:
				return "client_warn";
			case ModerationAction.NoAction:
				return "no_action";
			default:
				throw new ArgumentOutOfRangeException("action", action, null);
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000153 RID: 339 RVA: 0x00006CD6 File Offset: 0x00004ED6
		// (set) Token: 0x06000154 RID: 340 RVA: 0x00006CDE File Offset: 0x00004EDE
		public User ActiveUser { get; private set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000155 RID: 341 RVA: 0x00006CE7 File Offset: 0x00004EE7
		public int ActiveUserId
		{
			get
			{
				return this.ActiveUser.Id;
			}
		}

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000156 RID: 342 RVA: 0x00006CF4 File Offset: 0x00004EF4
		public string ActiveUserName
		{
			get
			{
				return this.ActiveUser.UserName;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x06000157 RID: 343 RVA: 0x00006D01 File Offset: 0x00004F01
		// (set) Token: 0x06000158 RID: 344 RVA: 0x00006D08 File Offset: 0x00004F08
		public static bool IsAdmin { get; private set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x06000159 RID: 345 RVA: 0x00006D10 File Offset: 0x00004F10
		// (set) Token: 0x0600015A RID: 346 RVA: 0x00006D17 File Offset: 0x00004F17
		public static bool IsModerator { get; private set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600015B RID: 347 RVA: 0x00006D1F File Offset: 0x00004F1F
		// (set) Token: 0x0600015C RID: 348 RVA: 0x00006D26 File Offset: 0x00004F26
		public static List<ModerationFlag> ContentRestrictionsOnAccount { get; private set; } = new List<ModerationFlag>();

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00006D2E File Offset: 0x00004F2E
		// (set) Token: 0x0600015E RID: 350 RVA: 0x00006D35 File Offset: 0x00004F35
		public static List<ModerationFlag> AllModerationFlags { get; private set; } = new List<ModerationFlag>();

		// Token: 0x0600015F RID: 351 RVA: 0x00006D40 File Offset: 0x00004F40
		public EndlessCloudService(string authToken, bool setupWebsockets = true)
		{
			this.AuthToken = authToken;
			this.GetCurrentUser(null, null, "id, public_id, username", false);
			if (setupWebsockets)
			{
				for (int i = 0; i < 5; i++)
				{
					this.webSocketMessageIdDispatcher.Add((WebSocketMessageId)i, new List<Action<WebSocketPayload>>());
				}
				this.SetupWebSocket();
			}
		}

		// Token: 0x06000160 RID: 352 RVA: 0x00006D9C File Offset: 0x00004F9C
		~EndlessCloudService()
		{
			Debug.Log("Endless Cloud Service destructor called");
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00006DCC File Offset: 0x00004FCC
		public static async Task SignIn(string username, string password, bool rememberMe = false, Action<string> onSuccess = null, Action<GraphQlResult> onFail = null)
		{
			try
			{
				object obj = new
				{
					username_or_email = username,
					password = password
				};
				GraphQlResult graphQlResult = await GraphQlRequest.Request(QueryType.Mutation, "loginWithUsernameOrEmail", obj, null, null, false, 10, false);
				if (graphQlResult != null && !graphQlResult.HasErrors)
				{
					string text = ((graphQlResult != null) ? graphQlResult.GetDataMember().ToString() : null);
					if (rememberMe)
					{
						PlayerPrefs.SetString("USER_AUTH_PLATFORM_ID", username);
						PlayerPrefs.SetInt("USER_AUTH_PLATFORM_TYPE", 3);
						PlayerPrefs.SetString("USER_AUTH_PLATFORM_TOKEN", text);
					}
					else
					{
						EndlessCloudService.ClearCachedCredentials();
					}
					if (onSuccess != null)
					{
						onSuccess(text);
					}
				}
				else if (onFail != null)
				{
					onFail(graphQlResult);
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (onFail != null)
				{
					onFail(GraphQlResult.CreateErrorResult(ex));
				}
			}
		}

		// Token: 0x06000162 RID: 354 RVA: 0x00006E30 File Offset: 0x00005030
		public static void SetAllModerationFlags(List<ModerationFlag> moderationFlags)
		{
			EndlessCloudService.AllModerationFlags = moderationFlags;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00006E38 File Offset: 0x00005038
		public async Task CheckUserElevatedRolesAndRestrictions(string authToken)
		{
			GraphQlResult graphQlResult = await this.GetRestrictedFlagsForUser(true);
			if (graphQlResult.HasErrors)
			{
				Debug.LogException(graphQlResult.GetErrorMessage(0));
			}
			else
			{
				var <>f__AnonymousType = new
				{
					blocked_flags = Array.Empty<ModerationFlag>()
				};
				var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
				if (<>f__AnonymousType2 == null || <>f__AnonymousType2.blocked_flags == null)
				{
					Debug.LogException(new Exception("Restricted Flags result could not be parsed into viable Content Restrictions for account"));
				}
				else
				{
					EndlessCloudService.ContentRestrictionsOnAccount = <>f__AnonymousType2.blocked_flags.ToList<ModerationFlag>();
				}
			}
			EndlessCloudService.IsAdmin = false;
			EndlessCloudService.IsModerator = false;
			GraphQlResult graphQlResult2 = await GraphQlRequest.Request(QueryType.Query, "me", null, "id, username, roles {id name}", authToken, false, 10, false);
			if (graphQlResult2.HasErrors)
			{
				Debug.LogError(graphQlResult2.GetErrorMessage(0));
			}
			else
			{
				var <>f__AnonymousType3 = new
				{
					id = -1,
					username = "",
					roles = new <>f__AnonymousType30<int, string>[]
					{
						new
						{
							id = 0,
							name = ""
						}
					}
				};
				var roles = JsonConvert.DeserializeAnonymousType(graphQlResult2.GetDataMember().ToString(), <>f__AnonymousType3).roles;
				for (int i = 0; i < roles.Length; i++)
				{
					var <>f__AnonymousType4 = roles[i];
					if (<>f__AnonymousType4.id == 1)
					{
						EndlessCloudService.IsAdmin = true;
					}
					else if (<>f__AnonymousType4.id == 21)
					{
						EndlessCloudService.IsModerator = true;
					}
				}
			}
			if (EndlessCloudService.IsAdmin || EndlessCloudService.IsModerator)
			{
				try
				{
					GraphQlResult graphQlResult3 = await this.GetAllModerationFlags(true);
					if (graphQlResult3.HasErrors)
					{
						throw graphQlResult3.GetErrorMessage(0);
					}
					List<ModerationFlag> dataMember = graphQlResult3.GetDataMember<List<ModerationFlag>>();
					for (int j = 0; j < dataMember.Count; j++)
					{
						dataMember[j].NiceName = StringUtilities.PrettifyName(dataMember[j].Code);
					}
					EndlessCloudService.SetAllModerationFlags(dataMember);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00006E84 File Offset: 0x00005084
		public static async Task<GraphQlResult> DirectLoginAsync(string username, string password)
		{
			object obj = new
			{
				username_or_email = username,
				password = password
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "loginWithUsernameOrEmail", obj, null, null, false, 10, false);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00006ECF File Offset: 0x000050CF
		[Obsolete("Use async version instead")]
		public static void VerifyPlayerPrefsToken(Action<string> onSuccess, Action<Exception> onFailure)
		{
			if (PlayerPrefs.HasKey("USER_AUTH_PLATFORM_TOKEN"))
			{
				EndlessCloudService.VerifyToken(PlayerPrefs.GetString("USER_AUTH_PLATFORM_TOKEN"), onSuccess, onFailure);
				return;
			}
			onFailure(new Exception("No stored token for auto login"));
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00006F00 File Offset: 0x00005100
		public static async void VerifyToken(string authToken, Action<string> onSuccess, Action<Exception> onFailure)
		{
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_PASS");
			string text = "id, username";
			GraphQlResult graphQlResult = await GraphQlRequest.Request(QueryType.Query, "me", null, text, authToken, false, 10, false);
			if (graphQlResult.HasErrors)
			{
				onFailure(graphQlResult.GetErrorMessage(0));
			}
			else
			{
				onSuccess(authToken);
			}
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00006F47 File Offset: 0x00005147
		public static string GetCachedUserName()
		{
			if (PlayerPrefs.HasKey("USER_AUTH_PLATFORM_ID"))
			{
				return PlayerPrefs.GetString("USER_AUTH_PLATFORM_ID");
			}
			return string.Empty;
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00006F65 File Offset: 0x00005165
		public static void ClearCachedCredentials()
		{
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_ID");
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_PASS");
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_TOKEN");
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_TYPE");
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00006F8F File Offset: 0x0000518F
		public static void ClearCachedToken()
		{
			PlayerPrefs.DeleteKey("USER_AUTH_PLATFORM_TOKEN");
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00006F9C File Offset: 0x0000519C
		public async void GetCurrentUser(Action onSuccessCallback = null, Action<Exception> onFailureCallback = null, string returnArgs = "id, public_id, username", bool debugQuery = false)
		{
			GraphQlResult graphQlResult = await GraphQlRequest.Request(QueryType.Query, "me", null, returnArgs, this.AuthToken, debugQuery, 10, false);
			if (graphQlResult.HasErrors)
			{
				if (onFailureCallback != null)
				{
					onFailureCallback(graphQlResult.GetErrorMessage(0));
				}
			}
			else
			{
				this.ActiveUser = JsonConvert.DeserializeObject<User>(graphQlResult.GetDataMember().ToString());
				CrashReportHandler.SetUserMetadata("UserId", this.ActiveUserId.ToString());
				CrashReportHandler.SetUserMetadata("UserName", this.ActiveUserName);
				if (onSuccessCallback != null)
				{
					onSuccessCallback();
				}
			}
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00006FF4 File Offset: 0x000051F4
		public async Task<GraphQlResult> FireAuthenticatedRequest(QueryType type, string callName, object inputArgs = null, string returnArgs = null, bool debugQuery = false)
		{
			return await GraphQlRequest.Request(type, callName, inputArgs, returnArgs, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00007064 File Offset: 0x00005264
		public async Task<GraphQlResult> SearchUsers(string query, PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			object obj = new
			{
				query = query,
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data { id public_id username }, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "searchUsers", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000070C0 File Offset: 0x000052C0
		public async Task<GraphQlResult> GetUserById(int userId, bool debugQuery = false)
		{
			object obj = new
			{
				user_id = userId
			};
			return await GraphQlRequest.Request(QueryType.Query, "getUserByIdV2", obj, "id public_id username", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00007113 File Offset: 0x00005313
		public void Dispose()
		{
			GraphQlRequest.CloseWebsocket();
		}

		// Token: 0x0600016F RID: 367 RVA: 0x0000711C File Offset: 0x0000531C
		public static bool CanHaveRiflemen()
		{
			string[] invalidFlags = new string[] { "all_combat", "ranged_combat", "realistic_npc_guns" };
			return !EndlessCloudService.ContentRestrictionsOnAccount.Any((ModerationFlag restriction) => invalidFlags.Contains(restriction.Code));
		}

		// Token: 0x06000170 RID: 368 RVA: 0x0000716C File Offset: 0x0000536C
		public static bool CanHaveGrunt()
		{
			string[] invalidFlags = new string[] { "all_combat", "melee_combat" };
			return !EndlessCloudService.ContentRestrictionsOnAccount.Any((ModerationFlag restriction) => invalidFlags.Contains(restriction.Code));
		}

		// Token: 0x06000171 RID: 369 RVA: 0x000071B4 File Offset: 0x000053B4
		public static bool CanHaveZombies()
		{
			string[] invalidFlags = new string[] { "all_combat", "melee_combat" };
			return !EndlessCloudService.ContentRestrictionsOnAccount.Any((ModerationFlag restriction) => invalidFlags.Contains(restriction.Code));
		}

		// Token: 0x06000172 RID: 370 RVA: 0x000071FC File Offset: 0x000053FC
		public async Task<GraphQlResult> CreateFileUploadLinkAsync(string name, string mimeType, string folder, int size, bool debugQuery = false)
		{
			object obj = new
			{
				name = name,
				mime_type = mimeType,
				size = size,
				folder = folder
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "createFileUploadLink", obj, "secure_upload_url,additional_s3_security_fields,file_id,file_instance_id", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000726C File Offset: 0x0000546C
		public async Task<GraphQlResult> MarkFileInstanceUploadedAsync(int fileInstanceId, bool debugQuery = false)
		{
			object obj = new
			{
				file_instance_id = fileInstanceId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "markFileInstanceUploaded", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x000072C0 File Offset: 0x000054C0
		public async Task<GraphQlResult> GetFileInstanceByIdAsync(int fileInstanceId, bool debugQuery = false)
		{
			object obj = new
			{
				file_instance_id = fileInstanceId
			};
			return await GraphQlRequest.Request(QueryType.Query, "getFileInstanceById", obj, "name,mime_type,file_url,file_id", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x00007314 File Offset: 0x00005514
		public async Task<GraphQlResult> SendFriendRequestAsync(string usernameToFriend, bool debugQuery = false)
		{
			object obj = new
			{
				recipient_username = usernameToFriend
			};
			string text = "id sender_id recipient_id request_status sender { id public_id username roles { id name } presence { status lastSeen user_id } metadata { image_url } } recipient { id public_id username }";
			return await GraphQlRequest.Request(QueryType.Mutation, "sendFriendRequest", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x00007368 File Offset: 0x00005568
		public async Task<GraphQlResult> GetFriendRequestsAsync(PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			object obj = new
			{
				status = new EndlessCloudService.EntityTypeEnum("pending"),
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data { id sender {id username} recipient {id username} }, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getFriendRequests", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000073BC File Offset: 0x000055BC
		public async Task<GraphQlResult> GetSentFriendRequests(PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			object obj = new
			{
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data { id sender {id username} recipient {id username} }, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getOngoingFriendRequests", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00007410 File Offset: 0x00005610
		public async Task<GraphQlResult> AcceptFriendRequestAsync(string requestId, bool debugQuery = false)
		{
			object obj = new
			{
				request_id = requestId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "acceptFriendRequest", obj, "id sender_id recipient_id request_status sender { id public_id username roles { id name } presence { status lastSeen user_id } metadata { image_url } } recipient { id public_id username }", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00007464 File Offset: 0x00005664
		public async Task<GraphQlResult> RejectFriendRequestAsync(string requestId, bool debugQuery = false)
		{
			object obj = new
			{
				request_id = requestId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "rejectFriendRequest", obj, "id sender_id recipient_id request_status sender { id public_id username roles { id name } presence { status lastSeen user_id } metadata { image_url } } recipient { id public_id username }", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x000074B8 File Offset: 0x000056B8
		public async Task<GraphQlResult> CancelFriendRequestAsync(string requestId, bool debugQuery = false)
		{
			object obj = new
			{
				request_id = requestId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "cancelFriendRequest", obj, "id sender_id recipient_id request_status", this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000750C File Offset: 0x0000570C
		public async Task<GraphQlResult> GetFriendshipsAsync(PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			object obj = new
			{
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data {id user1 { id public_id username } user2 { id public_id username}}, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getFriendships", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00007560 File Offset: 0x00005760
		public async Task<GraphQlResult> UnfriendAsync(int userId, bool debugQuery = false)
		{
			object obj = new
			{
				other_user_id = userId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "unfriend", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000075B4 File Offset: 0x000057B4
		public async Task<GraphQlResult> BlockUserAsync(int userId, bool debugQuery = false)
		{
			object obj = new
			{
				other_user_id = userId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "blockUser", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00007608 File Offset: 0x00005808
		public async Task<GraphQlResult> UnblockUserAsync(int userId, bool debugQuery = false)
		{
			object obj = new
			{
				other_user_id = userId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "unblockUser", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x0000765C File Offset: 0x0000585C
		public async Task<GraphQlResult> GetBlockedUsersAsync(PaginationParams paginationParams = null, bool debugQuery = false)
		{
			if (paginationParams == null)
			{
				paginationParams = new PaginationParams("", 0, 50);
			}
			object obj = new
			{
				limit = paginationParams.Limit,
				offset = paginationParams.Offset
			};
			string text = "data {id relationship_type user1 { id username } user2 {id username} }, " + paginationParams.PaginationQuery;
			return await GraphQlRequest.Request(QueryType.Query, "getBlockedUsers", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x000076B0 File Offset: 0x000058B0
		public async Task<GraphQlResult> SetRoleOnAssetToUserAsync(SerializableGuid assetId, int userId, int roleId, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = assetId,
				user_id = userId,
				role_id = roleId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "setRoleOnAssetToUser", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00007714 File Offset: 0x00005914
		public async Task<GraphQlResult> GetAllUsersWithRolesForAssetAsync(SerializableGuid assetId, SerializableGuid ancestorId = default(SerializableGuid), bool debugQuery = false)
		{
			object obj;
			if (!ancestorId.IsEmpty)
			{
				obj = new
				{
					asset_id = assetId,
					ancestor_asset_id_for_checking_rights = ancestorId.ToString()
				};
			}
			else
			{
				obj = new
				{
					asset_id = assetId
				};
			}
			string text = "user_id, inherited_from_parent, role { id }";
			return await GraphQlRequest.Request(QueryType.Query, "getAllUsersRolesForAsset", obj, text, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00007770 File Offset: 0x00005970
		public async Task<GraphQlResult> DeleteUserFromAssetAsync(int userId, SerializableGuid assetId, bool debugQuery = false)
		{
			object obj = new
			{
				asset_id = assetId,
				user_id = userId
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "deleteUserFromAsset", obj, null, this.AuthToken, debugQuery, 10, false);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x000077CC File Offset: 0x000059CC
		private async void SetupWebSocket()
		{
			await GraphQlRequest.InitializeWebsocket(this.AuthToken, delegate
			{
				this.SubscribeAllNotifications(new string[] { "assetupdated", "friendrequestsent", "friendrequestaccepted", "friendrequestrejected", "friendrequestcancelled" }, delegate
				{
					Debug.Log("Received Success from setting up websocket?");
				}, delegate(Exception e)
				{
					Debug.LogException(e);
				});
			}, new Action<WebSocketMessageId, WebSocketPayload>(this.OnWebSocketReceive));
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00007804 File Offset: 0x00005A04
		private void OnWebSocketReceive(WebSocketMessageId messageId, WebSocketPayload payload)
		{
			foreach (Action<WebSocketPayload> action in this.webSocketMessageIdDispatcher[messageId])
			{
				if (action != null)
				{
					action(payload);
				}
			}
		}

		// Token: 0x06000185 RID: 389 RVA: 0x00007864 File Offset: 0x00005A64
		public async void SubscribeAllNotifications(string[] notificationChannels, Action successCallback, Action<Exception> failureCallback)
		{
			GraphQlResult graphQlResult = await this.SubscribeAllNotificationsAsync(notificationChannels);
			if (!graphQlResult.HasErrors)
			{
				successCallback();
			}
			else
			{
				failureCallback(graphQlResult.GetErrorMessage(0));
			}
		}

		// Token: 0x06000186 RID: 390 RVA: 0x000078B4 File Offset: 0x00005AB4
		public async Task<GraphQlResult> SubscribeAllNotificationsAsync(string[] notificationChannels)
		{
			object obj = new
			{
				notification_types = notificationChannels.Select((string channel) => new EndlessCloudService.EntityTypeEnum(channel))
			};
			string text = "notification { id message notification_type_id object_type_id meta_data }";
			return await GraphQlRequest.Request(QueryType.Subscription, "subscribeAllNotifications", obj, text, this.AuthToken, true, 10, false);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00007900 File Offset: 0x00005B00
		public async void SubscribeToObjectNotifications(string instanceId, Action<object> successCallback, Action<Exception> failureCallback)
		{
			GraphQlResult graphQlResult = await this.SubscribeToObjectNotificationsAsync(instanceId);
			if (!graphQlResult.HasErrors)
			{
				object dataMember = graphQlResult.GetDataMember();
				if (successCallback != null)
				{
					successCallback(dataMember);
				}
			}
			else if (failureCallback != null)
			{
				failureCallback(graphQlResult.GetErrorMessage(0));
			}
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00007950 File Offset: 0x00005B50
		public async Task<GraphQlResult> SubscribeToObjectNotificationsAsync(string instanceId)
		{
			Debug.Log("Attempting to subscribe to asset id: " + instanceId);
			object obj = new
			{
				identifier = instanceId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset")
			};
			string text = "id object_id user_id muted_at";
			return await GraphQlRequest.Request(QueryType.Mutation, "subscribeToObjectNotifications", obj, text, this.AuthToken, true, 10, false);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x0000799C File Offset: 0x00005B9C
		public async void UnsubscribeToObjectNotifications(string instanceId, Action<object> successCallback, Action<Exception> failureCallback)
		{
			GraphQlResult graphQlResult = await this.UnsubscribeToObjectNotificationsAsync(instanceId);
			if (!graphQlResult.HasErrors)
			{
				object dataMember = graphQlResult.GetDataMember();
				if (successCallback != null)
				{
					successCallback(dataMember);
				}
			}
			else if (failureCallback != null)
			{
				failureCallback(graphQlResult.GetErrorMessage(0));
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x000079EC File Offset: 0x00005BEC
		public async Task<GraphQlResult> UnsubscribeToObjectNotificationsAsync(string instanceId)
		{
			object obj = new
			{
				identifier = instanceId,
				entity_type = new EndlessCloudService.EntityTypeEnum("asset")
			};
			return await GraphQlRequest.Request(QueryType.Mutation, "unsubscribeToObjectNotifications", obj, null, this.AuthToken, false, 10, false);
		}

		// Token: 0x0600018B RID: 395 RVA: 0x00007A37 File Offset: 0x00005C37
		public void AddWebSocketCallback(WebSocketMessageId messageId, Action<WebSocketPayload> assetUpdateCallback)
		{
			this.webSocketMessageIdDispatcher[messageId].Add(assetUpdateCallback);
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00007A4B File Offset: 0x00005C4B
		public void RemoveWebSocketCallback(WebSocketMessageId messageId, Action<WebSocketPayload> assetUpdateCallback)
		{
			this.webSocketMessageIdDispatcher[messageId].Remove(assetUpdateCallback);
		}

		// Token: 0x0400007A RID: 122
		private const string USER_AUTH_PLATFORM_ID_KEY = "USER_AUTH_PLATFORM_ID";

		// Token: 0x0400007B RID: 123
		private const string USER_AUTH_PLATFORM_TOKEN_KEY = "USER_AUTH_PLATFORM_TOKEN";

		// Token: 0x0400007C RID: 124
		private const string USER_AUTH_PLATFORM_PASS_KEY = "USER_AUTH_PLATFORM_PASS";

		// Token: 0x0400007D RID: 125
		private const string USER_AUTH_PLATFORM_TYPE_KEY = "USER_AUTH_PLATFORM_TYPE";

		// Token: 0x0400007E RID: 126
		public readonly string AuthToken;

		// Token: 0x04000084 RID: 132
		private Dictionary<WebSocketMessageId, List<Action<WebSocketPayload>>> webSocketMessageIdDispatcher = new Dictionary<WebSocketMessageId, List<Action<WebSocketPayload>>>();

		// Token: 0x0200003C RID: 60
		[JsonConverter(typeof(EndlessCloudService.EntityTypeEnumConverter))]
		public class EntityTypeEnum
		{
			// Token: 0x1700007B RID: 123
			// (get) Token: 0x06000194 RID: 404 RVA: 0x00007BB5 File Offset: 0x00005DB5
			public string Value { get; }

			// Token: 0x06000195 RID: 405 RVA: 0x00007BBD File Offset: 0x00005DBD
			public EntityTypeEnum(string type)
			{
				this.Value = type;
			}
		}

		// Token: 0x0200003D RID: 61
		public class EntityTypeEnumConverter : JsonConverter<EndlessCloudService.EntityTypeEnum>
		{
			// Token: 0x06000196 RID: 406 RVA: 0x00007BCC File Offset: 0x00005DCC
			public override void WriteJson(JsonWriter writer, EndlessCloudService.EntityTypeEnum value, JsonSerializer serializer)
			{
				writer.WriteRawValue(value.Value);
			}

			// Token: 0x06000197 RID: 407 RVA: 0x00007BDA File Offset: 0x00005DDA
			public override EndlessCloudService.EntityTypeEnum ReadJson(JsonReader reader, Type objectType, EndlessCloudService.EntityTypeEnum existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				return new EndlessCloudService.EntityTypeEnum(reader.Value as string);
			}
		}
	}
}
