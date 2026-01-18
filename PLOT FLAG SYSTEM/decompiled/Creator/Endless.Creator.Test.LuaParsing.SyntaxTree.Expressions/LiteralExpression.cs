using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class LiteralExpression : Expression
{
	public bool Negated { get; }

	public object Value { get; }

	public LiteralExpression(object value, bool negated = false)
	{
		Value = value;
		Negated = negated;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}
}
