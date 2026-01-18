using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000332 RID: 818
	public class TableStatement : Statement
	{
		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000F28 RID: 3880 RVA: 0x0004720F File Offset: 0x0004540F
		public Token Name { get; }

		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000F29 RID: 3881 RVA: 0x00047217 File Offset: 0x00045417
		public List<Statement> Statements { get; }

		// Token: 0x06000F2A RID: 3882 RVA: 0x0004721F File Offset: 0x0004541F
		public TableStatement(Token name, List<Statement> statements)
		{
			this.Name = name;
			this.Statements = statements;
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x00047235 File Offset: 0x00045435
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitTableStatement(this);
		}
	}
}
