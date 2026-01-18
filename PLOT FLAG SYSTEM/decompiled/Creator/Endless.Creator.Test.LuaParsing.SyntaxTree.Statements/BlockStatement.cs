using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class BlockStatement : Statement
{
	public IReadOnlyList<Statement> Statements { get; }

	public BlockStatement(List<Statement> statements)
	{
		Statements = statements;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitBlockStatement(this);
	}
}
