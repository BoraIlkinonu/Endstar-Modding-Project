using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using UnityEngine;

namespace Endless.Creator.UI;

public class UINpcInstanceReferenceView : UIBaseInstanceReferenceView<NpcInstanceReference, UINpcInstanceReferenceView.Styles>
{
	public enum Styles
	{
		Default,
		NoneOrContext
	}

	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.Npc;

	protected override Vector3 GetPinOffset(PropLibrary.RuntimePropInfo endlessDefinition)
	{
		return base.GetPinOffset(endlessDefinition) - Vector3.up * 1f;
	}
}
