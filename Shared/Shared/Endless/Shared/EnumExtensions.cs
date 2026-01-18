using System;

namespace Endless.Shared
{
	// Token: 0x0200005C RID: 92
	public static class EnumExtensions
	{
		// Token: 0x060002D7 RID: 727 RVA: 0x0000E2F4 File Offset: 0x0000C4F4
		public static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
		{
			T t;
			try
			{
				object[] customAttributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), false);
				T t2;
				if (customAttributes.Length == 0)
				{
					t = default(T);
					t2 = t;
				}
				else
				{
					t2 = (T)((object)customAttributes[0]);
				}
				t = t2;
			}
			catch
			{
				t = default(T);
			}
			return t;
		}
	}
}
