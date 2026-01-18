using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public class CallExpression : Expression
{
	public Expression Callee { get; }

	public Token Paren { get; }

	public List<Expression> Arguments { get; }

	public CallExpression(Expression callee, Token paren, List<Expression> arguments)
	{
		Callee = callee;
		Paren = paren;
		Arguments = arguments;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitCallExpression(this);
	}
}
