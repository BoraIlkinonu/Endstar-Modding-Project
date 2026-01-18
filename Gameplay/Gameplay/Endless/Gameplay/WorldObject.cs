using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002BF RID: 703
	public class WorldObject : EndlessBehaviour, IAwakeSubscriber
	{
		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000FFF RID: 4095 RVA: 0x00051D5F File Offset: 0x0004FF5F
		// (set) Token: 0x06001000 RID: 4096 RVA: 0x00051D67 File Offset: 0x0004FF67
		public EndlessProp EndlessProp { get; private set; }

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06001001 RID: 4097 RVA: 0x00051D70 File Offset: 0x0004FF70
		// (set) Token: 0x06001002 RID: 4098 RVA: 0x00051D78 File Offset: 0x0004FF78
		public NetworkObject NetworkObject { get; private set; }

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06001003 RID: 4099 RVA: 0x00051D81 File Offset: 0x0004FF81
		// (set) Token: 0x06001004 RID: 4100 RVA: 0x00051D89 File Offset: 0x0004FF89
		public EndlessVisuals EndlessVisuals { get; private set; }

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06001005 RID: 4101 RVA: 0x00051D94 File Offset: 0x0004FF94
		private Dictionary<Type, object> ComponentMap
		{
			get
			{
				if (this.componentMap == null)
				{
					this.componentMap = new Dictionary<Type, object>();
					foreach (MonoBehaviour monoBehaviour in this.components)
					{
						if (!this.componentMap.TryAdd(monoBehaviour.GetType(), monoBehaviour))
						{
							Debug.LogException(new InvalidOperationException(string.Format("Attempted to track {0}, but it was already tracked", monoBehaviour.GetType())), base.gameObject);
						}
					}
				}
				return this.componentMap;
			}
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06001006 RID: 4102 RVA: 0x00051E30 File Offset: 0x00050030
		public Context Context
		{
			get
			{
				return this.BaseType.Context;
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06001007 RID: 4103 RVA: 0x00051E40 File Offset: 0x00050040
		public IBaseType BaseType
		{
			get
			{
				IBaseType baseType;
				if ((baseType = this.baseType) == null)
				{
					baseType = (this.baseType = this.baseTypeComponent as IBaseType);
				}
				return baseType;
			}
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06001008 RID: 4104 RVA: 0x00051E6B File Offset: 0x0005006B
		public Component BaseTypeComponent
		{
			get
			{
				return this.baseTypeComponent;
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06001009 RID: 4105 RVA: 0x00051E73 File Offset: 0x00050073
		public SerializableGuid InstanceId
		{
			get
			{
				if (this.instanceId.IsEmpty)
				{
					this.instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.gameObject);
				}
				return this.instanceId;
			}
		}

		// Token: 0x0600100A RID: 4106 RVA: 0x00051EA3 File Offset: 0x000500A3
		public bool TryGetNetworkObjectId(out uint networkObjectId)
		{
			networkObjectId = (this.NetworkObject ? ((uint)this.NetworkObject.NetworkObjectId) : 0U);
			return this.NetworkObject;
		}

		// Token: 0x0600100B RID: 4107 RVA: 0x00051ECE File Offset: 0x000500CE
		public void Initialize(Component baseTypeComponent, IEnumerable<IComponentBase> componentBases, NetworkObject netObject = null)
		{
			if (componentBases != null)
			{
				this.components.AddRange(componentBases.Cast<MonoBehaviour>());
			}
			this.baseTypeComponent = baseTypeComponent;
			this.components.Add((MonoBehaviour)baseTypeComponent);
			this.NetworkObject = netObject;
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x00051F04 File Offset: 0x00050104
		public bool TryGetUserComponent(Type type, out object component)
		{
			component = null;
			object obj;
			if (!this.ComponentMap.TryGetValue(type, out obj))
			{
				return false;
			}
			component = obj;
			return true;
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x00051F2C File Offset: 0x0005012C
		public bool TryGetUserComponent<T>(out T component) where T : IComponentBase
		{
			component = default(T);
			object obj;
			if (!this.ComponentMap.TryGetValue(typeof(T), out obj))
			{
				return false;
			}
			component = (T)((object)obj);
			return true;
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x00051F68 File Offset: 0x00050168
		public T GetUserComponent<T>() where T : IComponentBase
		{
			return (T)((object)this.ComponentMap[typeof(T)]);
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x00051F84 File Offset: 0x00050184
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!ExitManager.IsQuitting && this.hasAwoken && this.NetworkObject)
			{
				MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.Remove((uint)this.NetworkObject.NetworkObjectId);
			}
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x00051FC4 File Offset: 0x000501C4
		public void EndlessAwake()
		{
			if (this.hasAwoken)
			{
				return;
			}
			this.hasAwoken = true;
			if (this.NetworkObject)
			{
				MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.TryAdd((uint)this.NetworkObject.NetworkObjectId, this);
			}
		}

		// Token: 0x04000DC6 RID: 3526
		[SerializeField]
		private List<MonoBehaviour> components = new List<MonoBehaviour>();

		// Token: 0x04000DC7 RID: 3527
		[SerializeField]
		[HideInInspector]
		private Component baseTypeComponent;

		// Token: 0x04000DCA RID: 3530
		private Dictionary<Type, object> componentMap;

		// Token: 0x04000DCB RID: 3531
		private IBaseType baseType;

		// Token: 0x04000DCC RID: 3532
		private SerializableGuid instanceId = SerializableGuid.Empty;

		// Token: 0x04000DCD RID: 3533
		private bool hasAwoken;
	}
}
