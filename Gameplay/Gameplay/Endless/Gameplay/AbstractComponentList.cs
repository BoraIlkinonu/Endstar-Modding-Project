using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000299 RID: 665
	public abstract class AbstractComponentList<T> : ScriptableObject where T : ComponentDefinition
	{
		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000EC2 RID: 3778 RVA: 0x0004E7F8 File Offset: 0x0004C9F8
		private Dictionary<SerializableGuid, T> DefinitionMap
		{
			get
			{
				if (this.definitionMap == null)
				{
					this.definitionMap = new Dictionary<SerializableGuid, T>();
					foreach (T t in this.components)
					{
						this.definitionMap.Add(t.ComponentId, t);
					}
				}
				return this.definitionMap;
			}
		}

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000EC3 RID: 3779 RVA: 0x0004E874 File Offset: 0x0004CA74
		public IReadOnlyList<T> AllDefinitions
		{
			get
			{
				return this.components;
			}
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x0004E87C File Offset: 0x0004CA7C
		public bool TryGetDefinition(SerializableGuid componentId, out T componentDefinition)
		{
			return this.DefinitionMap.TryGetValue(componentId, out componentDefinition);
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x0004E88C File Offset: 0x0004CA8C
		public bool TryGetDefinition(Type type, out T componentDefinition)
		{
			for (int i = 0; i < this.components.Count; i++)
			{
				if (this.components[i].ComponentBase.GetType() == type)
				{
					componentDefinition = this.components[i];
					return true;
				}
			}
			componentDefinition = default(T);
			return false;
		}

		// Token: 0x04000D38 RID: 3384
		[SerializeField]
		private List<T> components = new List<T>();

		// Token: 0x04000D39 RID: 3385
		private Dictionary<SerializableGuid, T> definitionMap;
	}
}
