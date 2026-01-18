using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200003B RID: 59
	public class PropRequirement : ScriptableObject
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000106 RID: 262 RVA: 0x000058F4 File Offset: 0x00003AF4
		public List<SerializableGuid> Guids
		{
			get
			{
				if (this.guids == null)
				{
					this.guids = this.propEntries.Select((PropRequirement.PropRequirementEntry entry) => new SerializableGuid(entry.Id)).ToList<SerializableGuid>();
				}
				return this.guids;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000107 RID: 263 RVA: 0x00005944 File Offset: 0x00003B44
		public int MinCount
		{
			get
			{
				return this.minCount;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000108 RID: 264 RVA: 0x0000594C File Offset: 0x00003B4C
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

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000109 RID: 265 RVA: 0x00005962 File Offset: 0x00003B62
		public bool UseMaxCount
		{
			get
			{
				return this.useMaxCount;
			}
		}

		// Token: 0x040000B2 RID: 178
		[SerializeField]
		private int minCount;

		// Token: 0x040000B3 RID: 179
		[SerializeField]
		private bool useMaxCount;

		// Token: 0x040000B4 RID: 180
		[SerializeField]
		private int maxCount;

		// Token: 0x040000B5 RID: 181
		[SerializeField]
		private List<PropRequirement.PropRequirementEntry> propEntries = new List<PropRequirement.PropRequirementEntry>();

		// Token: 0x040000B6 RID: 182
		[NonSerialized]
		private List<SerializableGuid> guids;

		// Token: 0x0200003C RID: 60
		[Serializable]
		public class PropRequirementEntry
		{
			// Token: 0x040000B7 RID: 183
			public string Name;

			// Token: 0x040000B8 RID: 184
			public string Id;
		}
	}
}
