using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public sealed class UIDynamicTypeFactory : UIMonoBehaviourSingleton<UIDynamicTypeFactory>, IUIDynamicTypeFactory
{
	private static readonly Dictionary<Type, Func<object>> parameterlessConstructorDictionary = new Dictionary<Type, Func<object>>
	{
		{
			typeof(string),
			() => string.Empty
		},
		{
			typeof(LocalizedString),
			() => new LocalizedString()
		},
		{
			typeof(CharacterVisualsReference),
			() => ReferenceFactory.CreateCharacterVisualsReference(SerializableGuid.Empty)
		},
		{
			typeof(NpcInstanceReference),
			() => ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, useContext: true)
		},
		{
			typeof(CellReference),
			() => ReferenceFactory.CreateCellReference()
		},
		{
			typeof(PlayerReference),
			() => ReferenceFactory.CreatePlayerReference()
		},
		{
			typeof(InventoryLibraryReference),
			() => ReferenceFactory.CreateInventoryLibraryReference(SerializableGuid.Empty)
		},
		{
			typeof(KeyLibraryReference),
			() => ReferenceFactory.CreateKeyLibraryReference(SerializableGuid.Empty)
		},
		{
			typeof(PhysicsObjectLibraryReference),
			() => new PhysicsObjectLibraryReference()
		},
		{
			typeof(AudioReference),
			() => ReferenceFactory.CreateAudioReference(SerializableGuid.Empty)
		},
		{
			typeof(AssetLibraryReferenceClass),
			() => ReferenceFactory.CreateAssetLibraryReferenceClass(SerializableGuid.Empty)
		},
		{
			typeof(PropLibraryReference),
			() => new PropLibraryReference()
		},
		{
			typeof(InstanceReference),
			() => ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, useContext: true)
		},
		{
			typeof(NpcClassCustomizationData),
			() => new GruntNpcCustomizationData()
		},
		{
			typeof(Color),
			() => Color.white
		}
	};

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly Dictionary<Type, bool> constructorCache = new Dictionary<Type, bool>();

	[ContextMenu("OnValidate")]
	private void OnValidate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnValidate");
		}
		ValidateLuaInspectorTypes();
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		UIIEnumerablePresenter.SetDynamicTypeFactory(this);
	}

	public object Create(Type type)
	{
		if (type == null)
		{
			DebugUtility.LogError("Create method received a null type.", this);
			return null;
		}
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Create", type.Name);
		}
		return CreateInstance(type);
	}

	public T Create<T>()
	{
		return (T)CreateInstance(typeof(T));
	}

	public T Create<T>(Type type)
	{
		if (type == null)
		{
			DebugUtility.LogError("Create method received a null type.", this);
			return default(T);
		}
		if (!typeof(T).IsAssignableFrom(type))
		{
			DebugUtility.LogError("Type " + type.Name + " is not assignable to " + typeof(T).Name + ".", this);
			return default(T);
		}
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Create", type.Name);
		}
		return (T)CreateInstance(type);
	}

	private object CreateInstance(Type type)
	{
		if (parameterlessConstructorDictionary.TryGetValue(type, out var value))
		{
			return value();
		}
		if (type.IsArray)
		{
			return CreateEmptyArray(type);
		}
		if (HasParameterlessConstructor(type))
		{
			try
			{
				return Activator.CreateInstance(type);
			}
			catch (Exception exception)
			{
				DebugUtility.LogException(exception, this);
				return null;
			}
		}
		DebugUtility.LogError("Cannot create instance of type " + type.Name + ". No parameterless constructor or factory method available.", this);
		return null;
	}

	private object CreateEmptyArray(Type arrayType)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateEmptyArray", arrayType.Name);
		}
		Type elementType = arrayType.GetElementType();
		if (elementType == null)
		{
			DebugUtility.LogError("Failed to determine element type for array type " + arrayType.Name + ".", this);
			return null;
		}
		return Array.CreateInstance(elementType, 0);
	}

	private void ValidateLuaInspectorTypes()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ValidateLuaInspectorTypes");
		}
		Type[] luaInspectorTypes = EndlessTypeMapping.Instance.LuaInspectorTypes;
		foreach (Type type in luaInspectorTypes)
		{
			if (!HasParameterlessConstructor(type) && !parameterlessConstructorDictionary.ContainsKey(type))
			{
				DebugUtility.LogError("Support for " + type.Name + " is missing in parameterlessConstructorDictionary and it lacks a parameterless constructor.", this);
			}
		}
	}

	private bool HasParameterlessConstructor(Type type)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HasParameterlessConstructor", type.Name);
		}
		if (constructorCache.TryGetValue(type, out var value))
		{
			return value;
		}
		value = type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
		constructorCache[type] = value;
		return value;
	}
}
