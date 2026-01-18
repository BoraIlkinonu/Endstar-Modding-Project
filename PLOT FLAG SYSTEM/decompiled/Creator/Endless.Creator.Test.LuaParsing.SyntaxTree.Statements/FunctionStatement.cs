using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class FunctionStatement : Statement
{
	public Token Name { get; }

	public Token SubName { get; }

	public List<Token> Parameters { get; }

	public List<Statement> Body { get; }

	public FunctionStatement(Token name, Token subname, List<Token> parameters, List<Statement> body)
	{
		Name = name;
		SubName = subname;
		Parameters = parameters;
		Body = body;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitFunctionStatement(this);
	}
}
