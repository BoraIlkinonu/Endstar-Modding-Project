using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.RightsManagement;

public class RightsManager : MonoBehaviourSingleton<RightsManager>
{
	private Dictionary<SerializableGuid, UserRoleCollection> userRoleCollectionMap = new Dictionary<SerializableGuid, UserRoleCollection>();

	private Dictionary<SerializableGuid, HashSet<Action<IReadOnlyList<UserRole>>>> rightsChangedCallbacks = new Dictionary<SerializableGuid, HashSet<Action<IReadOnlyList<UserRole>>>>();

	private HashSet<SerializableGuid> inFlightRequests = new HashSet<SerializableGuid>();

	public async Task<GetAllRolesResult> GetAllUserRolesForAssetAsync(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> immediateReturn = null, bool forceRefresh = false)
	{
		return await GetAllUserRolesForAssetAsync(assetId, SerializableGuid.Empty, immediateReturn, forceRefresh);
	}

	public async Task<GetAllRolesResult> GetAllUserRolesForAssetAsync(SerializableGuid assetId, SerializableGuid ancestorId, Action<IReadOnlyList<UserRole>> immediateReturn = null, bool forceRefresh = false)
	{
		GetAllRolesResult result = default(GetAllRolesResult);
		while (inFlightRequests.Contains(assetId))
		{
			await Task.Yield();
		}
		UserRoleCollection collection = GetUserRoleCollectionForAssetId(assetId);
		try
		{
			inFlightRequests.Add(assetId);
			if (collection == null)
			{
				collection = new UserRoleCollection(assetId, ancestorId);
				List<UserRole> list = await RequestRolesForAssetId(assetId, ancestorId, debugQuery: true);
				userRoleCollectionMap.Add(assetId, collection);
				collection.UpdateUserRoles(list);
				result.WasChanged = true;
				result.Roles = list;
				result.ChangedRoles = new List<RoleChange>();
				if (result.WasChanged)
				{
					ExecuteCallbacksForAsset(assetId);
				}
			}
			else if (forceRefresh || collection.IsStale())
			{
				IReadOnlyList<UserRole> obj = collection.UserRoles.ToList();
				immediateReturn?.Invoke(obj);
				List<UserRole> list2 = await RequestRolesForAssetId(assetId, ancestorId, debugQuery: true);
				collection = GetUserRoleCollectionForAssetId(assetId);
				List<RoleChange> list3 = collection.UpdateUserRoles(list2);
				result.WasChanged = list3.Count > 0;
				result.Roles = list2;
				result.ChangedRoles = list3;
				if (result.WasChanged)
				{
					Debug.Log("RightsManager.HasRoleOrGreaterForAssetAsync: Rights result has changed, executing callbacks");
					ExecuteCallbacksForAsset(assetId);
				}
			}
			else
			{
				result.WasChanged = false;
				result.Roles = collection.UserRoles;
				result.ChangedRoles = new List<RoleChange>();
			}
		}
		catch (Exception error)
		{
			result.WasChanged = false;
			result.Roles = collection.UserRoles;
			result.ChangedRoles = new List<RoleChange>();
			result.Error = error;
		}
		finally
		{
			inFlightRequests.Remove(assetId);
		}
		return result;
	}

	public async Task<UserRoleRequestResult> HasRoleOrGreaterForAssetAsync(SerializableGuid assetId, int userId, Roles role, Action<bool> immediateReturn = null, bool forceRefresh = false)
	{
		return await HasRoleOrGreaterForAssetAsync(assetId, SerializableGuid.Empty, userId, role, immediateReturn, forceRefresh);
	}

	public async Task<UserRoleRequestResult> HasRoleOrGreaterForAssetAsync(SerializableGuid assetId, SerializableGuid ancestorId, int userId, Roles role, Action<bool> immediateReturn = null, bool forceRefresh = false)
	{
		while (inFlightRequests.Contains(assetId))
		{
			await Task.Yield();
		}
		UserRoleCollection userRoleCollection = GetUserRoleCollectionForAssetId(assetId);
		UserRoleRequestResult result = default(UserRoleRequestResult);
		try
		{
			inFlightRequests.Add(assetId);
			if (userRoleCollection == null)
			{
				userRoleCollection = new UserRoleCollection(assetId, ancestorId);
				userRoleCollection.UpdateUserRoles(await RequestRolesForAssetId(assetId, ancestorId));
				if (!userRoleCollectionMap.ContainsKey(assetId))
				{
					userRoleCollectionMap.Add(assetId, userRoleCollection);
				}
				result.WasChanged = true;
				result.PassedCheck = userRoleCollection.UserHasRoleOrGreater(userId, role);
			}
			else if (forceRefresh || userRoleCollection.IsStale())
			{
				bool immediateResponse = userRoleCollection.UserHasRoleOrGreater(userId, role);
				immediateReturn?.Invoke(userRoleCollection.UserHasRoleOrGreater(userId, role));
				List<UserRole> newRoles = await RequestRolesForAssetId(assetId, ancestorId);
				userRoleCollection = GetUserRoleCollectionForAssetId(assetId);
				userRoleCollection.UpdateUserRoles(newRoles);
				result.PassedCheck = userRoleCollection.UserHasRoleOrGreater(userId, role);
				result.WasChanged = immediateResponse != result.PassedCheck;
				if (result.WasChanged)
				{
					Debug.Log("RightsManager.HasRoleOrGreaterForAssetAsync: Rights result has changed, executing callbacks");
					ExecuteCallbacksForAsset(assetId);
				}
			}
			else
			{
				result.PassedCheck = userRoleCollection.UserHasRoleOrGreater(userId, role);
				result.WasChanged = false;
			}
		}
		catch (Exception error)
		{
			result.PassedCheck = userRoleCollection.UserHasRoleOrGreater(userId, role);
			result.WasChanged = false;
			result.Error = error;
		}
		finally
		{
			inFlightRequests.Remove(assetId);
		}
		return result;
	}

