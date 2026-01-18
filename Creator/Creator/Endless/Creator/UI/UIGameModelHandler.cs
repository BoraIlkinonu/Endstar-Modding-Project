using System;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI
{
	// Token: 0x020000AD RID: 173
	public class UIGameModelHandler : UIAssetModelHandler<Game>
	{
		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x0001240E File Offset: 0x0001060E
		protected override ErrorCodes OnGetAssetFailErrorCode
		{
			get
			{
				return ErrorCodes.GameEditor_GetGameInitialResolveConflict;
			}
		}
	}
}
