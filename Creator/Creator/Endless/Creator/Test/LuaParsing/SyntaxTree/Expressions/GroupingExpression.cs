using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x0200033B RID: 827
	public class GroupingExpression : Expression
	{
		// Token: 0x1700024B RID: 587
		// (get) Token: 0x06000F4E RID: 3918 RVA: 0x000473C3 File Offset: 0x000455C3
		public Expression Expression { get; }

		// Token: 0x06000F4F RID: 3919 RVA: 0x000473CB File Offset: 0x000455CB
		public GroupingExpression(Expression expression)
		{
			this.Expression = expression;
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x000473DA File Offset: 0x000455DA
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitGroupingExpression(this);
		}
	}
}
