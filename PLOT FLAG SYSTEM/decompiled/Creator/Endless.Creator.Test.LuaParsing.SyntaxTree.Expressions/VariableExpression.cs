using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class VariableExpression : Expression
{
	public bool Negated { get; }

	public Token Name { get; }

	public VariableExpression(Token name, bool negated)
	{
		Name = name;
		Negated = negated;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitVariableExpression(this);
	}
}
