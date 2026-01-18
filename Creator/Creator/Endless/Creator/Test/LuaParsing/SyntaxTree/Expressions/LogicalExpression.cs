using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033D RID: 829
	public class LogicalExpression : Expression
	{
		// Token: 0x1700024E RID: 590
		// (get) Token: 0x06000F55 RID: 3925 RVA: 0x00047412 File Offset: 0x00045612
		public Expression Left { get; }

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06000F56 RID: 3926 RVA: 0x0004741A File Offset: 0x0004561A
		public Token Operator { get; }

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06000F57 RID: 3927 RVA: 0x00047422 File Offset: 0x00045622
		public Expression Right { get; }

		// Token: 0x06000F58 RID: 3928 RVA: 0x0004742A File Offset: 0x0004562A
		public LogicalExpression(Expression left, Token op, Expression right)
		{
			this.Left = left;
			this.Operator = op;
			this.Right = right;
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x00047447 File Offset: 0x00045647
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitLogicalExpression(this);
		}
	}
}
