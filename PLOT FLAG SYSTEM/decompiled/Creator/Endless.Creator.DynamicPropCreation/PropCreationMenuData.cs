using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/PropCreationMenuData")]
public class PropCreationMenuData : PropCreationData
{
	[SerializeField]
	private List<PropCreationData> options = new List<PropCreationData>();

	public override bool IsSubMenu => true;

	public List<PropCreationData> Options => options;
}
