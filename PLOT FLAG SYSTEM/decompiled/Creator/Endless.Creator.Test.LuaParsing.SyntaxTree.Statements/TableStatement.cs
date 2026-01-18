using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class TableStatement : Statement
{
	public Token Name { get; }

	public List<Statement> Statements { get; }

	public TableStatement(Token name, List<Statement> statements)
	{
		Name = name;
		Statements = statements;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitTableStatement(this);
	}
}
