using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032D RID: 813
	public class IfStatement : Statement
	{
		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000F16 RID: 3862 RVA: 0x00047153 File Offset: 0x00045353
		public Expression Condition { get; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000F17 RID: 3863 RVA: 0x0004715B File Offset: 0x0004535B
		public Statement ThenBranch { get; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000F18 RID: 3864 RVA: 0x00047163 File Offset: 0x00045363
		public Statement ElseBranch { get; }

		// Token: 0x06000F19 RID: 3865 RVA: 0x0004716B File Offset: 0x0004536B
		public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
		{
			this.Condition = condition;
			this.ThenBranch = thenBranch;
			this.ElseBranch = elseBranch;
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00047188 File Offset: 0x00045388
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitIfStatement(this);
		}
	}
}
