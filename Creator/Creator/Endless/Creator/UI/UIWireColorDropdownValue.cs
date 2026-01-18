using System;
using System.Text;
using Endless.Gameplay;

namespace Endless.Creator.UI
{
	// Token: 0x020002F4 RID: 756
	public struct UIWireColorDropdownValue
	{
		// Token: 0x06000D15 RID: 3349 RVA: 0x0003ECCF File Offset: 0x0003CECF
		public UIWireColorDropdownValue(UIWireColorDictionaryEntryValue wireColorDictionaryEntryValue, WireColor wireColor)
		{
			this.WireColorDictionaryEntryValue = wireColorDictionaryEntryValue;
			this.WireColor = wireColor;
			this.WireColorName = UIWireColorDropdownValue.AddSpaceBeforeCapitals(wireColor.ToString(), false);
		}

		// Token: 0x06000D16 RID: 3350 RVA: 0x0003ECF8 File Offset: 0x0003CEF8
		private static string AddSpaceBeforeCapitals(string input, bool preserveAcronyms)
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				if (UIWireColorDropdownValue.ShouldInsertSpace(input, i, preserveAcronyms))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(input[i]);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000D17 RID: 3351 RVA: 0x0003ED51 File Offset: 0x0003CF51
		private static bool ShouldInsertSpace(string input, int index, bool preserveAcronyms)
		{
			return index > 0 && char.IsUpper(input[index]) && (!preserveAcronyms || index >= input.Length - 1 || !char.IsUpper(input[index + 1]));
		}

		// Token: 0x04000B48 RID: 2888
		public readonly UIWireColorDictionaryEntryValue WireColorDictionaryEntryValue;

		// Token: 0x04000B49 RID: 2889
		public readonly WireColor WireColor;

		// Token: 0x04000B4A RID: 2890
		public string WireColorName;
	}
}
