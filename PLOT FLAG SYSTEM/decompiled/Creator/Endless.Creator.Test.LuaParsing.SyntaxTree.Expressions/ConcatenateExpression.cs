using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class ConcatenateExpression : Expression
{
	public Expression Left { get; }

	public Token Operator { get; }

	public Expression Right { get; }

	public ConcatenateExpression(Expression left, Token op, Expression right)
	{
		Left = left;
		Operator = op;
		Right = right;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitConcatenateExpression(this);
	}
}
