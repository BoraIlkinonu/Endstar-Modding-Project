using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200034B RID: 843
	public class EmptyTool : EndlessTool
	{
		// Token: 0x17000264 RID: 612
		// (get) Token: 0x06000FC2 RID: 4034 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override ToolType ToolType
		{
			get
			{
				return ToolType.Empty;
			}
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x000056F3 File Offset: 0x000038F3
		public override void HandleDeselected()
		{
		}

		// Token: 0x06000FC4 RID: 4036 RVA: 0x00049D96 File Offset: 0x00047F96
		public override void HandleSelected()
		{
			if (base.UIToolPrompter)
			{
				base.UIToolPrompter.Hide();
			}
		}

		// Token: 0x06000FC5 RID: 4037 RVA: 0x00049DB0 File Offset: 0x00047FB0
		public override void ToolPressed()
		{
			base.ToolPressed();
			if (this.secretModeEnabled)
			{
				LineCastHit activeLineCastResult = base.ActiveLineCastResult;
				if (activeLineCastResult.IntersectionOccured)
				{
					Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition);
					TerrainCell terrainCell = cellFromCoordinate as TerrainCell;
					if (terrainCell != null)
					{
						PaintingTool paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Painting) as PaintingTool;
						if (paintingTool)
						{
							MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(paintingTool);
							paintingTool.SetActiveTilesetIndex(terrainCell.TilesetIndex);
							return;
						}
					}
					else
					{
						PropCell propCell = cellFromCoordinate as PropCell;
						if (propCell != null)
						{
							InspectorTool inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.GetTool(ToolType.Inspector) as InspectorTool;
							if (inspectorTool)
							{
								MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(inspectorTool);
								inspectorTool.SetInspectedId(propCell.InstanceId);
								inspectorTool.SetStateToInspect();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000FC6 RID: 4038 RVA: 0x00049E7B File Offset: 0x0004807B
		public override void UpdateTool()
		{
			base.UpdateTool();
			if (EndlessInput.GetKeyDown(Key.B))
			{
				this.secretModeEnabled = !this.secretModeEnabled;
			}
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x00049EA4 File Offset: 0x000480A4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x00049EBA File Offset: 0x000480BA
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00049EC4 File Offset: 0x000480C4
		protected internal override string __getTypeName()
		{
			return "EmptyTool";
		}

		// Token: 0x04000D0E RID: 3342
		private bool secretModeEnabled;
	}
}
