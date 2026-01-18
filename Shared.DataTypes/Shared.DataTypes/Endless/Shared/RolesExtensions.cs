using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x0200000B RID: 11
	public static class RolesExtensions
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00003135 File Offset: 0x00001335
		public static bool IsGreaterThan(this Roles a, Roles b)
		{
			return a.Level() > b.Level();
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003145 File Offset: 0x00001345
		public static bool IsGreaterThanOrEqualTo(this Roles a, Roles b)
		{
			return a.Level() > b.Level() || a.Level() == b.Level();
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003165 File Offset: 0x00001365
		public static bool IsLessThan(this Roles a, Roles b)
		{
			return a.Level() < b.Level();
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003175 File Offset: 0x00001375
		public static bool IsLessThanOrEqualTo(this Roles a, Roles b)
		{
			return a.Level() < b.Level() || a.Level() == b.Level();
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003198 File Offset: 0x00001398
		public static int Level(this Roles role)
		{
			int num;
			if (RolesExtensions.RoleLevels.TryGetValue(role, out num))
			{
				return num;
			}
			Debug.LogError(string.Format("There is no support for a {0} of {1}!", "Roles", role));
			return 0;
		}

		// Token: 0x04000022 RID: 34
		private static readonly Dictionary<Roles, int> RoleLevels = new Dictionary<Roles, int>
		{
			{
				Roles.None,
				0
			},
			{
				Roles.Viewer,
				1
			},
			{
				Roles.Editor,
				2
			},
			{
				Roles.Publisher,
				3
			},
			{
				Roles.Owner,
				4
			}
		};
	}
}
