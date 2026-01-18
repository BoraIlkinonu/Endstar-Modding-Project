using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class GetExpression : Expression
{
	public Expression Obj { get; }

	public Token Name { get; }

	public GetExpression(Expression obj, Token name)
	{
		Obj = obj;
		Name = name;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitGetExpression(this);
	}
}
