using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Stats;

public abstract class StatBase
{
	public string Identifier = string.Empty;

	public LocalizedString Message = new LocalizedString();

	public int Order;

	public InventoryLibraryReference InventoryIcon;

	protected bool TryGetUserId(Context playerContext, out int userId)
	{
		if (playerContext.IsPlayer())
		{
			return NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerContext.WorldObject.NetworkObject.OwnerClientId, out userId);
		}
		userId = -1;
		return false;
	}

	internal void CopyFrom(StatBase statBase)
	{
		Identifier = statBase.Identifier;
		Message = statBase.Message;
		Order = statBase.Order;
		InventoryIcon = statBase.InventoryIcon;
	}

	internal static string GetFormattedString(float value, NumericDisplayFormat realFormat)
	{
		return realFormat switch
		{
			NumericDisplayFormat.Float => value.ToString("N2"), 
			NumericDisplayFormat.Int => Mathf.RoundToInt(value).ToString(), 
			NumericDisplayFormat.Time => FormatTimeSpan(TimeSpan.FromSeconds(value)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static string FormatTimeSpan(TimeSpan ts)
	{
		List<string> list = new List<string>();
		if (ts.Days > 0)
		{
			list.Add($"{ts.Days}d");
		}
		if (ts.Hours > 0)
		{
			list.Add($"{ts.Hours}h");
		}
		if (ts.Minutes > 0)
		{
			list.Add($"{ts.Minutes}m");
		}
		if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
		{
			list.Add($"{ts.TotalSeconds:F2}s");
		}
		else
		{
			list.Add($"{ts.Seconds}s");
		}
		return string.Join(":", list);
	}
}
