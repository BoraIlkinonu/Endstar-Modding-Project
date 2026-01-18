using System;
using System.Reflection;
using Endless.Core;
using Endless.Gameplay;
using Endless.Props.Scripting;
using UnityEngine;

namespace HackAnythingAnywhere.Core
{
	// Token: 0x0200002C RID: 44
	[Serializable]
	public class HackableMember
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600009E RID: 158 RVA: 0x00003F03 File Offset: 0x00002103
		public Type ReferencedType
		{
			get
			{
				return Type.GetType(this.assemblyQualifiedTypeName);
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600009F RID: 159 RVA: 0x00003F10 File Offset: 0x00002110
		public string MemberName
		{
			get
			{
				return this.memberName;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000A0 RID: 160 RVA: 0x00003F18 File Offset: 0x00002118
		public string AssemblyQualifiedTypeName
		{
			get
			{
				return this.assemblyQualifiedTypeName;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000A1 RID: 161 RVA: 0x00003F20 File Offset: 0x00002120
		public string DisplayTypeName
		{
			get
			{
				return this.displayTypeName;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000A2 RID: 162 RVA: 0x00003F28 File Offset: 0x00002128
		// (set) Token: 0x060000A3 RID: 163 RVA: 0x00003F30 File Offset: 0x00002130
		public HackableMember.HackabilityOption Hackability
		{
			get
			{
				return this.hackability;
			}
			set
			{
				this.hackability = value;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x00003F39 File Offset: 0x00002139
		public HackableMemberType MemberType
		{
			get
			{
				return this.memberType;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x00003F41 File Offset: 0x00002141
		public byte[] DefaultValue
		{
			get
			{
				return this.defaultValue;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00003F49 File Offset: 0x00002149
		public bool ShouldSerialize
		{
			get
			{
				return this.shouldSerialize;
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00003F51 File Offset: 0x00002151
		public HackableMember()
		{
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00003F7E File Offset: 0x0000217E
		public HackableMember(string memberName)
		{
			this.memberName = memberName;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00003FB2 File Offset: 0x000021B2
		public HackableMember(string memberName, byte[] defaultValue)
		{
			this.memberName = memberName;
			this.defaultValue = defaultValue;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00003FF0 File Offset: 0x000021F0
		public HackableMember(HackableMember other)
		{
			this.memberName = other.memberName;
			this.assemblyQualifiedTypeName = other.assemblyQualifiedTypeName;
			this.displayTypeName = other.displayTypeName;
			this.hackability = other.hackability;
			this.memberType = other.memberType;
			this.shouldSerialize = other.shouldSerialize;
			this.clampValues = new ClampValue[other.clampValues.Length];
			for (int i = 0; i < other.clampValues.Length; i++)
			{
				this.clampValues[i] = new ClampValue(other.clampValues[i]);
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000040AB File Offset: 0x000022AB
		public bool ShouldClampValue(int index)
		{
			return this.clampValues.Length > index && this.clampValues[index].ShouldClampValue;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x000040C7 File Offset: 0x000022C7
		public float MinValue(int index)
		{
			if (index < this.clampValues.Length)
			{
				return this.clampValues[index].MinValue;
			}
			return 0f;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x000040E7 File Offset: 0x000022E7
		public float MaxValue(int index)
		{
			if (index < this.clampValues.Length)
			{
				return this.clampValues[index].MaxValue;
			}
			return 0f;
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void InitializeClampValue(int index, float value)
		{
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004107 File Offset: 0x00002307
		public MemberInfo RetrieveMemberInfo()
		{
			return this.RetrieveMemberInfo(this.ReferencedType);
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004118 File Offset: 0x00002318
		public MemberInfo RetrieveMemberInfo(Type parentType)
		{
			HackableMemberType hackableMemberType = this.memberType;
			MemberTypes memberTypes;
			if (hackableMemberType != HackableMemberType.Field)
			{
				if (hackableMemberType != HackableMemberType.Property)
				{
				}
				memberTypes = MemberTypes.Property;
			}
			else
			{
				memberTypes = MemberTypes.Field;
			}
			MemberInfo[] member = parentType.GetMember(this.memberName, memberTypes, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (member.Length == 0)
			{
				return null;
			}
			return member[0];
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004158 File Offset: 0x00002358
		public bool UpdateDefaultValue(Component component)
		{
			byte[] bytes = BinarySerializationHelper.GetBytes(this.RetrieveMemberInfo(component.GetType()).GetValue(component));
			if (bytes != this.defaultValue)
			{
				this.defaultValue = bytes;
				return true;
			}
			return false;
		}

		// Token: 0x0400006A RID: 106
		public static Type[] SupportedTypes = new Type[]
		{
			typeof(float),
			typeof(int),
			typeof(bool),
			typeof(string),
			typeof(Vector2),
			typeof(Vector3),
			typeof(Vector4),
			typeof(Vector2Int),
			typeof(Vector3Int),
			typeof(Quaternion),
			typeof(LevelDestination),
			typeof(InventorySpawnOptions),
			typeof(NpcInstanceReference),
			typeof(CellReference),
			typeof(InventoryLibraryReference)
		};

		// Token: 0x0400006B RID: 107
		public const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		// Token: 0x0400006C RID: 108
		public const BindingFlags BINDING_FLAGS_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

		// Token: 0x0400006D RID: 109
		[SerializeField]
		private string memberName = "";

		// Token: 0x0400006E RID: 110
		[SerializeField]
		private string assemblyQualifiedTypeName;

		// Token: 0x0400006F RID: 111
		[SerializeField]
		private string displayTypeName;

		// Token: 0x04000070 RID: 112
		[SerializeField]
		private byte[] defaultValue;

		// Token: 0x04000071 RID: 113
		[SerializeField]
		private HackableMember.HackabilityOption hackability = HackableMember.HackabilityOption.Hackable;

		// Token: 0x04000072 RID: 114
		[SerializeField]
		private HackableMemberType memberType;

		// Token: 0x04000073 RID: 115
		[SerializeField]
		private bool shouldSerialize = true;

		// Token: 0x04000074 RID: 116
		[SerializeField]
		private ClampValue[] clampValues = new ClampValue[0];

		// Token: 0x0200002D RID: 45
		public enum HackabilityOption
		{
			// Token: 0x04000076 RID: 118
			Hidden,
			// Token: 0x04000077 RID: 119
			Viewable,
			// Token: 0x04000078 RID: 120
			Hackable
		}
	}
}
