using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class RuntimeDatabase : MonoBehaviourSingleton<RuntimeDatabase>
{
	[SerializeField]
	private string usableDefinitionResourcePath;

	protected static Dictionary<SerializableGuid, UsableDefinition> usableDefinitionMap = new Dictionary<SerializableGuid, UsableDefinition>();

	protected static Dictionary<TeleportType, TeleportInfo> teleportInfoDictionary = new Dictionary<TeleportType, TeleportInfo>();

	protected static UsableDefinition[] usableDefinitions = new UsableDefinition[0];

	[SerializeField]
	private List<TeleportInfo> teleportInfoList = new List<TeleportInfo>();

	private Dictionary<int, string> cachedUserNames = new Dictionary<int, string>();

	private Dictionary<int, List<Action<string>>> pendingUserNameCallbacks = new Dictionary<int, List<Action<string>>>();

	private Game previousGame;

	public static UsableDefinition[] UsableDefinitions => usableDefinitions;

	public Game ActiveGame { get; private set; }

	public static UsableDefinition GetUsableDefinition(SerializableGuid guid)
	{
		return usableDefinitionMap[guid];
	}

	public static T GetUsableDefinition<T>(SerializableGuid guid) where T : UsableDefinition
	{
		return (T)usableDefinitionMap[guid];
	}

	private void Start()
	{
		UserNetworkVariableSerialization<Item>.ReadValue = ItemSerialization.ReadValueSafe;
		UserNetworkVariableSerialization<Item>.WriteValue = ItemSerialization.WriteValueSafe;
		UserNetworkVariableSerialization<Item>.DuplicateValue = delegate(in Item item, ref Item item2)
		{
			item2 = item;
		};
		usableDefinitions = Resources.LoadAll<UsableDefinition>(usableDefinitionResourcePath);
		Debug.LogFormat("Loaded [{0}] Usable Definitions from resource path: {1}", usableDefinitions.Length, usableDefinitionResourcePath);
		UsableDefinition[] array = usableDefinitions;
		foreach (UsableDefinition usableDefinition in array)
		{
			usableDefinitionMap.Add(usableDefinition.Guid, usableDefinition);
		}
		foreach (TeleportInfo teleportInfo in teleportInfoList)
		{
			teleportInfoDictionary.TryAdd(teleportInfo.TeleportType, teleportInfo);
		}
	}

	public void CacheUser(User user)
	{
		cachedUserNames.TryAdd(user.Id, user.UserName);
	}

	public static SerializableGuid GetUsableDefinitionIDFromAssetID(SerializableGuid assetID)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out var metadata))
		{
			Item componentInChildren = metadata.EndlessProp.GetComponentInChildren<Item>();
			if (componentInChildren != null)
			{
				if (!componentInChildren.InventoryUsableDefinition)
				{
					return SerializableGuid.Empty;
				}
				return componentInChildren.InventoryUsableDefinition.Guid;
			}
		}
		return SerializableGuid.Empty;
	}

	public static InventoryUsableDefinition GetUsableDefinitionFromItemAssetID(SerializableGuid assetID)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out var metadata))
		{
			Item componentInChildren = metadata.EndlessProp.GetComponentInChildren<Item>();
			if (componentInChildren != null)
			{
				return componentInChildren.InventoryUsableDefinition;
			}
		}
		return null;
	}

	public void SetGame(Game game)
	{
		ActiveGame = game;
	}

	public void ClearActiveGame()
	{
		previousGame = ActiveGame;
		ActiveGame = null;
	}

	public bool RestoreActiveGame(string assetId, string version)
	{
		if (previousGame != null && previousGame.AssetID == assetId && previousGame.AssetVersion == version)
		{
			SetGame(previousGame);
			previousGame = null;
			return true;
		}
		return false;
	}

	public async Task<string> GetUserName(int userId)
	{
		if (!cachedUserNames.ContainsKey(userId) && !pendingUserNameCallbacks.ContainsKey(userId))
		{
			pendingUserNameCallbacks.Add(userId, new List<Action<string>>());
			return await RequestUserName(userId);
		}
		while (!cachedUserNames.ContainsKey(userId) && !ExitManager.IsQuitting)
		{
			await Task.Yield();
		}
		if (ExitManager.IsQuitting)
		{
			return string.Empty;
		}
		return cachedUserNames[userId];
	}

	public async Task<string> GetUserNameAsync(int userId, CancellationToken cancellationToken)
	{
		TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
		using (cancellationToken.Register(delegate
		{
			taskCompletionSource.TrySetCanceled();
		}))
		{
			Task<string> userNameTask = GetUserName(userId);
			if (await Task.WhenAny<string>(userNameTask, taskCompletionSource.Task) == taskCompletionSource.Task)
			{
				throw new OperationCanceledException();
			}
			return await userNameTask;
		}
	}

	private async Task<string> RequestUserName(int userId)
	{
		User dataMember = (await EndlessServices.Instance.CloudService.GetUserById(userId, debugQuery: true)).GetDataMember<User>();
		cachedUserNames[userId] = dataMember.UserName;
		if (!ExitManager.IsQuitting)
		{
			foreach (Action<string> item in pendingUserNameCallbacks[userId])
			{
				item(dataMember.UserName);
			}
			pendingUserNameCallbacks.Remove(userId);
		}
		return dataMember.UserName;
	}

	public static TeleportInfo GetTeleportInfo(TeleportType type)
	{
		if (teleportInfoDictionary.TryGetValue(type, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetUserNameSynchronous(int userId)
	{
		if (cachedUserNames.TryGetValue(userId, out var value))
		{
			return value;
		}
		return "Player";
	}
}
