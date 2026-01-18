using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033E RID: 830
	public class SetExpression : Expression
	{
		// Token: 0x17000251 RID: 593
		// (get) Token: 0x06000F5A RID: 3930 RVA: 0x00047450 File Offset: 0x00045650
		public Expression Object { get; }

		// Token: 0x17000252 RID: 594
		// (get) Token: 0x06000F5B RID: 3931 RVA: 0x00047458 File Offset: 0x00045658
		public Token Name { get; }

		// Token: 0x17000253 RID: 595
		// (get) Token: 0x06000F5C RID: 3932 RVA: 0x00047460 File Offset: 0x00045660
		public Expression Value { get; }

		// Token: 0x06000F5D RID: 3933 RVA: 0x00047468 File Offset: 0x00045668
		public SetExpression(Expression obj, Token name, Expression value)
		{
			this.Object = obj;
			this.Name = name;
			this.Value = value;
		}

		// Token: 0x06000F5E RID: 3934 RVA: 0x00047485 File Offset: 0x00045685
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitSetExpression(this);
		}
	}
}
