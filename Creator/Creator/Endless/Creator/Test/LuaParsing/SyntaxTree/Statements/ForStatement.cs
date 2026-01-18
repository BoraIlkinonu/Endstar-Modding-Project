using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032B RID: 811
	public class ForStatement : Statement
	{
		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000F0A RID: 3850 RVA: 0x000470B7 File Offset: 0x000452B7
		public Expression InitialAssignment { get; }

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000F0B RID: 3851 RVA: 0x000470BF File Offset: 0x000452BF
		public Expression MaxValue { get; }

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000F0C RID: 3852 RVA: 0x000470C7 File Offset: 0x000452C7
		public Expression Step { get; }

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x000470CF File Offset: 0x000452CF
		public Statement Body { get; }

		// Token: 0x06000F0E RID: 3854 RVA: 0x000470D7 File Offset: 0x000452D7
		public ForStatement(Expression initialAssignment, Expression maxValue, Expression step, Statement body)
		{
			this.InitialAssignment = initialAssignment;
			this.MaxValue = maxValue;
			this.Step = step;
			this.Body = body;
		}

		// Token: 0x06000F0F RID: 3855 RVA: 0x000470FC File Offset: 0x000452FC
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitForStatement(this);
		}
	}
}
