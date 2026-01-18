using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace __GEN;

internal class NetworkVariableSerializationHelper
{
	[RuntimeInitializeOnLoadMethod]
	internal static void InitializeSerialization()
	{
		NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<Color>();
		NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<Color>();
		NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedINetworkSerializable<SerializableGuid>();
		NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<SerializableGuid>();
	}
}
