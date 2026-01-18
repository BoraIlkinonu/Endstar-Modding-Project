namespace Endless.Gameplay;

public struct PlanNode
{
	public GoapAction.ActionKind Action;

	public unsafe PlanNode** Prerequisites;

	public int NumPrerequisites;

	public float Cost;
}
