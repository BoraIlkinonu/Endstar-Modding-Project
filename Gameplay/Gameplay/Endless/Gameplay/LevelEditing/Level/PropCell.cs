using System;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200055E RID: 1374
	public class PropCell : Cell
	{
		// Token: 0x17000652 RID: 1618
		// (get) Token: 0x06002116 RID: 8470 RVA: 0x00094F6C File Offset: 0x0009316C
		// (set) Token: 0x06002117 RID: 8471 RVA: 0x00094F74 File Offset: 0x00093174
		public SerializableGuid InstanceId { get; set; }

		// Token: 0x17000653 RID: 1619
		// (get) Token: 0x06002118 RID: 8472 RVA: 0x0001BD04 File Offset: 0x00019F04
		public override CellType Type
		{
			get
			{
				return CellType.Prop;
			}
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x00094F7D File Offset: 0x0009317D
		public PropCell(PropLocationOffset propLocationOffset, Transform cellBase)
			: base(propLocationOffset.Offset, cellBase)
		{
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x00017586 File Offset: 0x00015786
		public override bool BlocksDecorations()
		{
			return true;
		}
	}
}
