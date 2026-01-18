using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000331 RID: 817
	public abstract class Statement
	{
		// Token: 0x06000F26 RID: 3878
		public abstract void Accept(ILuaVisitor visitor);
	}
}
