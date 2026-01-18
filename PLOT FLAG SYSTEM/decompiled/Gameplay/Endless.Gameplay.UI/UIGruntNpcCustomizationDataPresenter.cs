using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.UI;

public class UIGruntNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<GruntNpcCustomizationData>
{
	public override NpcClass NpcClass => NpcClass.Grunt;
}
