using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class UnaryExpression : Expression
{
	public Token Operator { get; }

	public Expression Right { get; }

	public UnaryExpression(Token op, Expression right)
	{
		Operator = op;
		Right = right;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitUnaryExpression(this);
	}
}
