using System;
using System.Runtime.Serialization;

namespace ELM.Endless.EndlessFramework.Serialization
{
	// Token: 0x02000034 RID: 52
	internal static class SerializableTypeExtensions
	{
		// Token: 0x060000ED RID: 237 RVA: 0x0000566C File Offset: 0x0000386C
		public static T GetValue<T>(this SerializationInfo info, string name) where T : struct
		{
			return (T)((object)info.GetValue(name, typeof(T)));
		}
	}
}
