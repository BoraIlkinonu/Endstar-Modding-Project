using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIPlayerReferenceManagerListModel : UIBaseLocalFilterableListModel<PlayerReferenceManager>
{
	protected override Comparison<PlayerReferenceManager> DefaultSort => (PlayerReferenceManager x, PlayerReferenceManager y) => x.UserSlot.CompareTo(y.UserSlot);

	protected void Initialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(NewPlayerRegistered);
		MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(PlayerUnregistered);
		List<ulong> currentPlayerGuids = MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids;
		List<PlayerReferenceManager> list = new List<PlayerReferenceManager>();
		foreach (ulong item in currentPlayerGuids)
		{
			PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(item);
			list.Add(playerObject);
		}
		Set(list, triggerEvents: true);
	}

	protected void Uninitialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Uninitialize");
		}
		MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(NewPlayerRegistered);
		MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.RemoveListener(PlayerUnregistered);
		Clear(triggerEvents: true);
	}

	private void NewPlayerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "NewPlayerRegistered", clientId, playerReferenceManager);
		}
		try
		{
			Add(playerReferenceManager, triggerEvents: true);
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}
	}

	private void PlayerUnregistered(ulong clientId, PlayerReferenceManager _)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "PlayerUnregistered", clientId);
		}
		try
		{
			int num = List.FindIndex((PlayerReferenceManager item) => item.OwnerClientId == clientId);
			if (num == -1)
			{
				Debug.LogException(new Exception(string.Format("Could not find {0} in a list of {1} with a {2} of {3}!", "PlayerReferenceManager", Count, "OwnerClientId", clientId)), this);
			}
			else
			{
				RemoveFilteredAt(num, triggerEvents: true);
			}
		}
		catch (Exception exception)
		{
			DebugUtility.LogException(exception, this);
		}
	}
}
