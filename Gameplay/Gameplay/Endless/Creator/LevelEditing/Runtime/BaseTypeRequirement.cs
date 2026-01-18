using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000039 RID: 57
	[CreateAssetMenu(menuName = "ScriptableObject/BaseTypeRequirement", fileName = "BaseTypeRequirement")]
	public class BaseTypeRequirement : ScriptableObject
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000FE RID: 254 RVA: 0x00005854 File Offset: 0x00003A54
		public List<SerializableGuid> Guids
		{
			get
			{
				if (this.guids == null)
				{
					this.guids = this.componentDefinitions.Select((ComponentDefinition entry) => entry.ComponentId).ToList<SerializableGuid>();
				}
				return this.guids;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000FF RID: 255 RVA: 0x000058A4 File Offset: 0x00003AA4
		public int MinCount
		{
			get
			{
				return this.minCount;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000100 RID: 256 RVA: 0x000058AC File Offset: 0x00003AAC
		public int MaxCount
		{
			get
			{
				if (!this.useMaxCount)
				{
					return int.MaxValue;
				}
				return this.maxCount;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000101 RID: 257 RVA: 0x000058C2 File Offset: 0x00003AC2
		public bool UseMaxCount
		{
			get
			{
				return this.useMaxCount;
			}
		}

		// Token: 0x040000AB RID: 171
		[SerializeField]
		private int minCount;

		// Token: 0x040000AC RID: 172
		[SerializeField]
		private bool useMaxCount;

		// Token: 0x040000AD RID: 173
		[SerializeField]
		private int maxCount;

		// Token: 0x040000AE RID: 174
		[SerializeField]
		private List<ComponentDefinition> componentDefinitions = new List<ComponentDefinition>();

		// Token: 0x040000AF RID: 175
		[NonSerialized]
		private List<SerializableGuid> guids;
	}
}
