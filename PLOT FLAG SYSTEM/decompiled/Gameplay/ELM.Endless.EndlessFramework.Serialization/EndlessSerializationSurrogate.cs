using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ELM.Endless.EndlessFramework.Serialization;

internal class EndlessSerializationSurrogate : ISerializationSurrogate
{
	private struct TypeSerializer
	{
		public readonly Action<object, SerializationInfo> Serialize;

		public readonly Func<SerializationInfo, object> Deserialize;

		public TypeSerializer(Action<object, SerializationInfo> serialize, Func<SerializationInfo, object> deserialize)
		{
			Serialize = serialize;
			Deserialize = deserialize;
		}
	}

	private static readonly Dictionary<Type, TypeSerializer> _typeSerializers = CreateTypeSerializers();

	public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
	{
		Type type = obj.GetType();
		if (_typeSerializers.TryGetValue(type, out var value))
		{
			value.Serialize(obj, info);
		}
	}

	public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
	{
		Type type = obj.GetType();
		if (_typeSerializers.TryGetValue(type, out var value))
		{
			return value.Deserialize(info);
		}
		return null;
	}

	internal static bool DefinedTypesContains(Type type)
	{
		return _typeSerializers.ContainsKey(type);
	}

	private static Dictionary<Type, TypeSerializer> CreateTypeSerializers()
	{
		return new Dictionary<Type, TypeSerializer>
		{
			{
				typeof(Bounds),
				CreateTypeSerializer(delegate(Bounds value, SerializationInfo info)
				{
					info.AddValue("center", value.center);
					info.AddValue("extents", value.extents);
				}, (SerializationInfo info) => new Bounds
				{
					center = info.GetValue<Vector3>("center"),
					extents = info.GetValue<Vector3>("extents")
				})
			},
			{
				typeof(Color),
				CreateTypeSerializer(delegate(Color value, SerializationInfo info)
				{
					info.AddValue("r", value.r);
					info.AddValue("g", value.g);
					info.AddValue("b", value.b);
					info.AddValue("a", value.a);
				}, (SerializationInfo info) => new Color(info.GetSingle("r"), info.GetSingle("g"), info.GetSingle("b"), info.GetSingle("a")))
			},
			{
				typeof(Color32),
				CreateTypeSerializer(delegate(Color32 value, SerializationInfo info)
				{
					info.AddValue("r", value.r);
					info.AddValue("g", value.g);
					info.AddValue("b", value.b);
					info.AddValue("a", value.a);
				}, (SerializationInfo info) => new Color32(info.GetByte("r"), info.GetByte("g"), info.GetByte("b"), info.GetByte("a")))
			},
			{
				typeof(Keyframe),
				CreateTypeSerializer(delegate(Keyframe value, SerializationInfo info)
				{
					info.AddValue("time", value.time);
					info.AddValue("value", value.value);
					info.AddValue("inTangent", value.inTangent);
					info.AddValue("outTangent", value.outTangent);
					info.AddValue("inWeight", value.inWeight);
					info.AddValue("outWeight", value.outWeight);
				}, (SerializationInfo info) => new Keyframe(info.GetSingle("time"), info.GetSingle("value"), info.GetSingle("inTangent"), info.GetSingle("outTangent"), info.GetSingle("inWeight"), info.GetSingle("outWeight")))
			},
			{
				typeof(LayerMask),
				CreateTypeSerializer(delegate(LayerMask value, SerializationInfo info)
				{
					info.AddValue("mask", value.value);
				}, (SerializationInfo info) => new LayerMask
				{
					value = info.GetInt32("mask")
				})
			},
			{
				typeof(Matrix4x4),
				CreateTypeSerializer(delegate(Matrix4x4 value, SerializationInfo info)
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
					return boolean2 ? Matrix4x4.zero : new Matrix4x4
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
				})
			},
			{
				typeof(Plane),
				CreateTypeSerializer(delegate(Plane value, SerializationInfo info)
				{
					info.AddValue("normal", value.normal);
					info.AddValue("distance", value.distance);
				}, (SerializationInfo info) => new Plane
				{
					normal = info.GetValue<Vector3>("normal"),
					distance = info.GetSingle("distance")
				})
			},
			{
				typeof(Quaternion),
				CreateTypeSerializer(delegate(Quaternion value, SerializationInfo info)
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
				})
			},
			{
				typeof(Ray),
				CreateTypeSerializer(delegate(Ray value, SerializationInfo info)
				{
					info.AddValue("origin", value.origin);
					info.AddValue("direction", value.direction);
				}, (SerializationInfo info) => new Ray
				{
					origin = info.GetValue<Vector3>("origin"),
					direction = info.GetValue<Vector3>("direction")
				})
			},
			{
				typeof(Ray2D),
				CreateTypeSerializer(delegate(Ray2D value, SerializationInfo info)
				{
					info.AddValue("origin", value.origin);
					info.AddValue("direction", value.direction);
				}, (SerializationInfo info) => new Ray2D
				{
					origin = info.GetValue<Vector2>("origin"),
					direction = info.GetValue<Vector2>("direction")
				})
			},
			{
				typeof(Rect),
				CreateTypeSerializer(delegate(Rect value, SerializationInfo info)
				{
					info.AddValue("position", value.position);
					info.AddValue("size", value.size);
				}, (SerializationInfo info) => new Rect(info.GetValue<Vector3>("position"), info.GetValue<Vector3>("size")))
			},
			{
				typeof(Vector2),
				CreateTypeSerializer(delegate(Vector2 value, SerializationInfo info)
				{
					info.AddValue("x", value.x);
					info.AddValue("y", value.y);
				}, (SerializationInfo info) => new Vector2(info.GetSingle("x"), info.GetSingle("y")))
			},
			{
				typeof(Vector3),
				CreateTypeSerializer(delegate(Vector3 value, SerializationInfo info)
				{
					info.AddValue("x", value.x);
					info.AddValue("y", value.y);
					info.AddValue("z", value.z);
				}, (SerializationInfo info) => new Vector3(info.GetSingle("x"), info.GetSingle("y"), info.GetSingle("z")))
			},
			{
				typeof(Vector4),
				CreateTypeSerializer(delegate(Vector4 value, SerializationInfo info)
				{
					info.AddValue("x", value.x);
					info.AddValue("y", value.y);
					info.AddValue("z", value.z);
					info.AddValue("w", value.w);
				}, (SerializationInfo info) => new Vector4(info.GetSingle("x"), info.GetSingle("y"), info.GetSingle("z"), info.GetSingle("w")))
			},
			{
				typeof(AnimationCurve),
				CreateTypeSerializer(delegate(AnimationCurve value, SerializationInfo info)
				{
					info.AddValue("keyframeCount", value.keys.Length);
					for (int i = 0; i < value.keys.Length; i++)
					{
						Keyframe keyframe = value.keys[i];
						info.AddValue(i + "time", keyframe.time);
						info.AddValue(i + "value", keyframe.value);
						info.AddValue(i + "inTangent", keyframe.inTangent);
						info.AddValue(i + "outTangent", keyframe.outTangent);
						info.AddValue(i + "inWeight", keyframe.inWeight);
						info.AddValue(i + "outWeight", keyframe.outWeight);
					}
				}, delegate(SerializationInfo info)
				{
					int @int = info.GetInt32("keyframeCount");
					Keyframe[] array = new Keyframe[@int];
					for (int i = 0; i < @int; i++)
					{
						array[i].time = info.GetSingle(i + "time");
						array[i].value = info.GetSingle(i + "value");
						array[i].inTangent = info.GetSingle(i + "inTangent");
						array[i].outTangent = info.GetSingle(i + "outTangent");
						array[i].inWeight = info.GetSingle(i + "inWeight");
						array[i].outWeight = info.GetSingle(i + "outWeight");
					}
					return new AnimationCurve(array);
				})
			},
			{
				typeof(Vector2Int),
				CreateTypeSerializer(delegate(Vector2Int value, SerializationInfo info)
				{
					info.AddValue("x", value.x);
					info.AddValue("y", value.y);
				}, (SerializationInfo info) => new Vector2Int(info.GetInt32("x"), info.GetInt32("y")))
			},
			{
				typeof(Vector3Int),
				CreateTypeSerializer(delegate(Vector3Int value, SerializationInfo info)
				{
					info.AddValue("x", value.x);
					info.AddValue("y", value.y);
					info.AddValue("z", value.z);
				}, (SerializationInfo info) => new Vector3Int(info.GetInt32("x"), info.GetInt32("y"), info.GetInt32("z")))
			}
		};
	}

	private static TypeSerializer CreateTypeSerializer<T>(Action<T, SerializationInfo> serialize, Func<SerializationInfo, T> deserialize)
	{
		return new TypeSerializer(delegate(object @object, SerializationInfo info)
		{
			serialize((T)@object, info);
		}, (SerializationInfo info) => deserialize(info));
	}
}
