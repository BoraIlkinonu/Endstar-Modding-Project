using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032A RID: 810
	public class ExpressionStatement : Statement
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000F07 RID: 3847 RVA: 0x00047097 File Offset: 0x00045297
		public Expression Expression { get; }

		// Token: 0x06000F08 RID: 3848 RVA: 0x0004709F File Offset: 0x0004529F
		public ExpressionStatement(Expression expression)
		{
			this.Expression = expression;
		}

		// Token: 0x06000F09 RID: 3849 RVA: 0x000470AE File Offset: 0x000452AE
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitExpressionStatement(this);
		}
	}
}
