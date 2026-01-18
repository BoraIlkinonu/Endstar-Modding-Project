using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032F RID: 815
	public class PrintStatement : Statement
	{
		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000F1F RID: 3871 RVA: 0x000471C0 File Offset: 0x000453C0
		public Expression Expression { get; }

		// Token: 0x06000F20 RID: 3872 RVA: 0x000471C8 File Offset: 0x000453C8
		public PrintStatement(Expression expression)
		{
			this.Expression = expression;
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x000471D7 File Offset: 0x000453D7
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitPrintStatement(this);
		}
	}
}
