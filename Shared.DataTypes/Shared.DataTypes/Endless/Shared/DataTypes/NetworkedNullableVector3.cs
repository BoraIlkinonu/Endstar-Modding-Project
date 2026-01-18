using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000010 RID: 16
	public struct NetworkedNullableVector3 : INetworkSerializable, IEquatable<NetworkedNullableVector3>
	{
		// Token: 0x0600005D RID: 93 RVA: 0x000033F4 File Offset: 0x000015F4
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<bool>(ref this.HasValue, default(FastBufferWriter.ForPrimitives));
			if (this.HasValue)
			{
				serializer.SerializeValue(ref this.Value);
			}
		}

		// Token: 0x0600005E RID: 94 RVA: 0x0000342C File Offset: 0x0000162C
		public static implicit operator Vector3?(NetworkedNullableVector3 n)
		{
			if (!n.HasValue)
			{
				return null;
			}
			return new Vector3?(n.Value);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00003458 File Offset: 0x00001658
		public static implicit operator NetworkedNullableVector3(Vector3? v)
		{
			return new NetworkedNullableVector3
			{
				HasValue = (v != null),
				Value = v.GetValueOrDefault()
			};
		}

		// Token: 0x06000060 RID: 96 RVA: 0x0000348A File Offset: 0x0000168A
		public bool Equals(NetworkedNullableVector3 other)
		{
			return this.HasValue == other.HasValue && (!this.HasValue || this.Value.Equals(other.Value));
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000034B8 File Offset: 0x000016B8
		public override bool Equals(object obj)
		{
			if (obj is NetworkedNullableVector3)
			{
				NetworkedNullableVector3 networkedNullableVector = (NetworkedNullableVector3)obj;
				return this.Equals(networkedNullableVector);
			}
			return false;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000034DD File Offset: 0x000016DD
		public override int GetHashCode()
		{
			if (!this.HasValue)
			{
				return 0;
			}
			return this.Value.GetHashCode();
		}

		// Token: 0x0400002A RID: 42
		public bool HasValue;

		// Token: 0x0400002B RID: 43
		public Vector3 Value;
	}
}
