using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ELM.Endless.EndlessFramework.Serialization
{
	// Token: 0x02000030 RID: 48
	internal class EndlessSerializationSurrogate : ISerializationSurrogate
	{
		// Token: 0x060000BE RID: 190 RVA: 0x00004484 File Offset: 0x00002684
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Type type = obj.GetType();
			EndlessSerializationSurrogate.TypeSerializer typeSerializer;
			if (EndlessSerializationSurrogate._typeSerializers.TryGetValue(type, out typeSerializer))
			{
				typeSerializer.Serialize(obj, info);
			}
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000044B4 File Offset: 0x000026B4
		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Type type = obj.GetType();
			EndlessSerializationSurrogate.TypeSerializer typeSerializer;
			if (EndlessSerializationSurrogate._typeSerializers.TryGetValue(type, out typeSerializer))
			{
				return typeSerializer.Deserialize(info);
			}
			return null;
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000044E5 File Offset: 0x000026E5
		internal static bool DefinedTypesContains(Type type)
		{
			return EndlessSerializationSurrogate._typeSerializers.ContainsKey(type);
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000044F4 File Offset: 0x000026F4
		private static Dictionary<Type, EndlessSerializationSurrogate.TypeSerializer> CreateTypeSerializers()
		{
			Dictionary<Type, EndlessSerializationSurrogate.TypeSerializer> dictionary = new Dictionary<Type, EndlessSerializationSurrogate.TypeSerializer>();
			dictionary.Add(typeof(Bounds), EndlessSerializationSurrogate.CreateTypeSerializer<Bounds>(delegate(Bounds value, SerializationInfo info)
			{
				info.AddValue("center", value.center);
				info.AddValue("extents", value.extents);
			}, (SerializationInfo info) => new Bounds
			{
				center = info.GetValue("center"),
				extents = info.GetValue("extents")
			}));
			dictionary.Add(typeof(Color), EndlessSerializationSurrogate.CreateTypeSerializer<Color>(delegate(Color value, SerializationInfo info)
			{
				info.AddValue("r", value.r);
				info.AddValue("g", value.g);
				info.AddValue("b", value.b);
				info.AddValue("a", value.a);
			}, (SerializationInfo info) => new Color(info.GetSingle("r"), info.GetSingle("g"), info.GetSingle("b"), info.GetSingle("a"))));
			dictionary.Add(typeof(Color32), EndlessSerializationSurrogate.CreateTypeSerializer<Color32>(delegate(Color32 value, SerializationInfo info)
			{
				info.AddValue("r", value.r);
				info.AddValue("g", value.g);
				info.AddValue("b", value.b);
				info.AddValue("a", value.a);
			}, (SerializationInfo info) => new Color32(info.GetByte("r"), info.GetByte("g"), info.GetByte("b"), info.GetByte("a"))));
			dictionary.Add(typeof(Keyframe), EndlessSerializationSurrogate.CreateTypeSerializer<Keyframe>(delegate(Keyframe value, SerializationInfo info)
			{
				info.AddValue("time", value.time);
				info.AddValue("value", value.value);
				info.AddValue("inTangent", value.inTangent);
				info.AddValue("outTangent", value.outTangent);
				info.AddValue("inWeight", value.inWeight);
				info.AddValue("outWeight", value.outWeight);
			}, (SerializationInfo info) => new Keyframe(info.GetSingle("time"), info.GetSingle("value"), info.GetSingle("inTangent"), info.GetSingle("outTangent"), info.GetSingle("inWeight"), info.GetSingle("outWeight"))));
			dictionary.Add(typeof(LayerMask), EndlessSerializationSurrogate.CreateTypeSerializer<LayerMask>(delegate(LayerMask value, SerializationInfo info)
			{
				info.AddValue("mask", value.value);
			}, (SerializationInfo info) => new LayerMask
			{
				value = info.GetInt32("mask")
			}));
			dictionary.Add(typeof(Matrix4x4), EndlessSerializationSurrogate.CreateTypeSerializer<Matrix4x4>(delegate(Matrix4x4 value, SerializationInfo info)
			{
				bool isIdentity = value.isIdentity;
				bool flag = !isIdentity && value == Matrix4x4.zero;
				info.AddValue("isIdentity", isIdentity);
				info.AddValue("isZero", flag);
				if (!isIdentity && !flag)
				{
					info.AddValue("00", value.m00);
					info.AddValue("10", value.m10);
					info.AddValue("20", value.m20);
					info.AddValue("30", value.m30);
					info.AddValue("01", value.m01);
					info.AddValue("11", value.m11);
					info.AddValue("21", value.m21);
					info.AddValue("31", value.m31);
					info.AddValue("02", value.m02);
					info.AddValue("12", value.m12);
					info.AddValue("22", value.m22);
					info.AddValue("32", value.m32);
					info.AddValue("03", value.m03);
					info.AddValue("13", value.m13);
					info.AddValue("23", value.m23);
					info.AddValue("33", value.m33);
				}
			}, delegate(SerializationInfo info)
			{
				bool boolean = info.GetBoolean("isIdentity");
				bool boolean2 = info.GetBoolean("isZero");
				if (boolean)
				{
					return Matrix4x4.identity;
				}
				if (boolean2)
				{
					return Matrix4x4.zero;
				}
				return new Matrix4x4
				{
					m00 = info.GetSingle("00"),
					m10 = info.GetSingle("10"),
					m20 = info.GetSingle("20"),
					m30 = info.GetSingle("30"),
					m01 = info.GetSingle("01"),
					m11 = info.GetSingle("11"),
					m21 = info.GetSingle("21"),
					m31 = info.GetSingle("31"),
					m02 = info.GetSingle("02"),
					m12 = info.GetSingle("12"),
					m22 = info.GetSingle("22"),
					m32 = info.GetSingle("32"),
					m03 = info.GetSingle("03"),
					m13 = info.GetSingle("13"),
					m23 = info.GetSingle("23"),
					m33 = info.GetSingle("33")
				};
			}));
			dictionary.Add(typeof(Plane), EndlessSerializationSurrogate.CreateTypeSerializer<Plane>(delegate(Plane value, SerializationInfo info)
			{
				info.AddValue("normal", value.normal);
				info.AddValue("distance", value.distance);
			}, (SerializationInfo info) => new Plane
			{
				normal = info.GetValue("normal"),
				distance = info.GetSingle("distance")
			}));
			dictionary.Add(typeof(Quaternion), EndlessSerializationSurrogate.CreateTypeSerializer<Quaternion>(delegate(Quaternion value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
				info.AddValue("z", value.z);
				info.AddValue("w", value.w);
			}, (SerializationInfo info) => new Quaternion
			{
				x = info.GetSingle("x"),
				y = info.GetSingle("y"),
				z = info.GetSingle("z"),
				w = info.GetSingle("w")
			}));
			dictionary.Add(typeof(Ray), EndlessSerializationSurrogate.CreateTypeSerializer<Ray>(delegate(Ray value, SerializationInfo info)
			{
				info.AddValue("origin", value.origin);
				info.AddValue("direction", value.direction);
			}, (SerializationInfo info) => new Ray
			{
				origin = info.GetValue("origin"),
				direction = info.GetValue("direction")
			}));
			dictionary.Add(typeof(Ray2D), EndlessSerializationSurrogate.CreateTypeSerializer<Ray2D>(delegate(Ray2D value, SerializationInfo info)
			{
				info.AddValue("origin", value.origin);
				info.AddValue("direction", value.direction);
			}, (SerializationInfo info) => new Ray2D
			{
				origin = info.GetValue("origin"),
				direction = info.GetValue("direction")
			}));
			dictionary.Add(typeof(Rect), EndlessSerializationSurrogate.CreateTypeSerializer<Rect>(delegate(Rect value, SerializationInfo info)
			{
				info.AddValue("position", value.position);
				info.AddValue("size", value.size);
			}, (SerializationInfo info) => new Rect(info.GetValue("position"), info.GetValue("size"))));
			dictionary.Add(typeof(Vector2), EndlessSerializationSurrogate.CreateTypeSerializer<Vector2>(delegate(Vector2 value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
			}, (SerializationInfo info) => new Vector2(info.GetSingle("x"), info.GetSingle("y"))));
			dictionary.Add(typeof(Vector3), EndlessSerializationSurrogate.CreateTypeSerializer<Vector3>(delegate(Vector3 value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
				info.AddValue("z", value.z);
			}, (SerializationInfo info) => new Vector3(info.GetSingle("x"), info.GetSingle("y"), info.GetSingle("z"))));
			dictionary.Add(typeof(Vector4), EndlessSerializationSurrogate.CreateTypeSerializer<Vector4>(delegate(Vector4 value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
				info.AddValue("z", value.z);
				info.AddValue("w", value.w);
			}, (SerializationInfo info) => new Vector4(info.GetSingle("x"), info.GetSingle("y"), info.GetSingle("z"), info.GetSingle("w"))));
			dictionary.Add(typeof(AnimationCurve), EndlessSerializationSurrogate.CreateTypeSerializer<AnimationCurve>(delegate(AnimationCurve value, SerializationInfo info)
			{
				info.AddValue("keyframeCount", value.keys.Length);
				for (int i = 0; i < value.keys.Length; i++)
				{
					Keyframe keyframe = value.keys[i];
					info.AddValue(i.ToString() + "time", keyframe.time);
					info.AddValue(i.ToString() + "value", keyframe.value);
					info.AddValue(i.ToString() + "inTangent", keyframe.inTangent);
					info.AddValue(i.ToString() + "outTangent", keyframe.outTangent);
					info.AddValue(i.ToString() + "inWeight", keyframe.inWeight);
					info.AddValue(i.ToString() + "outWeight", keyframe.outWeight);
				}
			}, delegate(SerializationInfo info)
			{
				int @int = info.GetInt32("keyframeCount");
				Keyframe[] array = new Keyframe[@int];
				for (int j = 0; j < @int; j++)
				{
					array[j].time = info.GetSingle(j.ToString() + "time");
					array[j].value = info.GetSingle(j.ToString() + "value");
					array[j].inTangent = info.GetSingle(j.ToString() + "inTangent");
					array[j].outTangent = info.GetSingle(j.ToString() + "outTangent");
					array[j].inWeight = info.GetSingle(j.ToString() + "inWeight");
					array[j].outWeight = info.GetSingle(j.ToString() + "outWeight");
				}
				return new AnimationCurve(array);
			}));
			dictionary.Add(typeof(Vector2Int), EndlessSerializationSurrogate.CreateTypeSerializer<Vector2Int>(delegate(Vector2Int value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
			}, (SerializationInfo info) => new Vector2Int(info.GetInt32("x"), info.GetInt32("y"))));
			dictionary.Add(typeof(Vector3Int), EndlessSerializationSurrogate.CreateTypeSerializer<Vector3Int>(delegate(Vector3Int value, SerializationInfo info)
			{
				info.AddValue("x", value.x);
				info.AddValue("y", value.y);
				info.AddValue("z", value.z);
			}, (SerializationInfo info) => new Vector3Int(info.GetInt32("x"), info.GetInt32("y"), info.GetInt32("z"))));
			return dictionary;
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004A8C File Offset: 0x00002C8C
		private static EndlessSerializationSurrogate.TypeSerializer CreateTypeSerializer<T>(Action<T, SerializationInfo> serialize, Func<SerializationInfo, T> deserialize)
		{
			return new EndlessSerializationSurrogate.TypeSerializer(delegate(object @object, SerializationInfo info)
			{
				serialize((T)((object)@object), info);
			}, (SerializationInfo info) => deserialize(info));
		}

		// Token: 0x0400007D RID: 125
		private static readonly Dictionary<Type, EndlessSerializationSurrogate.TypeSerializer> _typeSerializers = EndlessSerializationSurrogate.CreateTypeSerializers();

		// Token: 0x02000031 RID: 49
		private struct TypeSerializer
		{
			// Token: 0x060000C5 RID: 197 RVA: 0x00004AD6 File Offset: 0x00002CD6
			public TypeSerializer(Action<object, SerializationInfo> serialize, Func<SerializationInfo, object> deserialize)
			{
				this.Serialize = serialize;
				this.Deserialize = deserialize;
			}

			// Token: 0x0400007E RID: 126
			public readonly Action<object, SerializationInfo> Serialize;

			// Token: 0x0400007F RID: 127
			public readonly Func<SerializationInfo, object> Deserialize;
		}
	}
}
