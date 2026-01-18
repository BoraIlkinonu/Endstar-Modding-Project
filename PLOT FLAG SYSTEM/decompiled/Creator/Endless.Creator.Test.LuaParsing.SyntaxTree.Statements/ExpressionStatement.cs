using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class ExpressionStatement : Statement
{
	public Expression Expression { get; }

	public ExpressionStatement(Expression expression)
	{
		Expression = expression;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitExpressionStatement(this);
	}
}
