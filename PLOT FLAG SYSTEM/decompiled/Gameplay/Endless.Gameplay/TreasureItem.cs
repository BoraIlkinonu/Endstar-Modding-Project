using UnityEngine;

namespace Endless.Gameplay;

public class TreasureItem : Item
{
	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "TreasureItem";
	}
}
