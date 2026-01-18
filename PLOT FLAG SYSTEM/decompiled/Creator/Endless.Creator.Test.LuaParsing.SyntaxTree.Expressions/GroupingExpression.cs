using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class GroupingExpression : Expression
{
	public Expression Expression { get; }

	public GroupingExpression(Expression expression)
	{
		Expression = expression;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitGroupingExpression(this);
	}
}
