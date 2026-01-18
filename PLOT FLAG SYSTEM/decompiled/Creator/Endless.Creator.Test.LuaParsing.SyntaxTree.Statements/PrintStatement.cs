using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class PrintStatement : Statement
{
	public Expression Expression { get; }

	public PrintStatement(Expression expression)
	{
		Expression = expression;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitPrintStatement(this);
	}
}
