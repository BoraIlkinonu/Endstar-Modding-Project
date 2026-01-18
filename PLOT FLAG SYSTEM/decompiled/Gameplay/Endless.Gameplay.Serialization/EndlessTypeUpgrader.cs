using Endless.Props.Scripting;

namespace Endless.Gameplay.Serialization;

public abstract class EndlessTypeUpgrader
{
	public abstract void Upgrade(MemberChange memberChange, bool isLua);

	public abstract void Upgrade(InspectorScriptValue inspectorScriptValue);
}
