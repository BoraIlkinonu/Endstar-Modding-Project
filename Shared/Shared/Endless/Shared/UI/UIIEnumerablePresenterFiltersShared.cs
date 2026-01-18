using System;
using System.Collections.Generic;
using System.Globalization;
using Endless.Shared.DataTypes;
using Endless.Shared.Social;

namespace Endless.Shared.UI
{
	// Token: 0x020001F3 RID: 499
	public static class UIIEnumerablePresenterFiltersShared
	{
		// Token: 0x06000C86 RID: 3206 RVA: 0x000367C8 File Offset: 0x000349C8
		static UIIEnumerablePresenterFiltersShared()
		{
			UIIEnumerablePresenterFiltersShared.RegisterIntegralTypeFilters(new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterIntegralType));
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x000368D0 File Offset: 0x00034AD0
		private static void RegisterIntegralTypeFilters(Func<string, object, bool> filterMethod)
		{
			foreach (Type type in new List<Type>
			{
				typeof(int),
				typeof(short),
				typeof(long),
				typeof(byte),
				typeof(sbyte),
				typeof(ushort),
				typeof(uint),
				typeof(ulong)
			})
			{
				UIIEnumerablePresenterFiltersShared.typeFilterDictionary.TryAdd(type, filterMethod);
			}
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x000369A8 File Offset: 0x00034BA8
		public static void Register()
		{
			foreach (KeyValuePair<Type, Func<string, object, bool>> keyValuePair in UIIEnumerablePresenterFiltersShared.typeFilterDictionary)
			{
				UIIEnumerablePresenter.RegisterTypeFilter(keyValuePair.Key, keyValuePair.Value);
			}
		}

		// Token: 0x06000C89 RID: 3209 RVA: 0x00036A08 File Offset: 0x00034C08
		public static bool FilterString(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			string text = item as string;
			return text != null && text.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x00036A38 File Offset: 0x00034C38
		private static bool FilterIntegralType(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			bool flag;
			try
			{
				flag = Convert.ToString(item, CultureInfo.InvariantCulture).Contains(filter, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x00036AA4 File Offset: 0x00034CA4
		public static bool FilterFloat(string filter, object item)
		{
			return !string.IsNullOrEmpty(filter) && item != null && item is float && ((float)item).ToString("G", CultureInfo.InvariantCulture).Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x00036AE8 File Offset: 0x00034CE8
		public static bool FilterBool(string filter, object item)
		{
			return !string.IsNullOrEmpty(filter) && item != null && item is bool && ((bool)item).ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x00036B24 File Offset: 0x00034D24
		public static bool FilterEnum(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			Enum @enum = item as Enum;
			return @enum != null && @enum.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x00036B58 File Offset: 0x00034D58
		public static bool FilterUser(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			User user = item as User;
			return user != null && user.UserName != null && user.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x00036B98 File Offset: 0x00034D98
		public static bool FilterFriendship(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			Friendship friendship = item as Friendship;
			if (friendship == null)
			{
				return false;
			}
			User user = UISocialUtility.ExtractNonActiveUser(friendship);
			return user != null && user.UserName != null && user.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x00036BE0 File Offset: 0x00034DE0
		public static bool FilterBlockedUser(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			BlockedUser blockedUser = item as BlockedUser;
			if (blockedUser == null)
			{
				return false;
			}
			User user = UISocialUtility.ExtractNonActiveUser(blockedUser);
			return user != null && user.UserName != null && user.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x00036C28 File Offset: 0x00034E28
		public static bool FilterFriendRequest(string filter, object item)
		{
			if (string.IsNullOrEmpty(filter) || item == null)
			{
				return false;
			}
			FriendRequest friendRequest = item as FriendRequest;
			if (friendRequest != null)
			{
				User sender = friendRequest.Sender;
				return ((sender != null) ? sender.UserName : null) != null && friendRequest.Sender.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		// Token: 0x04000813 RID: 2067
		private const bool VERBOSE_LOGGING = false;

		// Token: 0x04000814 RID: 2068
		private static readonly Dictionary<Type, Func<string, object, bool>> typeFilterDictionary = new Dictionary<Type, Func<string, object, bool>>
		{
			{
				typeof(string),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterString)
			},
			{
				typeof(float),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterFloat)
			},
			{
				typeof(bool),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterBool)
			},
			{
				typeof(User),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterUser)
			},
			{
				typeof(Friendship),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterFriendship)
			},
			{
				typeof(BlockedUser),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterBlockedUser)
			},
			{
				typeof(FriendRequest),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterFriendRequest)
			},
			{
				typeof(Enum),
				new Func<string, object, bool>(UIIEnumerablePresenterFiltersShared.FilterEnum)
			}
		};
	}
}
