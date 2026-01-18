using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public abstract class Statement
{
	public abstract void Accept(ILuaVisitor visitor);
}
