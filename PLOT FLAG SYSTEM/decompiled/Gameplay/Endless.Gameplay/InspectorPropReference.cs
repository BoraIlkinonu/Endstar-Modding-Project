using System;

namespace Endless.Gameplay;

[Serializable]
public abstract class InspectorPropReference : InspectorReference
{
	internal abstract ReferenceFilter Filter { get; }

	public override string ToString()
	{
		return string.Format("{0}, {1}: {2}", base.ToString(), "Filter", Filter);
	}
}
