using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class AssignmentExpression : Expression
{
	public Token Name { get; }

	public Expression Value { get; }

	public AssignmentExpression(Token name, Expression value)
	{
		Name = name;
		Value = value;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitAssignmentExpression(this);
	}
}
