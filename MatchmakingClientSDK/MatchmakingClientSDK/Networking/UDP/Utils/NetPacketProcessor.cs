using System;
using System.Collections.Generic;

namespace Networking.UDP.Utils
{
	// Token: 0x0200003D RID: 61
	public class NetPacketProcessor
	{
		// Token: 0x06000234 RID: 564 RVA: 0x0000AE81 File Offset: 0x00009081
		public NetPacketProcessor()
		{
			this._netSerializer = new NetSerializer();
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0000AE9F File Offset: 0x0000909F
		public NetPacketProcessor(int maxStringLength)
		{
			this._netSerializer = new NetSerializer(maxStringLength);
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0000AEBE File Offset: 0x000090BE
		protected virtual ulong GetHash<T>()
		{
			return NetPacketProcessor.HashCache<T>.Id;
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000AEC8 File Offset: 0x000090C8
		protected virtual NetPacketProcessor.SubscribeDelegate GetCallbackFromData(NetDataReader reader)
		{
			ulong @ulong = reader.GetULong();
			NetPacketProcessor.SubscribeDelegate subscribeDelegate;
			if (!this._callbacks.TryGetValue(@ulong, out subscribeDelegate))
			{
				throw new ParseException("Undefined packet in NetDataReader");
			}
			return subscribeDelegate;
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000AEF8 File Offset: 0x000090F8
		protected virtual void WriteHash<T>(NetDataWriter writer)
		{
			writer.Put(this.GetHash<T>());
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000AF06 File Offset: 0x00009106
		public void RegisterNestedType<T>() where T : struct, INetSerializable
		{
			this._netSerializer.RegisterNestedType<T>();
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0000AF13 File Offset: 0x00009113
		public void RegisterNestedType<T>(Action<NetDataWriter, T> writeDelegate, Func<NetDataReader, T> readDelegate)
		{
			this._netSerializer.RegisterNestedType<T>(writeDelegate, readDelegate);
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000AF22 File Offset: 0x00009122
		public void RegisterNestedType<T>(Func<T> constructor) where T : class, INetSerializable
		{
			this._netSerializer.RegisterNestedType<T>(constructor);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000AF30 File Offset: 0x00009130
		public void ReadAllPackets(NetDataReader reader)
		{
			while (reader.AvailableBytes > 0)
			{
				this.ReadPacket(reader);
			}
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000AF44 File Offset: 0x00009144
		public void ReadAllPackets(NetDataReader reader, object userData)
		{
			while (reader.AvailableBytes > 0)
			{
				this.ReadPacket(reader, userData);
			}
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000AF59 File Offset: 0x00009159
		public void ReadPacket(NetDataReader reader)
		{
			this.ReadPacket(reader, null);
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000AF63 File Offset: 0x00009163
		public void Write<T>(NetDataWriter writer, T packet) where T : class, new()
		{
			this.WriteHash<T>(writer);
			this._netSerializer.Serialize<T>(writer, packet);
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000AF79 File Offset: 0x00009179
		public void WriteNetSerializable<T>(NetDataWriter writer, ref T packet) where T : INetSerializable
		{
			this.WriteHash<T>(writer);
			packet.Serialize(writer);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x0000AF8F File Offset: 0x0000918F
		public void ReadPacket(NetDataReader reader, object userData)
		{
			this.GetCallbackFromData(reader)(reader, userData);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000AFA0 File Offset: 0x000091A0
		public void Subscribe<T>(Action<T> onReceive, Func<T> packetConstructor) where T : class, new()
		{
			this._netSerializer.Register<T>();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				T t = packetConstructor();
				this._netSerializer.Deserialize<T>(reader, t);
				onReceive(t);
			};
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000AFF0 File Offset: 0x000091F0
		public void Subscribe<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : class, new()
		{
			this._netSerializer.Register<T>();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				T t = packetConstructor();
				this._netSerializer.Deserialize<T>(reader, t);
				onReceive(t, (TUserData)((object)userData));
			};
		}

		// Token: 0x06000244 RID: 580 RVA: 0x0000B040 File Offset: 0x00009240
		public void SubscribeReusable<T>(Action<T> onReceive) where T : class, new()
		{
			this._netSerializer.Register<T>();
			T reference = new T();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				this._netSerializer.Deserialize<T>(reader, reference);
				onReceive(reference);
			};
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000B094 File Offset: 0x00009294
		public void SubscribeReusable<T, TUserData>(Action<T, TUserData> onReceive) where T : class, new()
		{
			this._netSerializer.Register<T>();
			T reference = new T();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				this._netSerializer.Deserialize<T>(reader, reference);
				onReceive(reference, (TUserData)((object)userData));
			};
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000B0E8 File Offset: 0x000092E8
		public void SubscribeNetSerializable<T, TUserData>(Action<T, TUserData> onReceive, Func<T> packetConstructor) where T : INetSerializable
		{
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				T t = packetConstructor();
				t.Deserialize(reader);
				onReceive(t, (TUserData)((object)userData));
			};
		}

		// Token: 0x06000247 RID: 583 RVA: 0x0000B128 File Offset: 0x00009328
		public void SubscribeNetSerializable<T>(Action<T> onReceive, Func<T> packetConstructor) where T : INetSerializable
		{
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				T t = packetConstructor();
				t.Deserialize(reader);
				onReceive(t);
			};
		}

		// Token: 0x06000248 RID: 584 RVA: 0x0000B168 File Offset: 0x00009368
		public void SubscribeNetSerializable<T, TUserData>(Action<T, TUserData> onReceive) where T : INetSerializable, new()
		{
			T reference = new T();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				reference.Deserialize(reader);
				onReceive(reference, (TUserData)((object)userData));
			};
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000B1AC File Offset: 0x000093AC
		public void SubscribeNetSerializable<T>(Action<T> onReceive) where T : INetSerializable, new()
		{
			T reference = new T();
			this._callbacks[this.GetHash<T>()] = delegate(NetDataReader reader, object userData)
			{
				reference.Deserialize(reader);
				onReceive(reference);
			};
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000B1EE File Offset: 0x000093EE
		public bool RemoveSubscription<T>()
		{
			return this._callbacks.Remove(this.GetHash<T>());
		}

		// Token: 0x04000175 RID: 373
		private readonly NetSerializer _netSerializer;

		// Token: 0x04000176 RID: 374
		private readonly Dictionary<ulong, NetPacketProcessor.SubscribeDelegate> _callbacks = new Dictionary<ulong, NetPacketProcessor.SubscribeDelegate>();

		// Token: 0x02000090 RID: 144
		private static class HashCache<T>
		{
			// Token: 0x06000487 RID: 1159 RVA: 0x00012B60 File Offset: 0x00010D60
			static HashCache()
			{
				ulong num = 14695981039346656037UL;
				string text = typeof(T).ToString();
				for (int i = 0; i < text.Length; i++)
				{
					num ^= (ulong)text[i];
					num *= 1099511628211UL;
				}
				NetPacketProcessor.HashCache<T>.Id = num;
			}

			// Token: 0x040002EB RID: 747
			public static readonly ulong Id;
		}

		// Token: 0x02000091 RID: 145
		// (Invoke) Token: 0x06000489 RID: 1161
		protected delegate void SubscribeDelegate(NetDataReader reader, object userData);
	}
}
