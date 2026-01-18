using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.RightsManagement
{
	// Token: 0x020005AA RID: 1450
	public class RightsManager : MonoBehaviourSingleton<RightsManager>
	{
		// Token: 0x060022DB RID: 8923 RVA: 0x0009F5D8 File Offset: 0x0009D7D8
		public async Task<GetAllRolesResult> GetAllUserRolesForAssetAsync(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> immediateReturn = null, bool forceRefresh = false)
		{
			return await this.GetAllUserRolesForAssetAsync(assetId, SerializableGuid.Empty, immediateReturn, forceRefresh);
		}

		// Token: 0x060022DC RID: 8924 RVA: 0x0009F634 File Offset: 0x0009D834
		public Task<GetAllRolesResult> GetAllUserRolesForAssetAsync(SerializableGuid assetId, SerializableGuid ancestorId, Action<IReadOnlyList<UserRole>> immediateReturn = null, bool forceRefresh = false)
		{
			RightsManager.<GetAllUserRolesForAssetAsync>d__4 <GetAllUserRolesForAssetAsync>d__;
			<GetAllUserRolesForAssetAsync>d__.<>t__builder = AsyncTaskMethodBuilder<GetAllRolesResult>.Create();
			<GetAllUserRolesForAssetAsync>d__.<>4__this = this;
			<GetAllUserRolesForAssetAsync>d__.assetId = assetId;
			<GetAllUserRolesForAssetAsync>d__.ancestorId = ancestorId;
			<GetAllUserRolesForAssetAsync>d__.immediateReturn = immediateReturn;
			<GetAllUserRolesForAssetAsync>d__.forceRefresh = forceRefresh;
			<GetAllUserRolesForAssetAsync>d__.<>1__state = -1;
			<GetAllUserRolesForAssetAsync>d__.<>t__builder.Start<RightsManager.<GetAllUserRolesForAssetAsync>d__4>(ref <GetAllUserRolesForAssetAsync>d__);
			return <GetAllUserRolesForAssetAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060022DD RID: 8925 RVA: 0x0009F698 File Offset: 0x0009D898
		public async Task<UserRoleRequestResult> HasRoleOrGreaterForAssetAsync(SerializableGuid assetId, int userId, Roles role, Action<bool> immediateReturn = null, bool forceRefresh = false)
		{
			return await this.HasRoleOrGreaterForAssetAsync(assetId, SerializableGuid.Empty, userId, role, immediateReturn, forceRefresh);
		}

		// Token: 0x060022DE RID: 8926 RVA: 0x0009F708 File Offset: 0x0009D908
		public Task<UserRoleRequestResult> HasRoleOrGreaterForAssetAsync(SerializableGuid assetId, SerializableGuid ancestorId, int userId, Roles role, Action<bool> immediateReturn = null, bool forceRefresh = false)
		{
			RightsManager.<HasRoleOrGreaterForAssetAsync>d__6 <HasRoleOrGreaterForAssetAsync>d__;
			<HasRoleOrGreaterForAssetAsync>d__.<>t__builder = AsyncTaskMethodBuilder<UserRoleRequestResult>.Create();
			<HasRoleOrGreaterForAssetAsync>d__.<>4__this = this;
			<HasRoleOrGreaterForAssetAsync>d__.assetId = assetId;
			<HasRoleOrGreaterForAssetAsync>d__.ancestorId = ancestorId;
			<HasRoleOrGreaterForAssetAsync>d__.userId = userId;
			<HasRoleOrGreaterForAssetAsync>d__.role = role;
			<HasRoleOrGreaterForAssetAsync>d__.immediateReturn = immediateReturn;
			<HasRoleOrGreaterForAssetAsync>d__.forceRefresh = forceRefresh;
			<HasRoleOrGreaterForAssetAsync>d__.<>1__state = -1;
			<HasRoleOrGreaterForAssetAsync>d__.<>t__builder.Start<RightsManager.<HasRoleOrGreaterForAssetAsync>d__6>(ref <HasRoleOrGreaterForAssetAsync>d__);
			return <HasRoleOrGreaterForAssetAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060022DF RID: 8927 RVA: 0x0009F780 File Offset: 0x0009D980
		private UserRoleCollection GetUserRoleCollectionForAssetId(SerializableGuid assetId)
		{
			UserRoleCollection userRoleCollection;
			if (!this.userRoleCollectionMap.TryGetValue(assetId, out userRoleCollection))
			{
				return null;
			}
			return userRoleCollection;
		}

		// Token: 0x060022E0 RID: 8928 RVA: 0x0009F7A0 File Offset: 0x0009D9A0
		private async Task<List<UserRole>> RequestRolesForAssetId(SerializableGuid assetId, SerializableGuid ancestorId, bool debugQuery = false)
		{
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAllUsersWithRolesForAssetAsync(assetId, ancestorId, debugQuery);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage(0);
			}
			return (from token in JsonConvert.DeserializeObject<JArray>(graphQlResult.GetDataMember().ToString())
				select new UserRole(token as JObject) into userRole
				where userRole.IsValid
				select userRole).ToList<UserRole>();
		}

		// Token: 0x060022E1 RID: 8929 RVA: 0x0009F7F4 File Offset: 0x0009D9F4
		private void ExecuteCallbacksForAsset(SerializableGuid assetId)
		{
			HashSet<Action<IReadOnlyList<UserRole>>> hashSet;
			if (!this.rightsChangedCallbacks.TryGetValue(assetId, out hashSet))
			{
				return;
			}
			UserRoleCollection userRoleCollectionForAssetId = this.GetUserRoleCollectionForAssetId(assetId);
			if (userRoleCollectionForAssetId == null)
			{
				return;
			}
			foreach (Action<IReadOnlyList<UserRole>> action in hashSet)
			{
				try
				{
					action(userRoleCollectionForAssetId.UserRoles.ToList<UserRole>());
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		// Token: 0x060022E2 RID: 8930 RVA: 0x0009F880 File Offset: 0x0009DA80
		public async Task<SetUserRoleForAssetResult> SetRightForUserOnAsset(SerializableGuid assetId, int userId, Roles targetRole)
		{
			bool isRemovingRole = targetRole == Roles.None;
			GraphQlResult graphQlResult;
			if (isRemovingRole)
			{
				graphQlResult = await EndlessServices.Instance.CloudService.DeleteUserFromAssetAsync(userId, assetId, false);
			}
			else
			{
				graphQlResult = await EndlessServices.Instance.CloudService.SetRoleOnAssetToUserAsync(assetId, userId, (int)targetRole, false);
			}
			SetUserRoleForAssetResult setUserRoleForAssetResult2;
			if (graphQlResult.HasErrors)
			{
				Exception errorMessage = graphQlResult.GetErrorMessage(0);
				SetUserRoleForAssetResult setUserRoleForAssetResult = default(SetUserRoleForAssetResult);
				string message = errorMessage.Message;
				SetUserRoleResult setUserRoleResult;
				if (message != null)
				{
					if (message.Contains("Unauthorized"))
					{
						setUserRoleResult = SetUserRoleResult.ErrorInsufficientUserRole;
						goto IL_01A1;
					}
					if (message.Contains("document not found"))
					{
						setUserRoleResult = SetUserRoleResult.ErrorAssetDoesNotExist;
						goto IL_01A1;
					}
				}
				setUserRoleResult = SetUserRoleResult.ErrorUnknown;
				IL_01A1:
				setUserRoleForAssetResult.Result = setUserRoleResult;
				setUserRoleForAssetResult.Error = errorMessage;
				setUserRoleForAssetResult2 = setUserRoleForAssetResult;
			}
			else
			{
				UserRoleCollection collection = this.GetUserRoleCollectionForAssetId(assetId);
				bool makeNewCollection = collection == null;
				if (makeNewCollection)
				{
					collection = new UserRoleCollection(assetId, SerializableGuid.Empty);
				}
				if (isRemovingRole)
				{
					if (collection.AncestorId == SerializableGuid.Empty)
					{
						collection.AddOrModifyUserRole(userId, Roles.None, false);
					}
					else
					{
						UserRole userRole = (await this.RequestRolesForAssetId(assetId, collection.AncestorId, false)).FirstOrDefault((UserRole role) => role.UserId == userId);
						if (userRole != null)
						{
							collection.AddOrModifyUserRole(userId, userRole.Role, true);
						}
					}
				}
				else
				{
					collection.AddOrModifyUserRole(userId, targetRole, false);
				}
				if (collection.IsStale() || makeNewCollection)
				{
					this.GetAllUserRolesForAssetAsync(assetId, null, false);
				}
				this.ExecuteCallbacksForAsset(assetId);
				setUserRoleForAssetResult2 = new SetUserRoleForAssetResult
				{
					Result = SetUserRoleResult.Success,
					Error = null
				};
			}
			return setUserRoleForAssetResult2;
		}

		// Token: 0x060022E3 RID: 8931 RVA: 0x0009F8DB File Offset: 0x0009DADB
		public void SubscribeToRoleChangeForAsset(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
		{
			this.SubscribeToRoleChangeForAsset(assetId, SerializableGuid.Empty, rightsChangedCallback);
		}

		// Token: 0x060022E4 RID: 8932 RVA: 0x0009F8EC File Offset: 0x0009DAEC
		public async void SubscribeToRoleChangeForAsset(SerializableGuid assetId, SerializableGuid ancestorId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
		{
			RightsManager.<>c__DisplayClass12_0 CS$<>8__locals1 = new RightsManager.<>c__DisplayClass12_0();
			CS$<>8__locals1.rightsChangedCallback = rightsChangedCallback;
			HashSet<Action<IReadOnlyList<UserRole>>> hashSet;
			if (!this.rightsChangedCallbacks.TryGetValue(assetId, out hashSet))
			{
				hashSet = new HashSet<Action<IReadOnlyList<UserRole>>>();
				this.rightsChangedCallbacks.Add(assetId, hashSet);
			}
			hashSet.Add(CS$<>8__locals1.rightsChangedCallback);
			CS$<>8__locals1.didImmediate = false;
			GetAllRolesResult getAllRolesResult = await this.GetAllUserRolesForAssetAsync(assetId, ancestorId, new Action<IReadOnlyList<UserRole>>(CS$<>8__locals1.<SubscribeToRoleChangeForAsset>g__ImmediateReturn|0), false);
			if (getAllRolesResult.WasChanged || !CS$<>8__locals1.didImmediate)
			{
				Action<IReadOnlyList<UserRole>> rightsChangedCallback2 = CS$<>8__locals1.rightsChangedCallback;
				if (rightsChangedCallback2 != null)
				{
					rightsChangedCallback2(getAllRolesResult.Roles);
				}
			}
		}

		// Token: 0x060022E5 RID: 8933 RVA: 0x0009F93C File Offset: 0x0009DB3C
		public void UnsubscribeToRoleChangeForAsset(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
		{
			HashSet<Action<IReadOnlyList<UserRole>>> hashSet;
			if (!this.rightsChangedCallbacks.TryGetValue(assetId, out hashSet))
			{
				return;
			}
			hashSet.Remove(rightsChangedCallback);
		}

		// Token: 0x04001BBA RID: 7098
		private Dictionary<SerializableGuid, UserRoleCollection> userRoleCollectionMap = new Dictionary<SerializableGuid, UserRoleCollection>();

		// Token: 0x04001BBB RID: 7099
		private Dictionary<SerializableGuid, HashSet<Action<IReadOnlyList<UserRole>>>> rightsChangedCallbacks = new Dictionary<SerializableGuid, HashSet<Action<IReadOnlyList<UserRole>>>>();

		// Token: 0x04001BBC RID: 7100
		private HashSet<SerializableGuid> inFlightRequests = new HashSet<SerializableGuid>();
	}
}
