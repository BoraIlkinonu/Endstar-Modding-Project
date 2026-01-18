using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI;

public class UIBlankNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<BlankNpcCustomizationData>
{
	public override NpcClass NpcClass => NpcClass.Blank;
}
