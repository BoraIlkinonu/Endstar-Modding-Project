using Endless.Gameplay;

namespace Endless.Creator.UI;

public class UIInstanceReferenceView : UIBaseInstanceReferenceView<InstanceReference, UIInstanceReferenceView.Styles>
{
	public enum Styles
	{
		Default,
		NoneOrContext
	}

	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.NonStatic;
}
