using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;

namespace Endless.Creator.UI;

public class UILevelModelHandler : UIAssetModelHandler<LevelState>
{
	protected override ErrorCodes OnGetAssetFailErrorCode => ErrorCodes.UILevelModelHandler_GetLevel;
}
