using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033A RID: 826
	public class GetExpression : Expression
	{
		// Token: 0x17000249 RID: 585
		// (get) Token: 0x06000F4A RID: 3914 RVA: 0x00047394 File Offset: 0x00045594
		public Expression Obj { get; }

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x06000F4B RID: 3915 RVA: 0x0004739C File Offset: 0x0004559C
		public Token Name { get; }

		// Token: 0x06000F4C RID: 3916 RVA: 0x000473A4 File Offset: 0x000455A4
		public GetExpression(Expression obj, Token name)
		{
			this.Obj = obj;
			this.Name = name;
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x000473BA File Offset: 0x000455BA
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitGetExpression(this);
		}
	}
}
