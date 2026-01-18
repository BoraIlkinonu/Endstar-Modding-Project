using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000338 RID: 824
	public class ConcatenateExpression : Expression
	{
		// Token: 0x17000246 RID: 582
		// (get) Token: 0x06000F43 RID: 3907 RVA: 0x00047356 File Offset: 0x00045556
		public Expression Left { get; }

		// Token: 0x17000247 RID: 583
		// (get) Token: 0x06000F44 RID: 3908 RVA: 0x0004735E File Offset: 0x0004555E
		public Token Operator { get; }

		// Token: 0x17000248 RID: 584
		// (get) Token: 0x06000F45 RID: 3909 RVA: 0x00047366 File Offset: 0x00045566
		public Expression Right { get; }

		// Token: 0x06000F46 RID: 3910 RVA: 0x0004736E File Offset: 0x0004556E
		public ConcatenateExpression(Expression left, Token op, Expression right)
		{
			this.Left = left;
			this.Operator = op;
			this.Right = right;
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x0004738B File Offset: 0x0004558B
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitConcatenateExpression(this);
		}
	}
}
