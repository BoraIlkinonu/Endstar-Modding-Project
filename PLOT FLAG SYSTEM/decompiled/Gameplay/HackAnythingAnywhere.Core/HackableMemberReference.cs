using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackAnythingAnywhere.Core;

[Serializable]
public class HackableMemberReference
{
	[SerializeField]
	private Component component;

	[SerializeField]
	private string memberName = "";

	[SerializeField]
	private string assemblyQualifiedTypeName;

	[SerializeField]
	private HackableMemberType memberType;

	public Component Component => component;

	public string MemberName => memberName;

	public string AssemblyQualifiedTypeName => assemblyQualifiedTypeName;

	public HackableMemberType MemberType => memberType;

	public override bool Equals(object obj)
	{
		HackableMemberReference hackableMemberReference = obj as HackableMemberReference;
		if (hackableMemberReference != null && EqualityComparer<Component>.Default.Equals(component, hackableMemberReference.component) && memberName == hackableMemberReference.memberName && assemblyQualifiedTypeName == hackableMemberReference.assemblyQualifiedTypeName)
		{
			return memberType == hackableMemberReference.memberType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((-826833188 * -1521134295 + EqualityComparer<Component>.Default.GetHashCode(component)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(memberName)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(assemblyQualifiedTypeName)) * -1521134295 + memberType.GetHashCode();
	}

	public static bool operator ==(HackableMemberReference reference, HackableMember hackableMember)
	{
		if (reference.MemberType == hackableMember.MemberType && reference.MemberName == hackableMember.MemberName)
		{
			return reference.AssemblyQualifiedTypeName == hackableMember.AssemblyQualifiedTypeName;
		}
		return false;
	}

	public static bool operator !=(HackableMemberReference reference, HackableMember hackableMember)
	{
		if (reference.MemberType == hackableMember.MemberType && !(reference.MemberName != hackableMember.MemberName))
		{
			return reference.AssemblyQualifiedTypeName != hackableMember.AssemblyQualifiedTypeName;
		}
		return true;
	}
}
