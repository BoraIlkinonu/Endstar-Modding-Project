using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.Serialization;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000B8 RID: 184
	public sealed class UIDynamicTypeFactory : UIMonoBehaviourSingleton<UIDynamicTypeFactory>, IUIDynamicTypeFactory
	{
		// Token: 0x060002E2 RID: 738 RVA: 0x00012D3E File Offset: 0x00010F3E
		[ContextMenu("OnValidate")]
		private void OnValidate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnValidate", Array.Empty<object>());
			}
			this.ValidateLuaInspectorTypes();
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x00012D5E File Offset: 0x00010F5E
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIIEnumerablePresenter.SetDynamicTypeFactory(this);
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x00012D80 File Offset: 0x00010F80
		public object Create(Type type)
		{
			if (type == null)
			{
				DebugUtility.LogError("Create method received a null type.", this);
				return null;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Create", new object[] { type.Name });
			}
			return this.CreateInstance(type);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x00012DCC File Offset: 0x00010FCC
		public T Create<T>()
		{
			return (T)((object)this.CreateInstance(typeof(T)));
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x00012DE4 File Offset: 0x00010FE4
		public T Create<T>(Type type)
		{
			if (type == null)
			{
				DebugUtility.LogError("Create method received a null type.", this);
				return default(T);
			}
			if (!typeof(T).IsAssignableFrom(type))
			{
				DebugUtility.LogError(string.Concat(new string[]
				{
					"Type ",
					type.Name,
					" is not assignable to ",
					typeof(T).Name,
					"."
				}), this);
				return default(T);
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Create", new object[] { type.Name });
			}
			return (T)((object)this.CreateInstance(type));
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00012EA0 File Offset: 0x000110A0
		private object CreateInstance(Type type)
		{
			Func<object> func;
			if (UIDynamicTypeFactory.parameterlessConstructorDictionary.TryGetValue(type, out func))
			{
				return func();
			}
			if (type.IsArray)
			{
				return this.CreateEmptyArray(type);
			}
			if (this.HasParameterlessConstructor(type))
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch (Exception ex)
				{
					DebugUtility.LogException(ex, this);
					return null;
				}
			}
			DebugUtility.LogError("Cannot create instance of type " + type.Name + ". No parameterless constructor or factory method available.", this);
			return null;
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00012F20 File Offset: 0x00011120
		private object CreateEmptyArray(Type arrayType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateEmptyArray", new object[] { arrayType.Name });
			}
			Type elementType = arrayType.GetElementType();
			if (elementType == null)
			{
				DebugUtility.LogError("Failed to determine element type for array type " + arrayType.Name + ".", this);
				return null;
			}
			return Array.CreateInstance(elementType, 0);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00012F84 File Offset: 0x00011184
		private void ValidateLuaInspectorTypes()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ValidateLuaInspectorTypes", Array.Empty<object>());
			}
			foreach (Type type in EndlessTypeMapping.Instance.LuaInspectorTypes)
			{
				if (!this.HasParameterlessConstructor(type) && !UIDynamicTypeFactory.parameterlessConstructorDictionary.ContainsKey(type))
				{
					DebugUtility.LogError("Support for " + type.Name + " is missing in parameterlessConstructorDictionary and it lacks a parameterless constructor.", this);
				}
			}
		}

		// Token: 0x060002EA RID: 746 RVA: 0x00012FF8 File Offset: 0x000111F8
		private bool HasParameterlessConstructor(Type type)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HasParameterlessConstructor", new object[] { type.Name });
			}
			bool flag;
			if (this.constructorCache.TryGetValue(type, out flag))
			{
				return flag;
			}
			flag = type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
			this.constructorCache[type] = flag;
			return flag;
		}

		// Token: 0x0400030B RID: 779
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
				() => ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, true)
			},
			{
				typeof(CellReference),
				() => ReferenceFactory.CreateCellReference(null, null)
			},
			{
				typeof(PlayerReference),
				() => ReferenceFactory.CreatePlayerReference(true, 0)
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
				() => ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, true)
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

		// Token: 0x0400030C RID: 780
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400030D RID: 781
		private readonly Dictionary<Type, bool> constructorCache = new Dictionary<Type, bool>();
	}
}
