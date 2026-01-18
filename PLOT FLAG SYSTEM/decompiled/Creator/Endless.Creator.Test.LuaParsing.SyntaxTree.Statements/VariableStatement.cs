using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class VariableStatement : Statement
{
	public Token Name { get; }

	public Expression Initializer { get; }

	public List<Statement> TableStatements { get; }

	public VariableStatement(Token name, Expression initializer, List<Statement> tableStatements)
	{
		Name = name;
		Initializer = initializer;
		TableStatements = tableStatements;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitVariableStatement(this);
	}
}
