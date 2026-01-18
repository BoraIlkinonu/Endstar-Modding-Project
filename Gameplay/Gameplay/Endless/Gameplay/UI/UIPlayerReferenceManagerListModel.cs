using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C3 RID: 963
	public class UIPlayerReferenceManagerListModel : UIBaseLocalFilterableListModel<PlayerReferenceManager>
	{
		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x06001886 RID: 6278 RVA: 0x00071F95 File Offset: 0x00070195
		protected override Comparison<PlayerReferenceManager> DefaultSort
		{
			get
			{
				return (PlayerReferenceManager x, PlayerReferenceManager y) => x.UserSlot.CompareTo(y.UserSlot);
			}
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x00071FB8 File Offset: 0x000701B8
		protected void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.NewPlayerRegistered));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.PlayerUnregistered));
			List<ulong> currentPlayerGuids = MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids;
			List<PlayerReferenceManager> list = new List<PlayerReferenceManager>();
			foreach (ulong num in currentPlayerGuids)
			{
				PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(num);
				list.Add(playerObject);
			}
			this.Set(list, true);
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x00072078 File Offset: 0x00070278
		protected void Uninitialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Uninitialize", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.NewPlayerRegistered));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.PlayerUnregistered));
			this.Clear(true);
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x000720DC File Offset: 0x000702DC
		private void NewPlayerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "NewPlayerRegistered", new object[] { clientId, playerReferenceManager });
			}
			try
			{
				this.Add(playerReferenceManager, true);
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x00072134 File Offset: 0x00070334
		private void PlayerUnregistered(ulong clientId, PlayerReferenceManager _)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayerUnregistered", new object[] { clientId });
			}
			try
			{
				int num = this.List.FindIndex((PlayerReferenceManager item) => item.OwnerClientId == clientId);
				if (num == -1)
				{
					Debug.LogException(new Exception(string.Format("Could not find {0} in a list of {1} with a {2} of {3}!", new object[] { "PlayerReferenceManager", this.Count, "OwnerClientId", clientId })), this);
				}
				else
				{
					base.RemoveFilteredAt(num, true);
				}
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
		}
	}
}
