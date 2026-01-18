using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core
{
	// Token: 0x02000033 RID: 51
	public class UserSlotManager : MonoBehaviourSingleton<UserSlotManager>
	{
		// Token: 0x060000E9 RID: 233 RVA: 0x00006D94 File Offset: 0x00004F94
		private void Start()
		{
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdAdded.AddListener(new UnityAction<int>(this.HandleUserAdded));
			NetworkBehaviourSingleton<UserIdManager>.Instance.OnUserIdRemoved.AddListener(new UnityAction<int>(this.HandleUserRemoved));
			MatchSession.OnMatchSessionClose += this.HandleMatchSessionClosed;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x00006DE8 File Offset: 0x00004FE8
		private void HandleMatchSessionClosed(MatchSession obj)
		{
			this.userSlots.Clear();
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00006DF5 File Offset: 0x00004FF5
		protected override void OnDestroy()
		{
			base.OnDestroy();
			MatchSession.OnMatchSessionClose -= this.HandleMatchSessionClosed;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00006E10 File Offset: 0x00005010
		private void HandleUserAdded(int userId)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				UserSlotManager.UserSlot userSlot = new UserSlotManager.UserSlot
				{
					UserId = userId,
					IsValid = true
				};
				bool flag = false;
				for (int i = 0; i < this.userSlots.Count; i++)
				{
					if (this.userSlots[i].UserId == userId)
					{
						flag = true;
						this.userSlots[i] = userSlot;
					}
				}
				if (!flag)
				{
					for (int j = 0; j < this.userSlots.Count; j++)
					{
						if (!this.userSlots[j].IsValid)
						{
							flag = true;
							this.userSlots[j] = userSlot;
							break;
						}
					}
					if (!flag)
					{
						this.userSlots.Add(userSlot);
					}
				}
			}
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00006EC8 File Offset: 0x000050C8
		private void HandleUserRemoved(int userId)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				for (int i = 0; i < this.userSlots.Count; i++)
				{
					if (this.userSlots[i].UserId == userId)
					{
						this.userSlots[i].IsValid = false;
					}
				}
			}
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00006F20 File Offset: 0x00005120
		public int GetUserSlot(int userId)
		{
			int num = this.userSlots.FindIndex((UserSlotManager.UserSlot slot) => slot.UserId == userId);
			if (num == -1)
			{
				Debug.LogError("User slot requested before it was ready!");
			}
			return num;
		}

		// Token: 0x04000092 RID: 146
		private readonly List<UserSlotManager.UserSlot> userSlots = new List<UserSlotManager.UserSlot>();

		// Token: 0x02000034 RID: 52
		private class UserSlot
		{
			// Token: 0x04000093 RID: 147
			public int UserId;

			// Token: 0x04000094 RID: 148
			public bool IsValid = true;
		}
	}
}
