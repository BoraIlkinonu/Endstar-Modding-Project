using System;
using Endless.Gameplay.Serialization;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x0200003E RID: 62
	public static class EventInfoExtensions
	{
		// Token: 0x0600010F RID: 271 RVA: 0x00005996 File Offset: 0x00003B96
		public static Type GetReferencedType(this EndlessParameterInfo info)
		{
			if (info.DataType == 27)
			{
				Debug.LogWarning("");
			}
			return Type.GetType(EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(info.DataType));
		}
	}
}
