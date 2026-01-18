using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000333 RID: 819
	public class VariableStatement : Statement
	{
		// Token: 0x17000239 RID: 569
		// (get) Token: 0x06000F2C RID: 3884 RVA: 0x0004723E File Offset: 0x0004543E
		public Token Name { get; }

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000F2D RID: 3885 RVA: 0x00047246 File Offset: 0x00045446
		public Expression Initializer { get; }

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x06000F2E RID: 3886 RVA: 0x0004724E File Offset: 0x0004544E
		public List<Statement> TableStatements { get; }

		// Token: 0x06000F2F RID: 3887 RVA: 0x00047256 File Offset: 0x00045456
		public VariableStatement(Token name, Expression initializer, List<Statement> tableStatements)
		{
			this.Name = name;
			this.Initializer = initializer;
			this.TableStatements = tableStatements;
		}

		// Token: 0x06000F30 RID: 3888 RVA: 0x00047273 File Offset: 0x00045473
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitVariableStatement(this);
		}
	}
}
