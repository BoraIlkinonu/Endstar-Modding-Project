using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000135 RID: 309
	public class UIDropdownEnum : UIBaseDropdown<Enum>
	{
		// Token: 0x17000159 RID: 345
		// (get) Token: 0x060007BA RID: 1978 RVA: 0x00020A69 File Offset: 0x0001EC69
		public UnityEvent<Enum> OnEnumValueChanged { get; } = new UnityEvent<Enum>();

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x060007BB RID: 1979 RVA: 0x00020A74 File Offset: 0x0001EC74
		public Enum EnumValue
		{
			get
			{
				if (base.Value.Count == 0)
				{
					return null;
				}
				if (!this.hasFlags || base.Value.Count <= 1)
				{
					return base.Value.FirstOrDefault<Enum>();
				}
				long num = base.Value.Aggregate(0L, (long current, Enum e) => current | Convert.ToInt64(e));
				return (Enum)Enum.ToObject(this.enumType, num);
			}
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x060007BC RID: 1980 RVA: 0x00020AF0 File Offset: 0x0001ECF0
		// (set) Token: 0x060007BD RID: 1981 RVA: 0x00020AF8 File Offset: 0x0001ECF8
		public bool Initialized { get; private set; }

		// Token: 0x060007BE RID: 1982 RVA: 0x00020B04 File Offset: 0x0001ED04
		protected override void Start()
		{
			base.Start();
			if (!string.IsNullOrEmpty(this.enumAssemblyQualifiedName))
			{
				this.InitializeWithAssemblyQualifiedName(this.enumAssemblyQualifiedName);
				if (this.enumType != null)
				{
					base.SetLabel(this.enumType.Name);
				}
			}
			base.OnValueChanged.AddListener(new UnityAction(this.TriggerOnEnumValueChanged));
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x00020B68 File Offset: 0x0001ED68
		public override void ToggleValueIndex(int valueIndex, bool triggerValueChanged)
		{
			if (this.enumType == null || !this.hasFlags)
			{
				base.ToggleValueIndex(valueIndex, triggerValueChanged);
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "ToggleValueIndex", "valueIndex", valueIndex, "triggerValueChanged", triggerValueChanged }), this);
			}
			base.ValidateIndex(valueIndex, base.Options.Count);
			Enum @enum = base.Options[valueIndex];
			long num = Convert.ToInt64(@enum);
			List<Enum> list = new List<Enum>(base.Value);
			if (this.GetIsSelected(valueIndex))
			{
				list.Remove(@enum);
			}
			else if (num == 0L)
			{
				list.Clear();
				list.Add(@enum);
			}
			else
			{
				UIDropdownEnum.RemoveNoneOptionIfExists(list);
				list.Add(@enum);
			}
			base.SetValue(list.ToArray(), triggerValueChanged);
			if (triggerValueChanged)
			{
				base.OnIndexChanged.Invoke(valueIndex);
			}
			if (!base.CanHaveMultipleValues)
			{
				base.HideOptions();
			}
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x00020C6C File Offset: 0x0001EE6C
		public override bool GetIsSelected(int index)
		{
			if (this.enumType == null || !this.hasFlags)
			{
				return base.GetIsSelected(index);
			}
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetIsSelected", "index", index), this);
			}
			long num = base.Value.Aggregate(0L, (long current, Enum e) => current | Convert.ToInt64(e));
			long num2 = Convert.ToInt64(base.Options[index]);
			if (num2 == 0L)
			{
				return num == 0L;
			}
			return (num & num2) == num2;
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x00020D10 File Offset: 0x0001EF10
		public void InitializeWithAssemblyQualifiedName(string assemblyQualifiedName)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("InitializeWithAssemblyQualifiedName ( assemblyQualifiedName: " + assemblyQualifiedName + " )", this);
			}
			this.enumType = Type.GetType(assemblyQualifiedName);
			Type type = this.enumType;
			if (type == null || !type.IsEnum)
			{
				DebugUtility.LogError("Invalid enum type: " + assemblyQualifiedName, this);
				return;
			}
			this.InitializeDropdownWithEnum(this.enumType);
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00020D78 File Offset: 0x0001EF78
		public void InitializeDropdownWithEnum(Type targetEnumType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InitializeDropdownWithEnum", "targetEnumType", targetEnumType), this);
			}
			if (targetEnumType == null || !targetEnumType.IsEnum)
			{
				throw new ArgumentException("Provided type must be an enum.", "targetEnumType");
			}
			this.enumType = targetEnumType;
			this.hasFlags = this.enumType.IsDefined(typeof(FlagsAttribute), false);
			base.SetCanHaveMultipleValues(this.hasFlags);
			IEnumerable<Enum> enumerable = Enum.GetValues(this.enumType).Cast<Enum>();
			if (this.hasFlags)
			{
				enumerable = enumerable.Where(new Func<Enum, bool>(UIDropdownEnum.IsPowerOfTwoOrZero));
			}
			Enum[] array = enumerable.ToArray<Enum>();
			this.enumNames = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Enum @enum = array[i];
				string[] array2 = this.enumNames;
				int num = i;
				UserFacingTextAttribute attributeOfType = @enum.GetAttributeOfType<UserFacingTextAttribute>();
				array2[num] = ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? @enum.ToString();
			}
			base.SetOptionsAndValue(array, new Enum[] { array.FirstOrDefault<Enum>() }, false);
			this.Initialized = true;
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x00020E88 File Offset: 0x0001F088
		public void SetEnumValue(Enum value, bool triggerValueChanged)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetEnumValue", "value", value, "triggerValueChanged", triggerValueChanged }), this);
			}
			if (!this.Initialized)
			{
				this.InitializeDropdownWithEnum(value.GetType());
			}
			Type type = this.enumType;
			if (type == null || !type.IsEnum)
			{
				throw new InvalidOperationException("Enum type must be initialized first.");
			}
			if (value.GetType() != this.enumType)
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Value type '",
					value.GetType().Name,
					"' does not match '",
					this.enumType.Name,
					"'."
				}));
			}
			if (!this.hasFlags)
			{
				base.SetValue(new Enum[] { value }, triggerValueChanged);
				return;
			}
			Array values = Enum.GetValues(this.enumType);
			long num = Convert.ToInt64(value);
			List<Enum> list = new List<Enum>();
			foreach (object obj in values)
			{
				Enum @enum = (Enum)obj;
				long num2 = Convert.ToInt64(@enum);
				if (num2 == 0L && num == 0L)
				{
					list.Add(@enum);
				}
				else if (num2 != 0L && (num & num2) == num2)
				{
					list.Add(@enum);
				}
			}
			Enum[] array = list.Where((Enum e) => base.Options.Contains(e)).Distinct<Enum>().ToArray<Enum>();
			base.SetValue(array, triggerValueChanged);
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x00021030 File Offset: 0x0001F230
		protected override string GetLabelFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetLabelFromOption", "optionIndex", optionIndex), this);
			}
			base.ValidateIndex(optionIndex, base.Count);
			return this.enumNames[optionIndex];
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x0002106F File Offset: 0x0001F26F
		protected override Sprite GetIconFromOption(int optionIndex)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetIconFromOption", "optionIndex", optionIndex), this);
			}
			return null;
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x0002109A File Offset: 0x0001F29A
		protected override string GetValueText()
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log("GetValueText", this);
			}
			if (!this.hasFlags || !this.AllButNoneSelected())
			{
				return base.GetValueText();
			}
			return "All";
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000210CC File Offset: 0x0001F2CC
		private bool AllButNoneSelected()
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log("AllButNoneSelected", this);
			}
			long num = (from v in Enum.GetValues(this.enumType).Cast<Enum>().Select(new Func<Enum, long>(Convert.ToInt64))
				where v != 0L && (v & (v - 1L)) == 0L
				select v).Aggregate(0L, (long acc, long v) => acc | v);
			return base.Value.Aggregate(0L, (long current, Enum selected) => current | Convert.ToInt64(selected)) == num;
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00021187 File Offset: 0x0001F387
		private void TriggerOnEnumValueChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("TriggerOnEnumValueChanged", this);
			}
			this.OnEnumValueChanged.Invoke(this.EnumValue);
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x000211B0 File Offset: 0x0001F3B0
		private static bool IsPowerOfTwoOrZero(Enum e)
		{
			long num = Convert.ToInt64(e);
			return num == 0L || (num & (num - 1L)) == 0L;
		}

		// Token: 0x060007CA RID: 1994 RVA: 0x000211D4 File Offset: 0x0001F3D4
		private static void RemoveNoneOptionIfExists(List<Enum> values)
		{
			Enum @enum = values.FirstOrDefault((Enum o) => Convert.ToInt64(o) == 0L);
			if (@enum != null)
			{
				values.Remove(@enum);
			}
		}

		// Token: 0x0400049A RID: 1178
		[Header("UIDropdownEnum")]
		[SerializeField]
		private string enumAssemblyQualifiedName;

		// Token: 0x0400049B RID: 1179
		private Type enumType;

		// Token: 0x0400049C RID: 1180
		private string[] enumNames = Array.Empty<string>();

		// Token: 0x0400049D RID: 1181
		private bool hasFlags;
	}
}
