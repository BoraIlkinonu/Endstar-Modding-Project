using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000243 RID: 579
	public abstract class UIBaseEnumRadio<TEnum> : UIBaseRadio<TEnum> where TEnum : Enum
	{
		// Token: 0x06000EBA RID: 3770 RVA: 0x0003F820 File Offset: 0x0003DA20
		protected override TEnum[] GetValues()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetValues", Array.Empty<object>());
			}
			Array values = Enum.GetValues(typeof(TEnum));
			TEnum[] array = new TEnum[values.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (TEnum)((object)values.GetValue(i));
			}
			return array;
		}
	}
}
