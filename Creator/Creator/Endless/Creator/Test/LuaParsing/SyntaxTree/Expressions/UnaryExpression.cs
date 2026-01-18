using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033F RID: 831
	public class UnaryExpression : Expression
	{
		// Token: 0x17000254 RID: 596
		// (get) Token: 0x06000F5F RID: 3935 RVA: 0x0004748E File Offset: 0x0004568E
		public Token Operator { get; }

		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06000F60 RID: 3936 RVA: 0x00047496 File Offset: 0x00045696
		public Expression Right { get; }

		// Token: 0x06000F61 RID: 3937 RVA: 0x0004749E File Offset: 0x0004569E
		public UnaryExpression(Token op, Expression right)
		{
			this.Operator = op;
			this.Right = right;
		}

		// Token: 0x06000F62 RID: 3938 RVA: 0x000474B4 File Offset: 0x000456B4
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitUnaryExpression(this);
		}
	}
}
