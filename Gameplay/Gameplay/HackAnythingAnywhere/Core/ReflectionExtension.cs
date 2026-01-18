using System;
using System.Reflection;

namespace HackAnythingAnywhere.Core
{
	// Token: 0x0200002F RID: 47
	public static class ReflectionExtension
	{
		// Token: 0x060000BC RID: 188 RVA: 0x000043F0 File Offset: 0x000025F0
		public static object GetValue(this MemberInfo member, object obj)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				return ((FieldInfo)member).GetValue(obj);
			}
			if (memberType != MemberTypes.Property)
			{
				throw new ArgumentException("MemberInfo must be of type Field or Property", "member");
			}
			return ((PropertyInfo)member).GetValue(obj);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004438 File Offset: 0x00002638
		public static void SetValue(this MemberInfo member, object obj, object value)
		{
			MemberTypes memberType = member.MemberType;
			if (memberType == MemberTypes.Field)
			{
				((FieldInfo)member).SetValue(obj, value);
				return;
			}
			if (memberType != MemberTypes.Property)
			{
				throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo", "member");
			}
			((PropertyInfo)member).SetValue(obj, value);
		}
	}
}
