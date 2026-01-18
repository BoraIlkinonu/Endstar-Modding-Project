using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000334 RID: 820
	public class WhileStatement : Statement
	{
		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000F31 RID: 3889 RVA: 0x0004727C File Offset: 0x0004547C
		public Expression Condition { get; }

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000F32 RID: 3890 RVA: 0x00047284 File Offset: 0x00045484
		public Statement Body { get; }

		// Token: 0x06000F33 RID: 3891 RVA: 0x0004728C File Offset: 0x0004548C
		public WhileStatement(Expression condition, Statement body)
		{
			this.Condition = condition;
			this.Body = body;
		}

		// Token: 0x06000F34 RID: 3892 RVA: 0x000472A2 File Offset: 0x000454A2
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitWhileStatement(this);
		}
	}
}
