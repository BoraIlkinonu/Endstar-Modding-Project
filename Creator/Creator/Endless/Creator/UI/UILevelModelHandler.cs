using System;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI
{
	// Token: 0x020000B0 RID: 176
	public class UILevelModelHandler : UIAssetModelHandler<LevelState>
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060002C3 RID: 707 RVA: 0x0001273C File Offset: 0x0001093C
		protected override ErrorCodes OnGetAssetFailErrorCode
		{
			get
			{
				return ErrorCodes.UILevelModelHandler_GetLevel;
			}
		}
	}
}
