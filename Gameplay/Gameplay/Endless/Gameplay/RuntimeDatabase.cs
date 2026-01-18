using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000292 RID: 658
	public class RuntimeDatabase : MonoBehaviourSingleton<RuntimeDatabase>
	{
		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000E9E RID: 3742 RVA: 0x0004DE00 File Offset: 0x0004C000
		public static UsableDefinition[] UsableDefinitions
		{
			get
			{
				return RuntimeDatabase.usableDefinitions;
			}
		}

		// Token: 0x06000E9F RID: 3743 RVA: 0x0004DE07 File Offset: 0x0004C007
		public static UsableDefinition GetUsableDefinition(SerializableGuid guid)
		{
			return RuntimeDatabase.usableDefinitionMap[guid];
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x0004DE14 File Offset: 0x0004C014
		public static T GetUsableDefinition<T>(SerializableGuid guid) where T : UsableDefinition
		{
			return (T)((object)RuntimeDatabase.usableDefinitionMap[guid]);
		}

		// Token: 0x170002C3 RID: 707
		// (get) Token: 0x06000EA1 RID: 3745 RVA: 0x0004DE26 File Offset: 0x0004C026
		// (set) Token: 0x06000EA2 RID: 3746 RVA: 0x0004DE2E File Offset: 0x0004C02E
		public Game ActiveGame { get; private set; }

		// Token: 0x06000EA3 RID: 3747 RVA: 0x0004DE38 File Offset: 0x0004C038
		private void Start()
		{
			UserNetworkVariableSerialization<Item>.ReadValue = new UserNetworkVariableSerialization<Item>.ReadValueDelegate(ItemSerialization.ReadValueSafe);
			UserNetworkVariableSerialization<Item>.WriteValue = new UserNetworkVariableSerialization<Item>.WriteValueDelegate(ItemSerialization.WriteValueSafe);
			UserNetworkVariableSerialization<Item>.DuplicateValue = delegate(in Item item, ref Item item2)
			{
				item2 = item;
			};
			RuntimeDatabase.usableDefinitions = Resources.LoadAll<UsableDefinition>(this.usableDefinitionResourcePath);
			Debug.LogFormat("Loaded [{0}] Usable Definitions from resource path: {1}", new object[]
			{
				RuntimeDatabase.usableDefinitions.Length,
				this.usableDefinitionResourcePath
			});
			foreach (UsableDefinition usableDefinition in RuntimeDatabase.usableDefinitions)
			{
				RuntimeDatabase.usableDefinitionMap.Add(usableDefinition.Guid, usableDefinition);
			}
			foreach (TeleportInfo teleportInfo in this.teleportInfoList)
			{
				RuntimeDatabase.teleportInfoDictionary.TryAdd(teleportInfo.TeleportType, teleportInfo);
			}
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x0004DF40 File Offset: 0x0004C140
		public void CacheUser(User user)
		{
			this.cachedUserNames.TryAdd(user.Id, user.UserName);
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x0004DF5C File Offset: 0x0004C15C
		public static SerializableGuid GetUsableDefinitionIDFromAssetID(SerializableGuid assetID)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out runtimePropInfo))
			{
				Item componentInChildren = runtimePropInfo.EndlessProp.GetComponentInChildren<Item>();
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

		// Token: 0x06000EA6 RID: 3750 RVA: 0x0004DFB8 File Offset: 0x0004C1B8
		public static InventoryUsableDefinition GetUsableDefinitionFromItemAssetID(SerializableGuid assetID)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out runtimePropInfo))
			{
				Item componentInChildren = runtimePropInfo.EndlessProp.GetComponentInChildren<Item>();
				if (componentInChildren != null)
				{
					return componentInChildren.InventoryUsableDefinition;
				}
			}
			return null;
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x0004DFF6 File Offset: 0x0004C1F6
		public void SetGame(Game game)
		{
			this.ActiveGame = game;
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x0004DFFF File Offset: 0x0004C1FF
		public void ClearActiveGame()
		{
			this.previousGame = this.ActiveGame;
			this.ActiveGame = null;
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x0004E014 File Offset: 0x0004C214
		public bool RestoreActiveGame(string assetId, string version)
		{
			if (this.previousGame != null && this.previousGame.AssetID == assetId && this.previousGame.AssetVersion == version)
			{
				this.SetGame(this.previousGame);
				this.previousGame = null;
				return true;
			}
			return false;
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x0004E068 File Offset: 0x0004C268
		public async Task<string> GetUserName(int userId)
		{
			string text;
			if (!this.cachedUserNames.ContainsKey(userId) && !this.pendingUserNameCallbacks.ContainsKey(userId))
			{
				this.pendingUserNameCallbacks.Add(userId, new List<Action<string>>());
				text = await this.RequestUserName(userId);
			}
			else
			{
				while (!this.cachedUserNames.ContainsKey(userId) && !ExitManager.IsQuitting)
				{
					await Task.Yield();
				}
				if (ExitManager.IsQuitting)
				{
					text = string.Empty;
				}
				else
				{
					text = this.cachedUserNames[userId];
				}
			}
			return text;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x0004E0B4 File Offset: 0x0004C2B4
		public async Task<string> GetUserNameAsync(int userId, CancellationToken cancellationToken)
		{
			TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
			string text;
			using (cancellationToken.Register(delegate
			{
				taskCompletionSource.TrySetCanceled();
			}))
			{
				Task<string> userNameTask = this.GetUserName(userId);
				TaskAwaiter<Task<string>> taskAwaiter = Task.WhenAny<string>(new Task<string>[] { userNameTask, taskCompletionSource.Task }).GetAwaiter();
				if (!taskAwaiter.IsCompleted)
				{
					await taskAwaiter;
					TaskAwaiter<Task<string>> taskAwaiter2;
					taskAwaiter = taskAwaiter2;
					taskAwaiter2 = default(TaskAwaiter<Task<string>>);
				}
				if (taskAwaiter.GetResult() == taskCompletionSource.Task)
				{
					throw new OperationCanceledException();
				}
				text = await userNameTask;
			}
			return text;
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x0004E108 File Offset: 0x0004C308
		private async Task<string> RequestUserName(int userId)
		{
			User dataMember = (await EndlessServices.Instance.CloudService.GetUserById(userId, true)).GetDataMember<User>();
			this.cachedUserNames[userId] = dataMember.UserName;
			if (!ExitManager.IsQuitting)
			{
				foreach (Action<string> action in this.pendingUserNameCallbacks[userId])
				{
					action(dataMember.UserName);
				}
				this.pendingUserNameCallbacks.Remove(userId);
			}
			return dataMember.UserName;
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x0004E154 File Offset: 0x0004C354
		public static TeleportInfo GetTeleportInfo(TeleportType type)
		{
			TeleportInfo teleportInfo;
			if (RuntimeDatabase.teleportInfoDictionary.TryGetValue(type, out teleportInfo))
			{
				return teleportInfo;
			}
			return null;
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x0004E174 File Offset: 0x0004C374
		public string GetUserNameSynchronous(int userId)
		{
			string text;
			if (this.cachedUserNames.TryGetValue(userId, out text))
			{
				return text;
			}
			return "Player";
		}

		// Token: 0x04000D16 RID: 3350
		[SerializeField]
		private string usableDefinitionResourcePath;

		// Token: 0x04000D17 RID: 3351
		protected static Dictionary<SerializableGuid, UsableDefinition> usableDefinitionMap = new Dictionary<SerializableGuid, UsableDefinition>();

		// Token: 0x04000D18 RID: 3352
		protected static Dictionary<TeleportType, TeleportInfo> teleportInfoDictionary = new Dictionary<TeleportType, TeleportInfo>();

		// Token: 0x04000D19 RID: 3353
		protected static UsableDefinition[] usableDefinitions = new UsableDefinition[0];

		// Token: 0x04000D1A RID: 3354
		[SerializeField]
		private List<TeleportInfo> teleportInfoList = new List<TeleportInfo>();

		// Token: 0x04000D1B RID: 3355
		private Dictionary<int, string> cachedUserNames = new Dictionary<int, string>();

		// Token: 0x04000D1C RID: 3356
		private Dictionary<int, List<Action<string>>> pendingUserNameCallbacks = new Dictionary<int, List<Action<string>>>();

		// Token: 0x04000D1E RID: 3358
		private Game previousGame;
	}
}
