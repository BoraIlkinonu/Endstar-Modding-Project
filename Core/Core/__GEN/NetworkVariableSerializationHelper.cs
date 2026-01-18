using System;
using Endless.Core;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace __GEN
{
	// Token: 0x020000E6 RID: 230
	internal class NetworkVariableSerializationHelper
	{
		// Token: 0x06000517 RID: 1303 RVA: 0x00018601 File Offset: 0x00016801
		[RuntimeInitializeOnLoadMethod]
		internal static void InitializeSerialization()
		{
			NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<GameState>();
			NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedValueEquals<GameState>();
			NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<SerializableGuid>();
			NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<SerializableGuid>();
		}
	}
}
