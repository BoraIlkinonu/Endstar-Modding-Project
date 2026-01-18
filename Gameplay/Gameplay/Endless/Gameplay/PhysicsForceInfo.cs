using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay
{
	// Token: 0x02000126 RID: 294
	public struct PhysicsForceInfo : INetworkSerializable, IEquatable<PhysicsForceInfo>
	{
		// Token: 0x060006A7 RID: 1703 RVA: 0x00020F98 File Offset: 0x0001F198
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<float>(ref this.Force, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.DirectionNormal);
			serializer.SerializeValue<uint>(ref this.StartFrame, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<ulong>(ref this.SourceID, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.FriendlyForce, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.ApplyRandomTorque, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x00021020 File Offset: 0x0001F220
		public PhysicsForceInfo GetCopy()
		{
			return new PhysicsForceInfo
			{
				Force = this.Force,
				DirectionNormal = this.DirectionNormal,
				StartFrame = this.StartFrame,
				SourceID = this.SourceID,
				FriendlyForce = this.FriendlyForce,
				ApplyRandomTorque = this.ApplyRandomTorque
			};
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x00021084 File Offset: 0x0001F284
		public bool Equals(PhysicsForceInfo other)
		{
			return this.StartFrame == other.StartFrame && this.SourceID == other.SourceID && Mathf.Approximately(this.Force, other.Force) && Vector3.Distance(this.DirectionNormal, other.DirectionNormal) < 0.05f && this.FriendlyForce == other.FriendlyForce && this.ApplyRandomTorque == other.ApplyRandomTorque;
		}

		// Token: 0x04000556 RID: 1366
		[FormerlySerializedAs("force")]
		public float Force;

		// Token: 0x04000557 RID: 1367
		[FormerlySerializedAs("directionNormal")]
		public Vector3 DirectionNormal;

		// Token: 0x04000558 RID: 1368
		[FormerlySerializedAs("startFrame")]
		public uint StartFrame;

		// Token: 0x04000559 RID: 1369
		[FormerlySerializedAs("sourceID")]
		public ulong SourceID;

		// Token: 0x0400055A RID: 1370
		[FormerlySerializedAs("friendlyForce")]
		public bool FriendlyForce;

		// Token: 0x0400055B RID: 1371
		public bool ApplyRandomTorque;
	}
}
