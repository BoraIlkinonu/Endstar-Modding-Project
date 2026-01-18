using System;
using System.Reflection;
using Endless.Core;
using Endless.Gameplay;
using Endless.Props.Scripting;
using UnityEngine;

namespace HackAnythingAnywhere.Core;

[Serializable]
public class HackableMember
{
	public enum HackabilityOption
	{
		Hidden,
		Viewable,
		Hackable
	}

	public static Type[] SupportedTypes = new Type[15]
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

	public const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public const BindingFlags BINDING_FLAGS_PUBLIC = BindingFlags.Instance | BindingFlags.Public;

	[SerializeField]
	private string memberName = "";

	[SerializeField]
	private string assemblyQualifiedTypeName;

	[SerializeField]
	private string displayTypeName;

	[SerializeField]
	private byte[] defaultValue;

	[SerializeField]
	private HackabilityOption hackability = HackabilityOption.Hackable;

	[SerializeField]
	private HackableMemberType memberType;

	[SerializeField]
	private bool shouldSerialize = true;

	[SerializeField]
	private ClampValue[] clampValues = new ClampValue[0];

	public Type ReferencedType => Type.GetType(assemblyQualifiedTypeName);

	public string MemberName => memberName;

	public string AssemblyQualifiedTypeName => assemblyQualifiedTypeName;

	public string DisplayTypeName => displayTypeName;

	public HackabilityOption Hackability
	{
		get
		{
			return hackability;
		}
		set
		{
			hackability = value;
		}
	}

	public HackableMemberType MemberType => memberType;

	public byte[] DefaultValue => defaultValue;

	public bool ShouldSerialize => shouldSerialize;

	public HackableMember()
	{
	}

	public HackableMember(string memberName)
	{
		this.memberName = memberName;
	}

	public HackableMember(string memberName, byte[] defaultValue)
	{
		this.memberName = memberName;
		this.defaultValue = defaultValue;
	}

	public HackableMember(HackableMember other)
	{
		memberName = other.memberName;
		assemblyQualifiedTypeName = other.assemblyQualifiedTypeName;
		displayTypeName = other.displayTypeName;
		hackability = other.hackability;
		memberType = other.memberType;
		shouldSerialize = other.shouldSerialize;
		clampValues = new ClampValue[other.clampValues.Length];
		for (int i = 0; i < other.clampValues.Length; i++)
		{
			clampValues[i] = new ClampValue(other.clampValues[i]);
		}
	}

	public bool ShouldClampValue(int index)
	{
		if (clampValues.Length > index)
		{
			return clampValues[index].ShouldClampValue;
		}
		return false;
	}

	public float MinValue(int index)
	{
		if (index < clampValues.Length)
		{
			return clampValues[index].MinValue;
		}
		return 0f;
	}

	public float MaxValue(int index)
	{
		if (index < clampValues.Length)
		{
			return clampValues[index].MaxValue;
		}
		return 0f;
	}

	public void InitializeClampValue(int index, float value)
	{
	}

	public MemberInfo RetrieveMemberInfo()
	{
		return RetrieveMemberInfo(ReferencedType);
	}

	public MemberInfo RetrieveMemberInfo(Type parentType)
	{
		HackableMemberType hackableMemberType = memberType;
		MemberInfo[] member = parentType.GetMember(memberName, hackableMemberType switch
		{
			HackableMemberType.Field => MemberTypes.Field, 
			_ => MemberTypes.Property, 
		}, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (member.Length == 0)
		{
			return null;
		}
		return member[0];
	}

	public bool UpdateDefaultValue(Component component)
	{
		byte[] bytes = BinarySerializationHelper.GetBytes(RetrieveMemberInfo(component.GetType()).GetValue(component));
		if (bytes != defaultValue)
		{
			defaultValue = bytes;
			return true;
		}
		return false;
	}
}
