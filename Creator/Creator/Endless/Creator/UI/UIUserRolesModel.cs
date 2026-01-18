using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002B2 RID: 690
	public class UIUserRolesModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06000B9C RID: 2972 RVA: 0x00036954 File Offset: 0x00034B54
		// (set) Token: 0x06000B9D RID: 2973 RVA: 0x0003695C File Offset: 0x00034B5C
		public AssetContexts AssetContext { get; private set; } = AssetContexts.GameInspectorPlay;

		// Token: 0x1700017B RID: 379
		// (get) Token: 0x06000B9E RID: 2974 RVA: 0x00036965 File Offset: 0x00034B65
		// (set) Token: 0x06000B9F RID: 2975 RVA: 0x0003696D File Offset: 0x00034B6D
		public UIUserRoleWizard.AssetTypes AssetType { get; private set; }

		// Token: 0x1700017C RID: 380
		// (get) Token: 0x06000BA0 RID: 2976 RVA: 0x00036976 File Offset: 0x00034B76
		// (set) Token: 0x06000BA1 RID: 2977 RVA: 0x0003697E File Offset: 0x00034B7E
		public SerializableGuid AssetId { get; private set; }

		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000BA2 RID: 2978 RVA: 0x00036987 File Offset: 0x00034B87
		// (set) Token: 0x06000BA3 RID: 2979 RVA: 0x0003698F File Offset: 0x00034B8F
		public string AssetName { get; private set; }

		// Token: 0x1700017E RID: 382
		// (get) Token: 0x06000BA4 RID: 2980 RVA: 0x00036998 File Offset: 0x00034B98
		// (set) Token: 0x06000BA5 RID: 2981 RVA: 0x000369A0 File Offset: 0x00034BA0
		public Roles LocalClientRole { get; private set; } = Roles.None;

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000BA6 RID: 2982 RVA: 0x000369A9 File Offset: 0x00034BA9
		public IReadOnlyCollection<int> UserIds
		{
			get
			{
				return this.userIds;
			}
		}

		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000BA7 RID: 2983 RVA: 0x000369B1 File Offset: 0x00034BB1
		public IReadOnlyCollection<User> Users
		{
			get
			{
				return this.users;
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000BA8 RID: 2984 RVA: 0x000369B9 File Offset: 0x00034BB9
		public IReadOnlyCollection<UserRole> UserRoles
		{
			get
			{
				return this.userRoleListModel.UserRolesModel.userRoleListModel.ReadOnlyList;
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000BA9 RID: 2985 RVA: 0x000369D0 File Offset: 0x00034BD0
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000BAA RID: 2986 RVA: 0x000369D8 File Offset: 0x00034BD8
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000BAB RID: 2987 RVA: 0x000369E0 File Offset: 0x00034BE0
		public void Initialize(SerializableGuid assetId, string assetName, SerializableGuid ancestorId, AssetContexts context)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { assetId, assetName, ancestorId, context });
			}
			this.AssetContext = context;
			if (!this.AssetId.IsEmpty && this.AssetId != assetId)
			{
				this.Clear();
			}
			this.AssetId = assetId;
			this.AssetName = assetName;
			if (this.AssetContext == AssetContexts.NewGame)
			{
				this.LocalClientRole = Roles.Owner;
				int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
				UserRole userRole = new UserRole(activeUserId, this.LocalClientRole);
				List<UserRole> list = new List<UserRole> { userRole };
				this.userRoleListModel.Set(list, true);
				this.userIds.Clear();
				this.userIds.Add(activeUserId);
				this.users.Clear();
				this.OnUserRolesSet.Invoke(list);
				this.OnLocalClientRoleSet.Invoke(this.LocalClientRole);
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(assetId, new Action<IReadOnlyList<UserRole>>(this.OnRoleChanged));
			MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(assetId, new Action<IReadOnlyList<UserRole>>(this.OnRoleChanged), false);
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x00036B1C File Offset: 0x00034D1C
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				string.Format("{0}: {1}, ", "userRoleListModel", this.userRoleListModel.Count),
				string.Format("{0}: {1}, ", "AssetId", this.AssetId),
				string.Format("{0}: {1}, ", "LocalClientRole", this.LocalClientRole),
				string.Format("{0}: {1}, ", "userIds", this.userIds.Count),
				string.Format("{0}: {1}, ", "AssetContext", this.AssetContext),
				string.Format("{0}: {1}", "AssetType", this.AssetType)
			});
		}

		// Token: 0x06000BAD RID: 2989 RVA: 0x00036BEC File Offset: 0x00034DEC
		public void AddToEnd(UserRole newUserRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddToEnd", new object[] { newUserRole });
			}
			this.AddToEndAsync(newUserRole);
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x00036C14 File Offset: 0x00034E14
		private Task AddToEndAsync(UserRole newUserRole)
		{
			UIUserRolesModel.<AddToEndAsync>d__45 <AddToEndAsync>d__;
			<AddToEndAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<AddToEndAsync>d__.<>4__this = this;
			<AddToEndAsync>d__.newUserRole = newUserRole;
			<AddToEndAsync>d__.<>1__state = -1;
			<AddToEndAsync>d__.<>t__builder.Start<UIUserRolesModel.<AddToEndAsync>d__45>(ref <AddToEndAsync>d__);
			return <AddToEndAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x00036C60 File Offset: 0x00034E60
		public void UpdateRole(UserRole newUserRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateRole", new object[] { newUserRole });
			}
			int num = this.userRoleListModel.IndexOf(newUserRole.UserId);
			if (num > -1)
			{
				this.userRoleListModel.SetItem(num, newUserRole, true);
				return;
			}
			DebugUtility.LogError("Could not find newUserRole in userRoleListModel!", this);
		}

		// Token: 0x06000BB0 RID: 2992 RVA: 0x00036CBC File Offset: 0x00034EBC
		public void RemoveUser(UserRole target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveUser", new object[] { target });
			}
			this.userIds.Remove(target.UserId);
			int num = this.IndexOf(this.users, target.UserId);
			if (num > -1)
			{
				this.users.RemoveAt(num);
			}
			else
			{
				DebugUtility.LogError("Could not find target in users!", this);
			}
			int num2 = this.userRoleListModel.IndexOf(target.UserId);
			if (num2 > -1)
			{
				this.userRoleListModel.RemoveAt(num2, true);
				return;
			}
			DebugUtility.LogError("Could not find target in userRoleListModel!", this);
		}

		// Token: 0x06000BB1 RID: 2993 RVA: 0x00036D57 File Offset: 0x00034F57
		public void SetAssetType(UIUserRoleWizard.AssetTypes assetType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAssetType", new object[] { assetType });
			}
			this.AssetType = assetType;
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x00036D82 File Offset: 0x00034F82
		public void SetAssetName(string assetName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetAssetName", new object[] { assetName });
			}
			this.AssetName = assetName;
		}

		// Token: 0x06000BB3 RID: 2995 RVA: 0x00036DA8 File Offset: 0x00034FA8
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.userRoleListModel.Clear(true);
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.AssetId, new Action<IReadOnlyList<UserRole>>(this.OnRoleChanged));
			this.SetUserRoles(new List<UserRole>());
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.addToEndAsyncCancellationTokenSource);
		}

		// Token: 0x06000BB4 RID: 2996 RVA: 0x00036E0B File Offset: 0x0003500B
		private void SetUserRoles(List<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUserRoles", new object[] { userRoles.Count });
				DebugUtility.DebugEnumerable<UserRole>("userRoles", userRoles, this);
			}
			this.SetUserRolesAsync(userRoles);
		}

		// Token: 0x06000BB5 RID: 2997 RVA: 0x00036E48 File Offset: 0x00035048
		private Task SetUserRolesAsync(List<UserRole> userRoles)
		{
			UIUserRolesModel.<SetUserRolesAsync>d__52 <SetUserRolesAsync>d__;
			<SetUserRolesAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<SetUserRolesAsync>d__.<>4__this = this;
			<SetUserRolesAsync>d__.userRoles = userRoles;
			<SetUserRolesAsync>d__.<>1__state = -1;
			<SetUserRolesAsync>d__.<>t__builder.Start<UIUserRolesModel.<SetUserRolesAsync>d__52>(ref <SetUserRolesAsync>d__);
			return <SetUserRolesAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000BB6 RID: 2998 RVA: 0x00036E94 File Offset: 0x00035094
		private void OnRoleChanged(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRoleChanged", new object[] { userRoles.Count });
			}
			List<UserRole> list = userRoles.ToList<UserRole>();
			this.SetUserRoles(list);
		}

		// Token: 0x06000BB7 RID: 2999 RVA: 0x00036ED8 File Offset: 0x000350D8
		private int IndexOf(List<User> usersList, int userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IndexOf", new object[]
				{
					usersList.Count<User>(),
					userId
				});
			}
			for (int i = 0; i < usersList.Count; i++)
			{
				if (usersList[i].Id == userId)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x040009CD RID: 2509
		public UnityEvent<List<UserRole>> OnUserRolesSet = new UnityEvent<List<UserRole>>();

		// Token: 0x040009CE RID: 2510
		public UnityEvent<Roles> OnLocalClientRoleSet = new UnityEvent<Roles>();

		// Token: 0x040009CF RID: 2511
		[SerializeField]
		private UIUserRolesModel.Filters filter;

		// Token: 0x040009D0 RID: 2512
		[SerializeField]
		private UIUserRoleListModel userRoleListModel;

		// Token: 0x040009D1 RID: 2513
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040009D2 RID: 2514
		private readonly HashSet<int> userIds = new HashSet<int>();

		// Token: 0x040009D3 RID: 2515
		private readonly List<User> users = new List<User>();

		// Token: 0x040009D4 RID: 2516
		private CancellationTokenSource addToEndAsyncCancellationTokenSource;

		// Token: 0x040009D5 RID: 2517
		private CancellationTokenSource setUserRolesAsyncCancellationTokenSource;

		// Token: 0x020002B3 RID: 691
		public enum Filters
		{
			// Token: 0x040009DE RID: 2526
			None,
			// Token: 0x040009DF RID: 2527
			NotInheritedFromParent,
			// Token: 0x040009E0 RID: 2528
			InheritedFromParent
		}
	}
}
