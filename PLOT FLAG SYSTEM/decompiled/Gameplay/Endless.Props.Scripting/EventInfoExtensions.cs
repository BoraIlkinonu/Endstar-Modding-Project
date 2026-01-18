using System;
using Endless.Gameplay.Serialization;
using UnityEngine;

namespace Endless.Props.Scripting;

public static class EventInfoExtensions
{
	public static Type GetReferencedType(this EndlessParameterInfo info)
	{
		if (info.DataType == 27)
		{
			Debug.LogWarning("");
		}
		return Type.GetType(EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(info.DataType));
	}
}
