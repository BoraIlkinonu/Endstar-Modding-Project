using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

public abstract class Expression
{
	public abstract void Accept(ILuaVisitor visitor);
}
