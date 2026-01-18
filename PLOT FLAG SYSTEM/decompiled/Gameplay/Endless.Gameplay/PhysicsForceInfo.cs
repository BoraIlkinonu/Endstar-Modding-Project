using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay;

public struct PhysicsForceInfo : INetworkSerializable, IEquatable<PhysicsForceInfo>
{
	[FormerlySerializedAs("force")]
	public float Force;

	[FormerlySerializedAs("directionNormal")]
	public Vector3 DirectionNormal;

	[FormerlySerializedAs("startFrame")]
	public uint StartFrame;

	[FormerlySerializedAs("sourceID")]
	public ulong SourceID;

	[FormerlySerializedAs("friendlyForce")]
	public bool FriendlyForce;

	public bool ApplyRandomTorque;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Force, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref DirectionNormal);
		serializer.SerializeValue(ref StartFrame, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref SourceID, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref FriendlyForce, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ApplyRandomTorque, default(FastBufferWriter.ForPrimitives));
	}

	public PhysicsForceInfo GetCopy()
	{
		return new PhysicsForceInfo
		{
			Force = Force,
			DirectionNormal = DirectionNormal,
			StartFrame = StartFrame,
			SourceID = SourceID,
			FriendlyForce = FriendlyForce,
			ApplyRandomTorque = ApplyRandomTorque
		};
	}

	public bool Equals(PhysicsForceInfo other)
	{
		if (StartFrame == other.StartFrame && SourceID == other.SourceID && Mathf.Approximately(Force, other.Force) && Vector3.Distance(DirectionNormal, other.DirectionNormal) < 0.05f && FriendlyForce == other.FriendlyForce)
		{
			return ApplyRandomTorque == other.ApplyRandomTorque;
		}
		return false;
	}
}
