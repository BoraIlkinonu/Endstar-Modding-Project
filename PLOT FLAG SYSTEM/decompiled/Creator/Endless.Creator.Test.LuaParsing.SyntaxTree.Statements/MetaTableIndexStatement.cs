using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;

public class MetaTableIndexStatement : Statement
{
	public Token Name { get; }

	public Token Target { get; }

	public MetaTableIndexStatement(Token name, Token target)
	{
		Name = name;
		Target = target;
	}

	public override void Accept(ILuaVisitor visitor)
	{
		visitor.VisitMetaTableIndexStatement(this);
	}
}
