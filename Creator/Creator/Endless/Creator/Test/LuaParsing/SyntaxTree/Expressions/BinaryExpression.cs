using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000336 RID: 822
	public class BinaryExpression : Expression
	{
		// Token: 0x17000240 RID: 576
		// (get) Token: 0x06000F39 RID: 3897 RVA: 0x000472DA File Offset: 0x000454DA
		public Expression Left { get; }

		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000F3A RID: 3898 RVA: 0x000472E2 File Offset: 0x000454E2
		public Token Operator { get; }

		// Token: 0x17000242 RID: 578
		// (get) Token: 0x06000F3B RID: 3899 RVA: 0x000472EA File Offset: 0x000454EA
		public Expression Right { get; }

		// Token: 0x06000F3C RID: 3900 RVA: 0x000472F2 File Offset: 0x000454F2
		public BinaryExpression(Expression left, Token op, Expression right)
		{
			this.Left = left;
			this.Operator = op;
			this.Right = right;
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x0004730F File Offset: 0x0004550F
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitBinaryExpression(this);
		}
	}
}
