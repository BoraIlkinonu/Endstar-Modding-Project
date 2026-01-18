using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class WhileStatement : Statement
{
	public Expression Condition { get; }

	public Statement Body { get; }

	public WhileStatement(Expression condition, Statement body)
	{
		Condition = condition;
		Body = body;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitWhileStatement(this);
	}
}
