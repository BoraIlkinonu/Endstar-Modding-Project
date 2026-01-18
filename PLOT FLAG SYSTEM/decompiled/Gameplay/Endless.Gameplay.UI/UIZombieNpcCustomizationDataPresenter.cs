using Endless.Gameplay.LuaEnums;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public class UIZombieNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<ZombieNpcCustomizationData>
{
	public override NpcClass NpcClass => NpcClass.Zombie;

	protected override void Start()
	{
		base.Start();
		(base.Viewable as UIZombieNpcCustomizationDataView).OnZombifyTargetChanged += SetZombifyTarget;
	}

	private void SetZombifyTarget(bool zombifyTarget)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetZombifyTarget", zombifyTarget);
		}
		base.Model.ZombifyTarget = zombifyTarget;
		SetModel(base.Model, triggerOnModelChanged: true);
	}
}
