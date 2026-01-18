using Endless.Props.ReferenceComponents;
using UnityEngine;

namespace Endless.Gameplay;

public class Key : Item
{
	[SerializeField]
	private VisualsInfo groundVisualsInfo;

	[SerializeField]
	private VisualsInfo equippedVisualsInfo;

	public override ReferenceFilter Filter => ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem | ReferenceFilter.Key;

	protected override VisualsInfo GroundVisualsInfo => groundVisualsInfo;

	protected override VisualsInfo EquippedVisualsInfo => equippedVisualsInfo;

	public GameObject GetLockVisuals()
	{
		return GetComponent<KeyReferences>().LockVisuals;
	}

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
		return "Key";
	}
}
