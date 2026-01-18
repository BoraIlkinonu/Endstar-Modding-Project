using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002A7 RID: 679
	public class IconList : ScriptableObject
	{
		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000EEE RID: 3822 RVA: 0x0004EBF0 File Offset: 0x0004CDF0
		private Dictionary<SerializableGuid, IconDefinition> IconMap
		{
			get
			{
				if (this.iconMap == null)
				{
					this.iconMap = new Dictionary<SerializableGuid, IconDefinition>();
					foreach (IconDefinition iconDefinition in this.definitions)
					{
						this.iconMap.Add(iconDefinition.IconId, iconDefinition);
					}
				}
				return this.iconMap;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000EEF RID: 3823 RVA: 0x0004EC68 File Offset: 0x0004CE68
		public IReadOnlyList<IconDefinition> Definitions
		{
			get
			{
				return this.definitions;
			}
		}

		// Token: 0x170002DA RID: 730
		public Texture2D this[SerializableGuid guid]
		{
			get
			{
				IconDefinition iconDefinition;
				if (this.IconMap.TryGetValue(guid, out iconDefinition))
				{
					return iconDefinition.IconTexture;
				}
				return this.defaultIcon;
			}
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x0004EC9C File Offset: 0x0004CE9C
		public static bool IconDefinitionIsMissingData(IconDefinition characterCosmeticsDefinition)
		{
			return characterCosmeticsDefinition.IconId.IsEmpty || characterCosmeticsDefinition.IconTexture == null;
		}

		// Token: 0x04000D4D RID: 3405
		[SerializeField]
		private List<IconDefinition> definitions = new List<IconDefinition>();

		// Token: 0x04000D4E RID: 3406
		[SerializeField]
		private Texture2D defaultIcon;

		// Token: 0x04000D4F RID: 3407
		private Dictionary<SerializableGuid, IconDefinition> iconMap;
	}
}
