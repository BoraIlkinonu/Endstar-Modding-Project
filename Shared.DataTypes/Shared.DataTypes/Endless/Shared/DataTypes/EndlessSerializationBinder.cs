using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000016 RID: 22
	public sealed class EndlessSerializationBinder : SerializationBinder
	{
		// Token: 0x060000A9 RID: 169 RVA: 0x00003E54 File Offset: 0x00002054
		public override Type BindToType(string assemblyName, string typeName)
		{
			if (typeName == "Endless.Shared." + typeof(SerializableGuid).Name)
			{
				Debug.Log("Returning SerializableGuid type directly");
				return typeof(SerializableGuid);
			}
			if (typeName == "System.Collections.Generic.List`1[[Endless.Shared.SerializableGuid, Shared, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]")
			{
				Debug.Log("Returning List SerializableGuid type directly");
				return typeof(List<SerializableGuid>);
			}
			return Type.GetType(assemblyName + ", " + typeName);
		}
	}
}
