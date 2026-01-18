using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000094 RID: 148
	public class UIUserGroupScreenBackground : UIScreenBackground, IValidatable
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000300 RID: 768 RVA: 0x00010244 File Offset: 0x0000E444
		public IReadOnlyList<Transform> CharacterPositions
		{
			get
			{
				return this.characterPositions;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000301 RID: 769 RVA: 0x0001024C File Offset: 0x0000E44C
		public IReadOnlyDictionary<ClientData, UIUserGroupScreenCharacter> CharacterDictionary
		{
			get
			{
				return this.characterDictionary;
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000302 RID: 770 RVA: 0x000082D0 File Offset: 0x000064D0
		private GroupInfo UserGroup
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000303 RID: 771 RVA: 0x00010254 File Offset: 0x0000E454
		private bool IsInUserGroup
		{
			get
			{
				return this.UserGroup != null;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000304 RID: 772 RVA: 0x00010260 File Offset: 0x0000E460
		private ClientData LocalClientData
		{
			get
			{
				return MatchmakingClientController.Instance.LocalClientData.Value;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000305 RID: 773 RVA: 0x0001027F File Offset: 0x0000E47F
		private bool LocalClientIsGroupHost
		{
			get
			{
				return this.IsInUserGroup && this.UserGroup.Host == this.LocalClientData.CoreData;
			}
		}

		// Token: 0x06000306 RID: 774 RVA: 0x000102A8 File Offset: 0x0000E4A8
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.characterPositions.Length != this.userGroupMaxSize.Value)
			{
				DebugUtility.LogError("UIUserGroupScreenBackground requires the same amount of entries in " + string.Format("{0} ({1}) as {2} ({3})!", new object[]
				{
					"characterPositions",
					this.characterPositions.Length,
					"userGroupMaxSize",
					this.userGroupMaxSize.Value
				}), this);
			}
		}

		// Token: 0x06000307 RID: 775 RVA: 0x00010338 File Offset: 0x0000E538
		public override void Display()
		{
			base.Display();
			MatchmakingClientController.GroupJoined += this.OnUserGroupJoined;
			MatchmakingClientController.GroupJoin += this.OnUserGroupJoin;
			MatchmakingClientController.GroupLeave += this.OnUserGroupLeave;
			MatchmakingClientController.GroupLeft += this.OnUserGroupLeft;
			if (this.IsInUserGroup)
			{
				this.AddEveryMember();
				return;
			}
			this.Add(this.LocalClientData);
		}

		// Token: 0x06000308 RID: 776 RVA: 0x000103AC File Offset: 0x0000E5AC
		public override void Hide()
		{
			base.Hide();
			MatchmakingClientController.GroupJoined -= this.OnUserGroupJoined;
			MatchmakingClientController.GroupJoin -= this.OnUserGroupJoin;
			MatchmakingClientController.GroupLeave -= this.OnUserGroupLeave;
			MatchmakingClientController.GroupLeft -= this.OnUserGroupLeft;
			this.Clean();
		}

		// Token: 0x06000309 RID: 777 RVA: 0x00010409 File Offset: 0x0000E609
		private void OnUserGroupJoined()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupJoined", Array.Empty<object>());
			}
			this.Clean();
			this.AddEveryMember();
		}

		// Token: 0x0600030A RID: 778 RVA: 0x00010430 File Offset: 0x0000E630
		private void OnUserGroupJoin(string joinedUserId)
		{
			ClientData clientData = new ClientData(joinedUserId, TargetPlatforms.Endless, "[userName]");
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupJoin", new object[] { clientData.ToPrettyString() });
			}
			this.Add(clientData);
		}

		// Token: 0x0600030B RID: 779 RVA: 0x00010474 File Offset: 0x0000E674
		private void OnUserGroupLeave(string userId)
		{
			ClientData clientData = new ClientData(userId, TargetPlatforms.Endless, "[userName]");
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupLeave", new object[] { clientData.ToPrettyString() });
			}
			if (clientData.CoreDataEquals(this.LocalClientData))
			{
				this.Clean();
				this.Add(this.LocalClientData);
				return;
			}
			this.Remove(clientData);
		}

		// Token: 0x0600030C RID: 780 RVA: 0x000104D9 File Offset: 0x0000E6D9
		private void OnUserGroupLeft()
		{
			this.OnUserGroupLeave(this.LocalClientData.CoreData.PlatformId);
		}

		// Token: 0x0600030D RID: 781 RVA: 0x000104F4 File Offset: 0x0000E6F4
		private void Add(ClientData clientData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Add", new object[] { clientData.ToPrettyString() });
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIUserGroupScreenCharacter uiuserGroupScreenCharacter = this.userGroupScreenCharacterSource;
			Transform transform = this.characterPositions[this.characterList.Count];
			UIUserGroupScreenCharacter uiuserGroupScreenCharacter2 = instance.Spawn<UIUserGroupScreenCharacter>(uiuserGroupScreenCharacter, default(Vector3), default(Quaternion), transform);
			uiuserGroupScreenCharacter2.transform.localPosition = Vector3.zero;
			uiuserGroupScreenCharacter2.transform.localEulerAngles = Vector3.zero;
			UIUserGroupScreenCharacter uiuserGroupScreenCharacter3;
			uiuserGroupScreenCharacter2.TryGetComponent<UIUserGroupScreenCharacter>(out uiuserGroupScreenCharacter3);
			bool flag = clientData.CoreDataEquals(this.LocalClientData);
			SerializableGuid serializableGuid = (flag ? CharacterCosmeticsDefinitionUtility.GetClientCharacterVisualId() : this.characterCosmeticsList.Cosmetics[0].AssetId);
			uiuserGroupScreenCharacter3.Display(serializableGuid, flag);
			this.characterList.Add(uiuserGroupScreenCharacter2);
			this.characterDictionary.Add(clientData, uiuserGroupScreenCharacter2);
			this.OnPlayerAdded.Invoke(clientData, uiuserGroupScreenCharacter2);
		}

		// Token: 0x0600030E RID: 782 RVA: 0x000105E4 File Offset: 0x0000E7E4
		private void Remove(ClientData clientData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Remove", new object[] { clientData.ToPrettyString() });
			}
			UIUserGroupScreenCharacter uiuserGroupScreenCharacter = this.characterDictionary[clientData];
			this.characterList.Remove(uiuserGroupScreenCharacter);
			this.characterDictionary.Remove(clientData);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIUserGroupScreenCharacter>(uiuserGroupScreenCharacter);
			this.OnPlayerRemoved.Invoke(clientData, uiuserGroupScreenCharacter);
		}

		// Token: 0x0600030F RID: 783 RVA: 0x00010654 File Offset: 0x0000E854
		private void AddEveryMember()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddEveryMember", Array.Empty<object>());
			}
			foreach (CoreClientData coreClientData in this.UserGroup.Members)
			{
				this.Add(new ClientData(coreClientData.PlatformId, coreClientData.Platform, "[userName]"));
			}
		}

		// Token: 0x06000310 RID: 784 RVA: 0x000106DC File Offset: 0x0000E8DC
		private void Clean()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clean", Array.Empty<object>());
			}
			List<ClientData> list = new List<ClientData>();
			foreach (KeyValuePair<ClientData, UIUserGroupScreenCharacter> keyValuePair in this.characterDictionary)
			{
				list.Add(keyValuePair.Key);
			}
			foreach (ClientData clientData in list)
			{
				this.Remove(clientData);
			}
		}

		// Token: 0x0400023B RID: 571
		public UnityEvent<ClientData, UIUserGroupScreenCharacter> OnPlayerAdded = new UnityEvent<ClientData, UIUserGroupScreenCharacter>();

		// Token: 0x0400023C RID: 572
		public UnityEvent<ClientData, UIUserGroupScreenCharacter> OnPlayerRemoved = new UnityEvent<ClientData, UIUserGroupScreenCharacter>();

		// Token: 0x0400023D RID: 573
		[Header("UIUserGroupScreenBackground")]
		[SerializeField]
		private IntVariable userGroupMaxSize;

		// Token: 0x0400023E RID: 574
		[SerializeField]
		private Transform[] characterPositions;

		// Token: 0x0400023F RID: 575
		[SerializeField]
		private UIUserGroupScreenCharacter userGroupScreenCharacterSource;

		// Token: 0x04000240 RID: 576
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x04000241 RID: 577
		private readonly List<UIUserGroupScreenCharacter> characterList = new List<UIUserGroupScreenCharacter>();

		// Token: 0x04000242 RID: 578
		private readonly Dictionary<ClientData, UIUserGroupScreenCharacter> characterDictionary = new Dictionary<ClientData, UIUserGroupScreenCharacter>();
	}
}