	private UserRoleCollection GetUserRoleCollectionForAssetId(SerializableGuid assetId)
	{
		if (!userRoleCollectionMap.TryGetValue(assetId, out var value))
		{
			return null;
		}
		return value;
	}

	private async Task<List<UserRole>> RequestRolesForAssetId(SerializableGuid assetId, SerializableGuid ancestorId, bool debugQuery = false)
	{
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAllUsersWithRolesForAssetAsync(assetId, ancestorId, debugQuery);
		if (graphQlResult.HasErrors)
		{
			throw graphQlResult.GetErrorMessage();
		}
		return (from token in JsonConvert.DeserializeObject<JArray>(graphQlResult.GetDataMember().ToString())
			select new UserRole(token as JObject) into userRole
			where userRole.IsValid
			select userRole).ToList();
	}

	private void ExecuteCallbacksForAsset(SerializableGuid assetId)
	{
		if (!rightsChangedCallbacks.TryGetValue(assetId, out var value))
		{
			return;
		}
		UserRoleCollection userRoleCollectionForAssetId = GetUserRoleCollectionForAssetId(assetId);
		if (userRoleCollectionForAssetId == null)
		{
			return;
		}
		foreach (Action<IReadOnlyList<UserRole>> item in value)
		{
			try
			{
				item(userRoleCollectionForAssetId.UserRoles.ToList());
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public async Task<SetUserRoleForAssetResult> SetRightForUserOnAsset(SerializableGuid assetId, int userId, Roles targetRole)
	{
		bool isRemovingRole = targetRole == Roles.None;
		GraphQlResult graphQlResult = ((!isRemovingRole) ? (await EndlessServices.Instance.CloudService.SetRoleOnAssetToUserAsync(assetId, userId, (int)targetRole)) : (await EndlessServices.Instance.CloudService.DeleteUserFromAssetAsync(userId, assetId)));
		Exception errorMessage;
		SetUserRoleForAssetResult result;
		SetUserRoleResult result2;
		if (graphQlResult.HasErrors)
		{
			errorMessage = graphQlResult.GetErrorMessage();
			result = default(SetUserRoleForAssetResult);
			string message = errorMessage.Message;
			if (message == null)
			{
				goto IL_019e;
			}
			if (message.Contains("Unauthorized"))
			{
				result2 = SetUserRoleResult.ErrorInsufficientUserRole;
			}
			else
			{
				if (!message.Contains("document not found"))
				{
					goto IL_019e;
				}
				result2 = SetUserRoleResult.ErrorAssetDoesNotExist;
			}
			goto IL_01a1;
		}
		UserRoleCollection collection = GetUserRoleCollectionForAssetId(assetId);
		bool makeNewCollection = collection == null;
		if (makeNewCollection)
		{
			collection = new UserRoleCollection(assetId, SerializableGuid.Empty);
		}
		if (isRemovingRole)
		{
			if (collection.AncestorId == SerializableGuid.Empty)
			{
				collection.AddOrModifyUserRole(userId, Roles.None);
			}
			else
			{
				UserRole userRole = (await RequestRolesForAssetId(assetId, collection.AncestorId)).FirstOrDefault((UserRole role) => role.UserId == userId);
				if (userRole != null)
				{
					collection.AddOrModifyUserRole(userId, userRole.Role, isInherited: true);
				}
			}
		}
		else
		{
			collection.AddOrModifyUserRole(userId, targetRole);
		}
		if (collection.IsStale() || makeNewCollection)
		{
			GetAllUserRolesForAssetAsync(assetId);
		}
		ExecuteCallbacksForAsset(assetId);
		return new SetUserRoleForAssetResult
		{
			Result = SetUserRoleResult.Success,
			Error = null
		};
		IL_01a1:
		result.Result = result2;
		result.Error = errorMessage;
		return result;
		IL_019e:
		result2 = SetUserRoleResult.ErrorUnknown;
		goto IL_01a1;
	}

	public void SubscribeToRoleChangeForAsset(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
	{
		SubscribeToRoleChangeForAsset(assetId, SerializableGuid.Empty, rightsChangedCallback);
	}

	public async void SubscribeToRoleChangeForAsset(SerializableGuid assetId, SerializableGuid ancestorId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
	{
		if (!rightsChangedCallbacks.TryGetValue(assetId, out var value))
		{
			value = new HashSet<Action<IReadOnlyList<UserRole>>>();
			rightsChangedCallbacks.Add(assetId, value);
		}
		value.Add(rightsChangedCallback);
		bool didImmediate = false;
		GetAllRolesResult getAllRolesResult = await GetAllUserRolesForAssetAsync(assetId, ancestorId, ImmediateReturn);
		if (getAllRolesResult.WasChanged || !didImmediate)
		{
			rightsChangedCallback?.Invoke(getAllRolesResult.Roles);
		}
		void ImmediateReturn(IReadOnlyList<UserRole> list)
		{
			didImmediate = false;
			rightsChangedCallback(list);
		}
	}

	public void UnsubscribeToRoleChangeForAsset(SerializableGuid assetId, Action<IReadOnlyList<UserRole>> rightsChangedCallback)
	{
		if (rightsChangedCallbacks.TryGetValue(assetId, out var value))
		{
			value.Remove(rightsChangedCallback);
		}
	}
}
