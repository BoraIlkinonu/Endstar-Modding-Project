using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace Networking.UDP.Utils
{
	// Token: 0x02000040 RID: 64
	public class NetSerializer
	{
		// Token: 0x0600024D RID: 589 RVA: 0x0000B213 File Offset: 0x00009413
		public void RegisterNestedType<T>() where T : struct, INetSerializable
		{
			this._registeredTypes.Add(typeof(T), new NetSerializer.CustomTypeStruct<T>());
		}

		// Token: 0x0600024E RID: 590 RVA: 0x0000B22F File Offset: 0x0000942F
		public void RegisterNestedType<T>(Func<T> constructor) where T : class, INetSerializable
		{
			this._registeredTypes.Add(typeof(T), new NetSerializer.CustomTypeClass<T>(constructor));
		}

		// Token: 0x0600024F RID: 591 RVA: 0x0000B24C File Offset: 0x0000944C
		public void RegisterNestedType<T>(Action<NetDataWriter, T> writer, Func<NetDataReader, T> reader)
		{
			this._registeredTypes.Add(typeof(T), new NetSerializer.CustomTypeStatic<T>(writer, reader));
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000B26A File Offset: 0x0000946A
		public NetSerializer()
			: this(0)
		{
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000B273 File Offset: 0x00009473
		public NetSerializer(int maxStringLength)
		{
			this._maxStringLength = maxStringLength;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x0000B290 File Offset: 0x00009490
		private NetSerializer.ClassInfo<T> RegisterInternal<T>()
		{
			if (NetSerializer.ClassInfo<T>.Instance != null)
			{
				return NetSerializer.ClassInfo<T>.Instance;
			}
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			List<NetSerializer.FastCall<T>> list = new List<NetSerializer.FastCall<T>>();
			foreach (PropertyInfo propertyInfo in properties)
			{
				Type propertyType = propertyInfo.PropertyType;
				Type type = (propertyType.IsArray ? propertyType.GetElementType() : propertyType);
				NetSerializer.CallType callType = (propertyType.IsArray ? NetSerializer.CallType.Array : NetSerializer.CallType.Basic);
				if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
				{
					type = propertyType.GetGenericArguments()[0];
					callType = NetSerializer.CallType.List;
				}
				if (!Attribute.IsDefined(propertyInfo, typeof(IgnoreDataMemberAttribute)))
				{
					MethodInfo getMethod = propertyInfo.GetGetMethod();
					MethodInfo setMethod = propertyInfo.GetSetMethod();
					if (!(getMethod == null) && !(setMethod == null))
					{
						NetSerializer.FastCall<T> fastCall = null;
						if (propertyType.IsEnum)
						{
							Type underlyingType = Enum.GetUnderlyingType(propertyType);
							if (underlyingType == typeof(byte))
							{
								fastCall = new NetSerializer.EnumByteSerializer<T>(propertyInfo, propertyType);
							}
							else
							{
								if (!(underlyingType == typeof(int)))
								{
									throw new InvalidTypeException("Not supported enum underlying type: " + underlyingType.Name);
								}
								fastCall = new NetSerializer.EnumIntSerializer<T>(propertyInfo, propertyType);
							}
						}
						else if (type == typeof(string))
						{
							fastCall = new NetSerializer.StringSerializer<T>(this._maxStringLength);
						}
						else if (type == typeof(bool))
						{
							fastCall = new NetSerializer.BoolSerializer<T>();
						}
						else if (type == typeof(byte))
						{
							fastCall = new NetSerializer.ByteSerializer<T>();
						}
						else if (type == typeof(sbyte))
						{
							fastCall = new NetSerializer.SByteSerializer<T>();
						}
						else if (type == typeof(short))
						{
							fastCall = new NetSerializer.ShortSerializer<T>();
						}
						else if (type == typeof(ushort))
						{
							fastCall = new NetSerializer.UShortSerializer<T>();
						}
						else if (type == typeof(int))
						{
							fastCall = new NetSerializer.IntSerializer<T>();
						}
						else if (type == typeof(uint))
						{
							fastCall = new NetSerializer.UIntSerializer<T>();
						}
						else if (type == typeof(long))
						{
							fastCall = new NetSerializer.LongSerializer<T>();
						}
						else if (type == typeof(ulong))
						{
							fastCall = new NetSerializer.ULongSerializer<T>();
						}
						else if (type == typeof(float))
						{
							fastCall = new NetSerializer.FloatSerializer<T>();
						}
						else if (type == typeof(double))
						{
							fastCall = new NetSerializer.DoubleSerializer<T>();
						}
						else if (type == typeof(char))
						{
							fastCall = new NetSerializer.CharSerializer<T>();
						}
						else if (type == typeof(IPEndPoint))
						{
							fastCall = new NetSerializer.IPEndPointSerializer<T>();
						}
						else if (type == typeof(Guid))
						{
							fastCall = new NetSerializer.GuidSerializer<T>();
						}
						else
						{
							NetSerializer.CustomType customType;
							this._registeredTypes.TryGetValue(type, out customType);
							if (customType != null)
							{
								fastCall = customType.Get<T>();
							}
						}
						if (fastCall == null)
						{
							throw new InvalidTypeException("Unknown property type: " + propertyType.FullName);
						}
						fastCall.Init(getMethod, setMethod, callType);
						list.Add(fastCall);
					}
				}
			}
			NetSerializer.ClassInfo<T>.Instance = new NetSerializer.ClassInfo<T>(list);
			return NetSerializer.ClassInfo<T>.Instance;
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000B612 File Offset: 0x00009812
		public void Register<T>()
		{
			this.RegisterInternal<T>();
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000B61C File Offset: 0x0000981C
		public T Deserialize<T>(NetDataReader reader) where T : class, new()
		{
			NetSerializer.ClassInfo<T> classInfo = this.RegisterInternal<T>();
			T t = new T();
			try
			{
				classInfo.Read(t, reader);
			}
			catch
			{
				return default(T);
			}
			return t;
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000B660 File Offset: 0x00009860
		public bool Deserialize<T>(NetDataReader reader, T target) where T : class, new()
		{
			NetSerializer.ClassInfo<T> classInfo = this.RegisterInternal<T>();
			try
			{
				classInfo.Read(target, reader);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000B698 File Offset: 0x00009898
		public void Serialize<T>(NetDataWriter writer, T obj) where T : class, new()
		{
			this.RegisterInternal<T>().Write(obj, writer);
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000B6A7 File Offset: 0x000098A7
		public byte[] Serialize<T>(T obj) where T : class, new()
		{
			if (this._writer == null)
			{
				this._writer = new NetDataWriter();
			}
			this._writer.Reset();
			this.Serialize<T>(this._writer, obj);
			return this._writer.CopyData();
		}

		// Token: 0x04000177 RID: 375
		private NetDataWriter _writer;

		// Token: 0x04000178 RID: 376
		private readonly int _maxStringLength;

		// Token: 0x04000179 RID: 377
		private readonly Dictionary<Type, NetSerializer.CustomType> _registeredTypes = new Dictionary<Type, NetSerializer.CustomType>();

		// Token: 0x0200009A RID: 154
		private enum CallType
		{
			// Token: 0x04000301 RID: 769
			Basic,
			// Token: 0x04000302 RID: 770
			Array,
			// Token: 0x04000303 RID: 771
			List
		}

		// Token: 0x0200009B RID: 155
		private abstract class FastCall<T>
		{
			// Token: 0x0600049C RID: 1180 RVA: 0x00012D8B File Offset: 0x00010F8B
			public virtual void Init(MethodInfo getMethod, MethodInfo setMethod, NetSerializer.CallType type)
			{
				this.Type = type;
			}

			// Token: 0x0600049D RID: 1181
			public abstract void Read(T inf, NetDataReader r);

			// Token: 0x0600049E RID: 1182
			public abstract void Write(T inf, NetDataWriter w);

			// Token: 0x0600049F RID: 1183
			public abstract void ReadArray(T inf, NetDataReader r);

			// Token: 0x060004A0 RID: 1184
			public abstract void WriteArray(T inf, NetDataWriter w);

			// Token: 0x060004A1 RID: 1185
			public abstract void ReadList(T inf, NetDataReader r);

			// Token: 0x060004A2 RID: 1186
			public abstract void WriteList(T inf, NetDataWriter w);

			// Token: 0x04000304 RID: 772
			public NetSerializer.CallType Type;
		}

		// Token: 0x0200009C RID: 156
		private abstract class FastCallSpecific<TClass, TProperty> : NetSerializer.FastCall<TClass>
		{
			// Token: 0x060004A4 RID: 1188 RVA: 0x00012D9C File Offset: 0x00010F9C
			public override void ReadArray(TClass inf, NetDataReader r)
			{
				string text = "Unsupported type: ";
				Type typeFromHandle = typeof(TProperty);
				throw new InvalidTypeException(text + ((typeFromHandle != null) ? typeFromHandle.ToString() : null) + "[]");
			}

			// Token: 0x060004A5 RID: 1189 RVA: 0x00012DC8 File Offset: 0x00010FC8
			public override void WriteArray(TClass inf, NetDataWriter w)
			{
				string text = "Unsupported type: ";
				Type typeFromHandle = typeof(TProperty);
				throw new InvalidTypeException(text + ((typeFromHandle != null) ? typeFromHandle.ToString() : null) + "[]");
			}

			// Token: 0x060004A6 RID: 1190 RVA: 0x00012DF4 File Offset: 0x00010FF4
			public override void ReadList(TClass inf, NetDataReader r)
			{
				string text = "Unsupported type: List<";
				Type typeFromHandle = typeof(TProperty);
				throw new InvalidTypeException(text + ((typeFromHandle != null) ? typeFromHandle.ToString() : null) + ">");
			}

			// Token: 0x060004A7 RID: 1191 RVA: 0x00012E20 File Offset: 0x00011020
			public override void WriteList(TClass inf, NetDataWriter w)
			{
				string text = "Unsupported type: List<";
				Type typeFromHandle = typeof(TProperty);
				throw new InvalidTypeException(text + ((typeFromHandle != null) ? typeFromHandle.ToString() : null) + ">");
			}

			// Token: 0x060004A8 RID: 1192 RVA: 0x00012E4C File Offset: 0x0001104C
			protected TProperty[] ReadArrayHelper(TClass inf, NetDataReader r)
			{
				ushort @ushort = r.GetUShort();
				TProperty[] array = this.GetterArr(inf);
				array = ((array == null || array.Length != (int)@ushort) ? new TProperty[(int)@ushort] : array);
				this.SetterArr(inf, array);
				return array;
			}

			// Token: 0x060004A9 RID: 1193 RVA: 0x00012E90 File Offset: 0x00011090
			protected TProperty[] WriteArrayHelper(TClass inf, NetDataWriter w)
			{
				TProperty[] array = this.GetterArr(inf);
				w.Put((ushort)array.Length);
				return array;
			}

			// Token: 0x060004AA RID: 1194 RVA: 0x00012EB8 File Offset: 0x000110B8
			protected List<TProperty> ReadListHelper(TClass inf, NetDataReader r, out int len)
			{
				len = (int)r.GetUShort();
				List<TProperty> list = this.GetterList(inf);
				if (list == null)
				{
					list = new List<TProperty>(len);
					this.SetterList(inf, list);
				}
				return list;
			}

			// Token: 0x060004AB RID: 1195 RVA: 0x00012EF4 File Offset: 0x000110F4
			protected List<TProperty> WriteListHelper(TClass inf, NetDataWriter w, out int len)
			{
				List<TProperty> list = this.GetterList(inf);
				if (list == null)
				{
					len = 0;
					w.Put(0);
					return null;
				}
				len = list.Count;
				w.Put((ushort)len);
				return list;
			}

			// Token: 0x060004AC RID: 1196 RVA: 0x00012F30 File Offset: 0x00011130
			public override void Init(MethodInfo getMethod, MethodInfo setMethod, NetSerializer.CallType type)
			{
				base.Init(getMethod, setMethod, type);
				if (type == NetSerializer.CallType.Array)
				{
					this.GetterArr = (Func<TClass, TProperty[]>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty[]>), getMethod);
					this.SetterArr = (Action<TClass, TProperty[]>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty[]>), setMethod);
					return;
				}
				if (type != NetSerializer.CallType.List)
				{
					this.Getter = (Func<TClass, TProperty>)Delegate.CreateDelegate(typeof(Func<TClass, TProperty>), getMethod);
					this.Setter = (Action<TClass, TProperty>)Delegate.CreateDelegate(typeof(Action<TClass, TProperty>), setMethod);
					return;
				}
				this.GetterList = (Func<TClass, List<TProperty>>)Delegate.CreateDelegate(typeof(Func<TClass, List<TProperty>>), getMethod);
				this.SetterList = (Action<TClass, List<TProperty>>)Delegate.CreateDelegate(typeof(Action<TClass, List<TProperty>>), setMethod);
			}

			// Token: 0x04000305 RID: 773
			protected Func<TClass, TProperty> Getter;

			// Token: 0x04000306 RID: 774
			protected Action<TClass, TProperty> Setter;

			// Token: 0x04000307 RID: 775
			protected Func<TClass, TProperty[]> GetterArr;

			// Token: 0x04000308 RID: 776
			protected Action<TClass, TProperty[]> SetterArr;

			// Token: 0x04000309 RID: 777
			protected Func<TClass, List<TProperty>> GetterList;

			// Token: 0x0400030A RID: 778
			protected Action<TClass, List<TProperty>> SetterList;
		}

		// Token: 0x0200009D RID: 157
		private abstract class FastCallSpecificAuto<TClass, TProperty> : NetSerializer.FastCallSpecific<TClass, TProperty>
		{
			// Token: 0x060004AE RID: 1198
			protected abstract void ElementRead(NetDataReader r, out TProperty prop);

			// Token: 0x060004AF RID: 1199
			protected abstract void ElementWrite(NetDataWriter w, ref TProperty prop);

			// Token: 0x060004B0 RID: 1200 RVA: 0x00012FFC File Offset: 0x000111FC
			public override void Read(TClass inf, NetDataReader r)
			{
				TProperty tproperty;
				this.ElementRead(r, out tproperty);
				this.Setter(inf, tproperty);
			}

			// Token: 0x060004B1 RID: 1201 RVA: 0x00013020 File Offset: 0x00011220
			public override void Write(TClass inf, NetDataWriter w)
			{
				TProperty tproperty = this.Getter(inf);
				this.ElementWrite(w, ref tproperty);
			}

			// Token: 0x060004B2 RID: 1202 RVA: 0x00013044 File Offset: 0x00011244
			public override void ReadArray(TClass inf, NetDataReader r)
			{
				TProperty[] array = base.ReadArrayHelper(inf, r);
				for (int i = 0; i < array.Length; i++)
				{
					this.ElementRead(r, out array[i]);
				}
			}

			// Token: 0x060004B3 RID: 1203 RVA: 0x00013078 File Offset: 0x00011278
			public override void WriteArray(TClass inf, NetDataWriter w)
			{
				TProperty[] array = base.WriteArrayHelper(inf, w);
				for (int i = 0; i < array.Length; i++)
				{
					this.ElementWrite(w, ref array[i]);
				}
			}
		}

		// Token: 0x0200009E RID: 158
		private sealed class FastCallStatic<TClass, TProperty> : NetSerializer.FastCallSpecific<TClass, TProperty>
		{
			// Token: 0x060004B5 RID: 1205 RVA: 0x000130B2 File Offset: 0x000112B2
			public FastCallStatic(Action<NetDataWriter, TProperty> write, Func<NetDataReader, TProperty> read)
			{
				this._writer = write;
				this._reader = read;
			}

			// Token: 0x060004B6 RID: 1206 RVA: 0x000130C8 File Offset: 0x000112C8
			public override void Read(TClass inf, NetDataReader r)
			{
				this.Setter(inf, this._reader(r));
			}

			// Token: 0x060004B7 RID: 1207 RVA: 0x000130E2 File Offset: 0x000112E2
			public override void Write(TClass inf, NetDataWriter w)
			{
				this._writer(w, this.Getter(inf));
			}

			// Token: 0x060004B8 RID: 1208 RVA: 0x000130FC File Offset: 0x000112FC
			public override void ReadList(TClass inf, NetDataReader r)
			{
				int num;
				List<TProperty> list = base.ReadListHelper(inf, r, out num);
				int count = list.Count;
				for (int i = 0; i < num; i++)
				{
					if (i < count)
					{
						list[i] = this._reader(r);
					}
					else
					{
						list.Add(this._reader(r));
					}
				}
				if (num < count)
				{
					list.RemoveRange(num, count - num);
				}
			}

			// Token: 0x060004B9 RID: 1209 RVA: 0x00013160 File Offset: 0x00011360
			public override void WriteList(TClass inf, NetDataWriter w)
			{
				int num;
				List<TProperty> list = base.WriteListHelper(inf, w, out num);
				for (int i = 0; i < num; i++)
				{
					this._writer(w, list[i]);
				}
			}

			// Token: 0x060004BA RID: 1210 RVA: 0x00013198 File Offset: 0x00011398
			public override void ReadArray(TClass inf, NetDataReader r)
			{
				TProperty[] array = base.ReadArrayHelper(inf, r);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i] = this._reader(r);
				}
			}

			// Token: 0x060004BB RID: 1211 RVA: 0x000131D4 File Offset: 0x000113D4
			public override void WriteArray(TClass inf, NetDataWriter w)
			{
				TProperty[] array = base.WriteArrayHelper(inf, w);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					this._writer(w, array[i]);
				}
			}

			// Token: 0x0400030B RID: 779
			private readonly Action<NetDataWriter, TProperty> _writer;

			// Token: 0x0400030C RID: 780
			private readonly Func<NetDataReader, TProperty> _reader;
		}

		// Token: 0x0200009F RID: 159
		private sealed class FastCallStruct<TClass, TProperty> : NetSerializer.FastCallSpecific<TClass, TProperty> where TProperty : struct, INetSerializable
		{
			// Token: 0x060004BC RID: 1212 RVA: 0x0001320D File Offset: 0x0001140D
			public override void Read(TClass inf, NetDataReader r)
			{
				this._p.Deserialize(r);
				this.Setter(inf, this._p);
			}

			// Token: 0x060004BD RID: 1213 RVA: 0x00013233 File Offset: 0x00011433
			public override void Write(TClass inf, NetDataWriter w)
			{
				this._p = this.Getter(inf);
				this._p.Serialize(w);
			}

			// Token: 0x060004BE RID: 1214 RVA: 0x0001325C File Offset: 0x0001145C
			public override void ReadList(TClass inf, NetDataReader r)
			{
				int num;
				List<TProperty> list = base.ReadListHelper(inf, r, out num);
				int count = list.Count;
				for (int i = 0; i < num; i++)
				{
					TProperty tproperty = default(TProperty);
					tproperty.Deserialize(r);
					if (i < count)
					{
						list[i] = tproperty;
					}
					else
					{
						list.Add(tproperty);
					}
				}
				if (num < count)
				{
					list.RemoveRange(num, count - num);
				}
			}

			// Token: 0x060004BF RID: 1215 RVA: 0x000132C4 File Offset: 0x000114C4
			public override void WriteList(TClass inf, NetDataWriter w)
			{
				int num;
				List<TProperty> list = base.WriteListHelper(inf, w, out num);
				for (int i = 0; i < num; i++)
				{
					TProperty tproperty = list[i];
					tproperty.Serialize(w);
				}
			}

			// Token: 0x060004C0 RID: 1216 RVA: 0x00013300 File Offset: 0x00011500
			public override void ReadArray(TClass inf, NetDataReader r)
			{
				TProperty[] array = base.ReadArrayHelper(inf, r);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i].Deserialize(r);
				}
			}

			// Token: 0x060004C1 RID: 1217 RVA: 0x0001333C File Offset: 0x0001153C
			public override void WriteArray(TClass inf, NetDataWriter w)
			{
				TProperty[] array = base.WriteArrayHelper(inf, w);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i].Serialize(w);
				}
			}

			// Token: 0x0400030D RID: 781
			private TProperty _p;
		}

		// Token: 0x020000A0 RID: 160
		private sealed class FastCallClass<TClass, TProperty> : NetSerializer.FastCallSpecific<TClass, TProperty> where TProperty : class, INetSerializable
		{
			// Token: 0x060004C3 RID: 1219 RVA: 0x0001337F File Offset: 0x0001157F
			public FastCallClass(Func<TProperty> constructor)
			{
				this._constructor = constructor;
			}

			// Token: 0x060004C4 RID: 1220 RVA: 0x00013390 File Offset: 0x00011590
			public override void Read(TClass inf, NetDataReader r)
			{
				TProperty tproperty = this._constructor();
				tproperty.Deserialize(r);
				this.Setter(inf, tproperty);
			}

			// Token: 0x060004C5 RID: 1221 RVA: 0x000133C2 File Offset: 0x000115C2
			public override void Write(TClass inf, NetDataWriter w)
			{
				TProperty tproperty = this.Getter(inf);
				if (tproperty == null)
				{
					return;
				}
				tproperty.Serialize(w);
			}

			// Token: 0x060004C6 RID: 1222 RVA: 0x000133E0 File Offset: 0x000115E0
			public override void ReadList(TClass inf, NetDataReader r)
			{
				int num;
				List<TProperty> list = base.ReadListHelper(inf, r, out num);
				int count = list.Count;
				for (int i = 0; i < num; i++)
				{
					if (i < count)
					{
						list[i].Deserialize(r);
					}
					else
					{
						TProperty tproperty = this._constructor();
						tproperty.Deserialize(r);
						list.Add(tproperty);
					}
				}
				if (num < count)
				{
					list.RemoveRange(num, count - num);
				}
			}

			// Token: 0x060004C7 RID: 1223 RVA: 0x00013454 File Offset: 0x00011654
			public override void WriteList(TClass inf, NetDataWriter w)
			{
				int num;
				List<TProperty> list = base.WriteListHelper(inf, w, out num);
				for (int i = 0; i < num; i++)
				{
					list[i].Serialize(w);
				}
			}

			// Token: 0x060004C8 RID: 1224 RVA: 0x0001348C File Offset: 0x0001168C
			public override void ReadArray(TClass inf, NetDataReader r)
			{
				TProperty[] array = base.ReadArrayHelper(inf, r);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i] = this._constructor();
					array[i].Deserialize(r);
				}
			}

			// Token: 0x060004C9 RID: 1225 RVA: 0x000134D8 File Offset: 0x000116D8
			public override void WriteArray(TClass inf, NetDataWriter w)
			{
				TProperty[] array = base.WriteArrayHelper(inf, w);
				int num = array.Length;
				for (int i = 0; i < num; i++)
				{
					array[i].Serialize(w);
				}
			}

			// Token: 0x0400030E RID: 782
			private readonly Func<TProperty> _constructor;
		}

		// Token: 0x020000A1 RID: 161
		private class IntSerializer<T> : NetSerializer.FastCallSpecific<T, int>
		{
			// Token: 0x060004CA RID: 1226 RVA: 0x00013510 File Offset: 0x00011710
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetInt());
			}

			// Token: 0x060004CB RID: 1227 RVA: 0x00013524 File Offset: 0x00011724
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004CC RID: 1228 RVA: 0x00013538 File Offset: 0x00011738
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetIntArray());
			}

			// Token: 0x060004CD RID: 1229 RVA: 0x0001354C File Offset: 0x0001174C
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A2 RID: 162
		private class UIntSerializer<T> : NetSerializer.FastCallSpecific<T, uint>
		{
			// Token: 0x060004CF RID: 1231 RVA: 0x00013568 File Offset: 0x00011768
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetUInt());
			}

			// Token: 0x060004D0 RID: 1232 RVA: 0x0001357C File Offset: 0x0001177C
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004D1 RID: 1233 RVA: 0x00013590 File Offset: 0x00011790
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetUIntArray());
			}

			// Token: 0x060004D2 RID: 1234 RVA: 0x000135A4 File Offset: 0x000117A4
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A3 RID: 163
		private class ShortSerializer<T> : NetSerializer.FastCallSpecific<T, short>
		{
			// Token: 0x060004D4 RID: 1236 RVA: 0x000135C0 File Offset: 0x000117C0
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetShort());
			}

			// Token: 0x060004D5 RID: 1237 RVA: 0x000135D4 File Offset: 0x000117D4
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004D6 RID: 1238 RVA: 0x000135E8 File Offset: 0x000117E8
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetShortArray());
			}

			// Token: 0x060004D7 RID: 1239 RVA: 0x000135FC File Offset: 0x000117FC
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A4 RID: 164
		private class UShortSerializer<T> : NetSerializer.FastCallSpecific<T, ushort>
		{
			// Token: 0x060004D9 RID: 1241 RVA: 0x00013618 File Offset: 0x00011818
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetUShort());
			}

			// Token: 0x060004DA RID: 1242 RVA: 0x0001362C File Offset: 0x0001182C
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004DB RID: 1243 RVA: 0x00013640 File Offset: 0x00011840
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetUShortArray());
			}

			// Token: 0x060004DC RID: 1244 RVA: 0x00013654 File Offset: 0x00011854
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A5 RID: 165
		private class LongSerializer<T> : NetSerializer.FastCallSpecific<T, long>
		{
			// Token: 0x060004DE RID: 1246 RVA: 0x00013670 File Offset: 0x00011870
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetLong());
			}

			// Token: 0x060004DF RID: 1247 RVA: 0x00013684 File Offset: 0x00011884
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004E0 RID: 1248 RVA: 0x00013698 File Offset: 0x00011898
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetLongArray());
			}

			// Token: 0x060004E1 RID: 1249 RVA: 0x000136AC File Offset: 0x000118AC
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A6 RID: 166
		private class ULongSerializer<T> : NetSerializer.FastCallSpecific<T, ulong>
		{
			// Token: 0x060004E3 RID: 1251 RVA: 0x000136C8 File Offset: 0x000118C8
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetULong());
			}

			// Token: 0x060004E4 RID: 1252 RVA: 0x000136DC File Offset: 0x000118DC
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004E5 RID: 1253 RVA: 0x000136F0 File Offset: 0x000118F0
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetULongArray());
			}

			// Token: 0x060004E6 RID: 1254 RVA: 0x00013704 File Offset: 0x00011904
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A7 RID: 167
		private class ByteSerializer<T> : NetSerializer.FastCallSpecific<T, byte>
		{
			// Token: 0x060004E8 RID: 1256 RVA: 0x00013720 File Offset: 0x00011920
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetByte());
			}

			// Token: 0x060004E9 RID: 1257 RVA: 0x00013734 File Offset: 0x00011934
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004EA RID: 1258 RVA: 0x00013748 File Offset: 0x00011948
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetBytesWithLength());
			}

			// Token: 0x060004EB RID: 1259 RVA: 0x0001375C File Offset: 0x0001195C
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutBytesWithLength(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A8 RID: 168
		private class SByteSerializer<T> : NetSerializer.FastCallSpecific<T, sbyte>
		{
			// Token: 0x060004ED RID: 1261 RVA: 0x00013778 File Offset: 0x00011978
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetSByte());
			}

			// Token: 0x060004EE RID: 1262 RVA: 0x0001378C File Offset: 0x0001198C
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004EF RID: 1263 RVA: 0x000137A0 File Offset: 0x000119A0
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetSBytesWithLength());
			}

			// Token: 0x060004F0 RID: 1264 RVA: 0x000137B4 File Offset: 0x000119B4
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutSBytesWithLength(this.GetterArr(inf));
			}
		}

		// Token: 0x020000A9 RID: 169
		private class FloatSerializer<T> : NetSerializer.FastCallSpecific<T, float>
		{
			// Token: 0x060004F2 RID: 1266 RVA: 0x000137D0 File Offset: 0x000119D0
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetFloat());
			}

			// Token: 0x060004F3 RID: 1267 RVA: 0x000137E4 File Offset: 0x000119E4
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004F4 RID: 1268 RVA: 0x000137F8 File Offset: 0x000119F8
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetFloatArray());
			}

			// Token: 0x060004F5 RID: 1269 RVA: 0x0001380C File Offset: 0x00011A0C
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000AA RID: 170
		private class DoubleSerializer<T> : NetSerializer.FastCallSpecific<T, double>
		{
			// Token: 0x060004F7 RID: 1271 RVA: 0x00013828 File Offset: 0x00011A28
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetDouble());
			}

			// Token: 0x060004F8 RID: 1272 RVA: 0x0001383C File Offset: 0x00011A3C
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004F9 RID: 1273 RVA: 0x00013850 File Offset: 0x00011A50
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetDoubleArray());
			}

			// Token: 0x060004FA RID: 1274 RVA: 0x00013864 File Offset: 0x00011A64
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000AB RID: 171
		private class BoolSerializer<T> : NetSerializer.FastCallSpecific<T, bool>
		{
			// Token: 0x060004FC RID: 1276 RVA: 0x00013880 File Offset: 0x00011A80
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetBool());
			}

			// Token: 0x060004FD RID: 1277 RVA: 0x00013894 File Offset: 0x00011A94
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf));
			}

			// Token: 0x060004FE RID: 1278 RVA: 0x000138A8 File Offset: 0x00011AA8
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetBoolArray());
			}

			// Token: 0x060004FF RID: 1279 RVA: 0x000138BC File Offset: 0x00011ABC
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf));
			}
		}

		// Token: 0x020000AC RID: 172
		private class CharSerializer<T> : NetSerializer.FastCallSpecificAuto<T, char>
		{
			// Token: 0x06000501 RID: 1281 RVA: 0x000138D8 File Offset: 0x00011AD8
			protected override void ElementWrite(NetDataWriter w, ref char prop)
			{
				w.Put(prop);
			}

			// Token: 0x06000502 RID: 1282 RVA: 0x000138E2 File Offset: 0x00011AE2
			protected override void ElementRead(NetDataReader r, out char prop)
			{
				prop = r.GetChar();
			}
		}

		// Token: 0x020000AD RID: 173
		private class IPEndPointSerializer<T> : NetSerializer.FastCallSpecificAuto<T, IPEndPoint>
		{
			// Token: 0x06000504 RID: 1284 RVA: 0x000138F4 File Offset: 0x00011AF4
			protected override void ElementWrite(NetDataWriter w, ref IPEndPoint prop)
			{
				w.Put(prop);
			}

			// Token: 0x06000505 RID: 1285 RVA: 0x000138FE File Offset: 0x00011AFE
			protected override void ElementRead(NetDataReader r, out IPEndPoint prop)
			{
				prop = r.GetNetEndPoint();
			}
		}

		// Token: 0x020000AE RID: 174
		private class GuidSerializer<T> : NetSerializer.FastCallSpecificAuto<T, Guid>
		{
			// Token: 0x06000507 RID: 1287 RVA: 0x00013910 File Offset: 0x00011B10
			protected override void ElementWrite(NetDataWriter w, ref Guid guid)
			{
				w.Put(guid);
			}

			// Token: 0x06000508 RID: 1288 RVA: 0x0001391E File Offset: 0x00011B1E
			protected override void ElementRead(NetDataReader r, out Guid guid)
			{
				guid = r.GetGuid();
			}
		}

		// Token: 0x020000AF RID: 175
		private class StringSerializer<T> : NetSerializer.FastCallSpecific<T, string>
		{
			// Token: 0x0600050A RID: 1290 RVA: 0x00013934 File Offset: 0x00011B34
			public StringSerializer(int maxLength)
			{
				this._maxLength = ((maxLength > 0) ? maxLength : 32767);
			}

			// Token: 0x0600050B RID: 1291 RVA: 0x0001394E File Offset: 0x00011B4E
			public override void Read(T inf, NetDataReader r)
			{
				this.Setter(inf, r.GetString(this._maxLength));
			}

			// Token: 0x0600050C RID: 1292 RVA: 0x00013968 File Offset: 0x00011B68
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put(this.Getter(inf), this._maxLength);
			}

			// Token: 0x0600050D RID: 1293 RVA: 0x00013982 File Offset: 0x00011B82
			public override void ReadArray(T inf, NetDataReader r)
			{
				this.SetterArr(inf, r.GetStringArray(this._maxLength));
			}

			// Token: 0x0600050E RID: 1294 RVA: 0x0001399C File Offset: 0x00011B9C
			public override void WriteArray(T inf, NetDataWriter w)
			{
				w.PutArray(this.GetterArr(inf), this._maxLength);
			}

			// Token: 0x0400030F RID: 783
			private readonly int _maxLength;
		}

		// Token: 0x020000B0 RID: 176
		private class EnumByteSerializer<T> : NetSerializer.FastCall<T>
		{
			// Token: 0x0600050F RID: 1295 RVA: 0x000139B6 File Offset: 0x00011BB6
			public EnumByteSerializer(PropertyInfo property, Type propertyType)
			{
				this.Property = property;
				this.PropertyType = propertyType;
			}

			// Token: 0x06000510 RID: 1296 RVA: 0x000139CC File Offset: 0x00011BCC
			public override void Read(T inf, NetDataReader r)
			{
				this.Property.SetValue(inf, Enum.ToObject(this.PropertyType, r.GetByte()), null);
			}

			// Token: 0x06000511 RID: 1297 RVA: 0x000139F1 File Offset: 0x00011BF1
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put((byte)this.Property.GetValue(inf, null));
			}

			// Token: 0x06000512 RID: 1298 RVA: 0x00013A10 File Offset: 0x00011C10
			public override void ReadArray(T inf, NetDataReader r)
			{
				throw new InvalidTypeException("Unsupported type: Enum[]");
			}

			// Token: 0x06000513 RID: 1299 RVA: 0x00013A1C File Offset: 0x00011C1C
			public override void WriteArray(T inf, NetDataWriter w)
			{
				throw new InvalidTypeException("Unsupported type: Enum[]");
			}

			// Token: 0x06000514 RID: 1300 RVA: 0x00013A28 File Offset: 0x00011C28
			public override void ReadList(T inf, NetDataReader r)
			{
				throw new InvalidTypeException("Unsupported type: List<Enum>");
			}

			// Token: 0x06000515 RID: 1301 RVA: 0x00013A34 File Offset: 0x00011C34
			public override void WriteList(T inf, NetDataWriter w)
			{
				throw new InvalidTypeException("Unsupported type: List<Enum>");
			}

			// Token: 0x04000310 RID: 784
			protected readonly PropertyInfo Property;

			// Token: 0x04000311 RID: 785
			protected readonly Type PropertyType;
		}

		// Token: 0x020000B1 RID: 177
		private class EnumIntSerializer<T> : NetSerializer.EnumByteSerializer<T>
		{
			// Token: 0x06000516 RID: 1302 RVA: 0x00013A40 File Offset: 0x00011C40
			public EnumIntSerializer(PropertyInfo property, Type propertyType)
				: base(property, propertyType)
			{
			}

			// Token: 0x06000517 RID: 1303 RVA: 0x00013A4A File Offset: 0x00011C4A
			public override void Read(T inf, NetDataReader r)
			{
				this.Property.SetValue(inf, Enum.ToObject(this.PropertyType, r.GetInt()), null);
			}

			// Token: 0x06000518 RID: 1304 RVA: 0x00013A6F File Offset: 0x00011C6F
			public override void Write(T inf, NetDataWriter w)
			{
				w.Put((int)this.Property.GetValue(inf, null));
			}
		}

		// Token: 0x020000B2 RID: 178
		private sealed class ClassInfo<T>
		{
			// Token: 0x06000519 RID: 1305 RVA: 0x00013A8E File Offset: 0x00011C8E
			public ClassInfo(List<NetSerializer.FastCall<T>> serializers)
			{
				this._membersCount = serializers.Count;
				this._serializers = serializers.ToArray();
			}

			// Token: 0x0600051A RID: 1306 RVA: 0x00013AB0 File Offset: 0x00011CB0
			public void Write(T obj, NetDataWriter writer)
			{
				for (int i = 0; i < this._membersCount; i++)
				{
					NetSerializer.FastCall<T> fastCall = this._serializers[i];
					if (fastCall.Type == NetSerializer.CallType.Basic)
					{
						fastCall.Write(obj, writer);
					}
					else if (fastCall.Type == NetSerializer.CallType.Array)
					{
						fastCall.WriteArray(obj, writer);
					}
					else
					{
						fastCall.WriteList(obj, writer);
					}
				}
			}

			// Token: 0x0600051B RID: 1307 RVA: 0x00013B04 File Offset: 0x00011D04
			public void Read(T obj, NetDataReader reader)
			{
				for (int i = 0; i < this._membersCount; i++)
				{
					NetSerializer.FastCall<T> fastCall = this._serializers[i];
					if (fastCall.Type == NetSerializer.CallType.Basic)
					{
						fastCall.Read(obj, reader);
					}
					else if (fastCall.Type == NetSerializer.CallType.Array)
					{
						fastCall.ReadArray(obj, reader);
					}
					else
					{
						fastCall.ReadList(obj, reader);
					}
				}
			}

			// Token: 0x04000312 RID: 786
			public static NetSerializer.ClassInfo<T> Instance;

			// Token: 0x04000313 RID: 787
			private readonly NetSerializer.FastCall<T>[] _serializers;

			// Token: 0x04000314 RID: 788
			private readonly int _membersCount;
		}

		// Token: 0x020000B3 RID: 179
		private abstract class CustomType
		{
			// Token: 0x0600051C RID: 1308
			public abstract NetSerializer.FastCall<T> Get<T>();
		}

		// Token: 0x020000B4 RID: 180
		private sealed class CustomTypeStruct<TProperty> : NetSerializer.CustomType where TProperty : struct, INetSerializable
		{
			// Token: 0x0600051E RID: 1310 RVA: 0x00013B60 File Offset: 0x00011D60
			public override NetSerializer.FastCall<T> Get<T>()
			{
				return new NetSerializer.FastCallStruct<T, TProperty>();
			}
		}

		// Token: 0x020000B5 RID: 181
		private sealed class CustomTypeClass<TProperty> : NetSerializer.CustomType where TProperty : class, INetSerializable
		{
			// Token: 0x06000520 RID: 1312 RVA: 0x00013B6F File Offset: 0x00011D6F
			public CustomTypeClass(Func<TProperty> constructor)
			{
				this._constructor = constructor;
			}

			// Token: 0x06000521 RID: 1313 RVA: 0x00013B7E File Offset: 0x00011D7E
			public override NetSerializer.FastCall<T> Get<T>()
			{
				return new NetSerializer.FastCallClass<T, TProperty>(this._constructor);
			}

			// Token: 0x04000315 RID: 789
			private readonly Func<TProperty> _constructor;
		}

		// Token: 0x020000B6 RID: 182
		private sealed class CustomTypeStatic<TProperty> : NetSerializer.CustomType
		{
			// Token: 0x06000522 RID: 1314 RVA: 0x00013B8B File Offset: 0x00011D8B
			public CustomTypeStatic(Action<NetDataWriter, TProperty> writer, Func<NetDataReader, TProperty> reader)
			{
				this._writer = writer;
				this._reader = reader;
			}

			// Token: 0x06000523 RID: 1315 RVA: 0x00013BA1 File Offset: 0x00011DA1
			public override NetSerializer.FastCall<T> Get<T>()
			{
				return new NetSerializer.FastCallStatic<T, TProperty>(this._writer, this._reader);
			}

			// Token: 0x04000316 RID: 790
			private readonly Action<NetDataWriter, TProperty> _writer;

			// Token: 0x04000317 RID: 791
			private readonly Func<NetDataReader, TProperty> _reader;
		}
	}
}
