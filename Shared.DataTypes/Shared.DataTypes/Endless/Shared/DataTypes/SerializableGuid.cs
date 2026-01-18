using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000015 RID: 21
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public struct SerializableGuid : IComparable, IComparable<SerializableGuid>, IEquatable<SerializableGuid>, INetworkSerializable
	{
		// Token: 0x06000093 RID: 147 RVA: 0x00003C6C File Offset: 0x00001E6C
		public SerializableGuid(long long1, long long2)
		{
			this.Guid = Guid.Empty;
			this.Long1 = long1;
			this.Long2 = long2;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00003C87 File Offset: 0x00001E87
		public SerializableGuid(Guid guid)
		{
			this.Long1 = 0L;
			this.Long2 = 0L;
			this.Guid = guid;
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00003CA0 File Offset: 0x00001EA0
		public SerializableGuid(string guidString)
		{
			this.Long1 = 0L;
			this.Long2 = 0L;
			this.Guid = new Guid(guidString);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003CBE File Offset: 0x00001EBE
		public static SerializableGuid NewGuid()
		{
			return new SerializableGuid(Guid.NewGuid());
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003CCA File Offset: 0x00001ECA
		public static bool IsValid(Guid guid)
		{
			return !string.IsNullOrEmpty(guid.ToString()) && guid != Guid.Empty;
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000098 RID: 152 RVA: 0x00003CED File Offset: 0x00001EED
		[JsonIgnore]
		public bool IsEmpty
		{
			get
			{
				return this.Guid == Guid.Empty;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000099 RID: 153 RVA: 0x00003CFF File Offset: 0x00001EFF
		public static SerializableGuid Empty
		{
			get
			{
				return new SerializableGuid(Guid.Empty);
			}
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003D0B File Offset: 0x00001F0B
		public static implicit operator SerializableGuid(Guid guid)
		{
			return new SerializableGuid(guid);
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00003D13 File Offset: 0x00001F13
		public static implicit operator Guid(SerializableGuid serializableGuid)
		{
			return serializableGuid.Guid;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00003D1B File Offset: 0x00001F1B
		public static implicit operator SerializableGuid(string guid)
		{
			if (!string.IsNullOrEmpty(guid))
			{
				return new SerializableGuid(Guid.Parse(guid));
			}
			return SerializableGuid.Empty;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003D36 File Offset: 0x00001F36
		public static implicit operator string(SerializableGuid serializableGuid)
		{
			return serializableGuid.Guid.ToString();
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00003D4A File Offset: 0x00001F4A
		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}
			if (!(value is SerializableGuid))
			{
				throw new ArgumentException("Must be SerializableGuid");
			}
			if (!(((SerializableGuid)value).Guid == this.Guid))
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00003D7F File Offset: 0x00001F7F
		public int CompareTo(SerializableGuid other)
		{
			if (!(other.Guid == this.Guid))
			{
				return 1;
			}
			return 0;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00003D97 File Offset: 0x00001F97
		public static bool operator ==(SerializableGuid obj1, SerializableGuid obj2)
		{
			return obj1.Guid == obj2.Guid;
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00003DAA File Offset: 0x00001FAA
		public static bool operator !=(SerializableGuid obj1, SerializableGuid obj2)
		{
			return !(obj1 == obj2);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00003DB6 File Offset: 0x00001FB6
		public static bool operator ==(SerializableGuid obj1, string obj2)
		{
			return obj1.Guid.ToString() == obj2;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00003DD0 File Offset: 0x00001FD0
		public static bool operator !=(SerializableGuid obj1, string obj2)
		{
			return !(obj1 == obj2);
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x00003D97 File Offset: 0x00001F97
		public bool Equals(SerializableGuid other)
		{
			return this.Guid == other.Guid;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00003DDC File Offset: 0x00001FDC
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00003DEF File Offset: 0x00001FEF
		public override int GetHashCode()
		{
			return this.Guid.GetHashCode();
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00003E02 File Offset: 0x00002002
		public override string ToString()
		{
			return this.Guid.ToString();
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00003E15 File Offset: 0x00002015
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				Compression.SerializeLong<T>(serializer, this.Long1);
				Compression.SerializeLong<T>(serializer, this.Long2);
				return;
			}
			this.Long1 = Compression.DeserializeLong<T>(serializer);
			this.Long2 = Compression.DeserializeLong<T>(serializer);
		}

		// Token: 0x04000037 RID: 55
		[FieldOffset(0)]
		public Guid Guid;

		// Token: 0x04000038 RID: 56
		[SerializeField]
		[JsonIgnore]
		[FieldOffset(0)]
		public long Long1;

		// Token: 0x04000039 RID: 57
		[JsonIgnore]
		[SerializeField]
		[FieldOffset(8)]
		public long Long2;
	}
}
