using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000339 RID: 825
	public abstract class Expression
	{
		// Token: 0x06000F48 RID: 3912
		public abstract void Accept(ILuaVisitor visitor);
	}
}
