using System;
using System.Collections;

namespace Endless.Shared.UI
{
	// Token: 0x02000236 RID: 566
	public static class UIPresenterModelTypeUtility
	{
		// Token: 0x06000E56 RID: 3670 RVA: 0x0003ECE4 File Offset: 0x0003CEE4
		public static Type SanitizeType(Type modelType)
		{
			if (modelType == typeof(string))
			{
				return typeof(string);
			}
			if (typeof(IEnumerable).IsAssignableFrom(modelType))
			{
				return typeof(IEnumerable);
			}
			if (modelType.IsEnum)
			{
				return typeof(Enum);
			}
			return modelType;
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x0003ED3F File Offset: 0x0003CF3F
		public static Type GetArrayOrListElementType(object CurrentValue)
		{
			if (CurrentValue.GetType().IsArray)
			{
				return CurrentValue.GetType().GetElementType();
			}
			return CurrentValue.GetType().GetGenericArguments()[0];
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x0003ED67 File Offset: 0x0003CF67
		public static Type GetArrayOrListElementType(Type type)
		{
			if (type.IsArray)
			{
				return type.GetElementType();
			}
			return type.GetGenericArguments()[0];
		}
	}
}
