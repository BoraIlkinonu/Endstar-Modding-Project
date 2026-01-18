using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class ReturnStatement : Statement
{
	public Token Keyword { get; }

	public Expression Value { get; }

	public ReturnStatement(Token keyword, Expression value)
	{
		Keyword = keyword;
		Value = value;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitReturnStatement(this);
	}
}
