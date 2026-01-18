using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000329 RID: 809
	public class BlockStatement : Statement
	{
		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000F04 RID: 3844 RVA: 0x00047077 File Offset: 0x00045277
		public IReadOnlyList<Statement> Statements { get; }

		// Token: 0x06000F05 RID: 3845 RVA: 0x0004707F File Offset: 0x0004527F
		public BlockStatement(List<Statement> statements)
		{
			this.Statements = statements;
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x0004708E File Offset: 0x0004528E
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitBlockStatement(this);
		}
	}
}
