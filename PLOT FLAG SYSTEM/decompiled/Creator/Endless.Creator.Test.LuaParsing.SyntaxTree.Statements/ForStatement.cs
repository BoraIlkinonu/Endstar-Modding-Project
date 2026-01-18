using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class ForStatement : Statement
{
	public Expression InitialAssignment { get; }

	public Expression MaxValue { get; }

	public Expression Step { get; }

	public Statement Body { get; }

	public ForStatement(Expression initialAssignment, Expression maxValue, Expression step, Statement body)
	{
		InitialAssignment = initialAssignment;
		MaxValue = maxValue;
		Step = step;
		Body = body;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitForStatement(this);
	}
}
