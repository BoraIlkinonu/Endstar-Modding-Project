using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class IfStatement : Statement
{
	public Expression Condition { get; }

	public Statement ThenBranch { get; }

	public Statement ElseBranch { get; }

	public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
	{
		Condition = condition;
		ThenBranch = thenBranch;
		ElseBranch = elseBranch;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}
}
