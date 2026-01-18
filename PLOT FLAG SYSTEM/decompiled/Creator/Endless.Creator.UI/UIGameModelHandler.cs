using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI;

public class UIGameModelHandler : UIAssetModelHandler<Game>
{
	protected override ErrorCodes OnGetAssetFailErrorCode => ErrorCodes.GameEditor_GetGameInitialResolveConflict;
}
