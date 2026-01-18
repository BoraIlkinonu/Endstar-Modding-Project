using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace __GEN
{
	// Token: 0x020003BF RID: 959
	internal class NetworkVariableSerializationHelper
	{
		// Token: 0x06001297 RID: 4759 RVA: 0x0005FE3E File Offset: 0x0005E03E
		[RuntimeInitializeOnLoadMethod]
		internal static void InitializeSerialization()
		{
			NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<Color>();
			NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<Color>();
			NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<SerializableGuid>();
			NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<SerializableGuid>();
		}
	}
}
