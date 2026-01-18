using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public abstract class InspectorReference
{
	[SerializeField]
	internal SerializableGuid Id = SerializableGuid.Empty;

	public bool IsReferenceEmpty()
	{
		return Id == SerializableGuid.Empty;
	}

	public bool IsReferenceSet()
	{
		return !IsReferenceEmpty();
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}", "Id", Id);
	}
}
