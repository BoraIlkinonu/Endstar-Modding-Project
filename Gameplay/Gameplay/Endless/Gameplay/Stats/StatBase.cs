using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Stats
{
	// Token: 0x02000382 RID: 898
	public abstract class StatBase
	{
		// Token: 0x060016F8 RID: 5880 RVA: 0x0006B57C File Offset: 0x0006977C
		protected bool TryGetUserId(Context playerContext, out int userId)
		{
			if (playerContext.IsPlayer())
			{
				return NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerContext.WorldObject.NetworkObject.OwnerClientId, out userId);
			}
			userId = -1;
			return false;
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x0006B5A6 File Offset: 0x000697A6
		internal void CopyFrom(StatBase statBase)
		{
			this.Identifier = statBase.Identifier;
			this.Message = statBase.Message;
			this.Order = statBase.Order;
			this.InventoryIcon = statBase.InventoryIcon;
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x0006B5D8 File Offset: 0x000697D8
		internal static string GetFormattedString(float value, NumericDisplayFormat realFormat)
		{
			switch (realFormat)
			{
			case NumericDisplayFormat.Float:
				return value.ToString("N2");
			case NumericDisplayFormat.Int:
				return Mathf.RoundToInt(value).ToString();
			case NumericDisplayFormat.Time:
				return StatBase.FormatTimeSpan(TimeSpan.FromSeconds((double)value));
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x0006B628 File Offset: 0x00069828
		private static string FormatTimeSpan(TimeSpan ts)
		{
			List<string> list = new List<string>();
			if (ts.Days > 0)
			{
				list.Add(string.Format("{0}d", ts.Days));
			}
			if (ts.Hours > 0)
			{
				list.Add(string.Format("{0}h", ts.Hours));
			}
			if (ts.Minutes > 0)
			{
				list.Add(string.Format("{0}m", ts.Minutes));
			}
			if (ts.Days == 0 && ts.Hours == 0 && ts.Minutes == 0)
			{
				list.Add(string.Format("{0:F2}s", ts.TotalSeconds));
			}
			else
			{
				list.Add(string.Format("{0}s", ts.Seconds));
			}
			return string.Join(":", list);
		}

		// Token: 0x0400126E RID: 4718
		public string Identifier = string.Empty;

		// Token: 0x0400126F RID: 4719
		public LocalizedString Message = new LocalizedString();

		// Token: 0x04001270 RID: 4720
		public int Order;

		// Token: 0x04001271 RID: 4721
		public InventoryLibraryReference InventoryIcon;
	}
}
