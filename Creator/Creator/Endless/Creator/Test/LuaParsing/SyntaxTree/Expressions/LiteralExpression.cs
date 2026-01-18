using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033C RID: 828
	public class LiteralExpression : Expression
	{
		// Token: 0x1700024C RID: 588
		// (get) Token: 0x06000F51 RID: 3921 RVA: 0x000473E3 File Offset: 0x000455E3
		public bool Negated { get; }

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x06000F52 RID: 3922 RVA: 0x000473EB File Offset: 0x000455EB
		public object Value { get; }

		// Token: 0x06000F53 RID: 3923 RVA: 0x000473F3 File Offset: 0x000455F3
		public LiteralExpression(object value, bool negated = false)
		{
			this.Value = value;
			this.Negated = negated;
		}

		// Token: 0x06000F54 RID: 3924 RVA: 0x00047409 File Offset: 0x00045609
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitLiteralExpression(this);
		}
	}
}
