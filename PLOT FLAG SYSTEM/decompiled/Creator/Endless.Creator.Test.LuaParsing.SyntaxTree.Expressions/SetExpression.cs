using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class SetExpression : Expression
{
	public Expression Object { get; }

	public Token Name { get; }

	public Expression Value { get; }

	public SetExpression(Expression obj, Token name, Expression value)
	{
		Object = obj;
		Name = name;
		Value = value;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitSetExpression(this);
	}
}
