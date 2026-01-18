using System;
using System.Collections;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000267 RID: 615
	public class UISpriteAndEnumTabGroup : UIBaseTabGroup<SpriteAndEnum>
	{
		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000F93 RID: 3987 RVA: 0x00042EFE File Offset: 0x000410FE
		// (set) Token: 0x06000F94 RID: 3988 RVA: 0x00042F06 File Offset: 0x00041106
		public bool PopulatedFromEnum { get; private set; }

		// Token: 0x06000F95 RID: 3989 RVA: 0x00042F10 File Offset: 0x00041110
		public void PopulateFromEnum(Enum value, bool triggerOnValueChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "PopulateFromEnum", "value", value, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			Array values = Enum.GetValues(value.GetType());
			SpriteAndEnum[] array = new SpriteAndEnum[values.Length];
			int num = 0;
			for (int i = 0; i < values.Length; i++)
			{
				Enum @enum = (Enum)values.GetValue(i);
				if (@enum.Equals(value))
				{
					num = i;
				}
				array[i] = new SpriteAndEnum(null, @enum);
			}
			this.PopulatedFromEnum = true;
			base.SetOptionsAndValue(array, num, triggerOnValueChanged);
		}

		// Token: 0x06000F96 RID: 3990 RVA: 0x00042FC4 File Offset: 0x000411C4
		public void PopulateFromEnumWithSprites(Enum value, Sprite[] sprites, bool triggerOnValueChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "PopulateFromEnumWithSprites", "value", value, "sprites", sprites.Length, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			Array values = Enum.GetValues(value.GetType());
			if (values.Length != sprites.Length)
			{
				Debug.LogError(string.Format("The number of {0} provided ({1}) does not match the number of {2} values ({3})!", new object[] { "sprites", sprites.Length, "enumValues", values.Length }), this);
				this.PopulateFromEnum(value, triggerOnValueChanged);
				return;
			}
			SpriteAndEnum[] array = new SpriteAndEnum[values.Length];
			int num = 0;
			for (int i = 0; i < values.Length; i++)
			{
				Sprite sprite = sprites[i];
				Enum @enum = (Enum)values.GetValue(i);
				if (@enum.Equals(value))
				{
					num = i;
				}
				array[i] = new SpriteAndEnum(sprite, @enum);
			}
			this.PopulatedFromEnum = true;
			base.SetOptionsAndValue(array, num, triggerOnValueChanged);
		}

		// Token: 0x06000F97 RID: 3991 RVA: 0x000430E4 File Offset: 0x000412E4
		public void SetValue(Enum value, bool triggerOnValueChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetValue", "value", value, "triggerOnValueChanged", triggerOnValueChanged }), this);
			}
			Array values = Enum.GetValues(value.GetType());
			int num = -1;
			int num2 = 0;
			using (IEnumerator enumerator = values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (object.Equals(enumerator.Current as Enum, value))
					{
						num = num2;
						break;
					}
					num2++;
				}
			}
			if (num >= 0)
			{
				this.SetValue(num, triggerOnValueChanged);
				return;
			}
			DebugUtility.LogError(string.Format("The specified {0} '{1}' is not a valid option for this {2}!", "value", value, "UISpriteAndEnumTabGroup"), this);
		}
	}
}
