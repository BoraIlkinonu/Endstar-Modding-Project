using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions
{
	// Token: 0x02000337 RID: 823
	public class CallExpression : Expression
	{
		// Token: 0x17000243 RID: 579
		// (get) Token: 0x06000F3E RID: 3902 RVA: 0x00047318 File Offset: 0x00045518
		public Expression Callee { get; }

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06000F3F RID: 3903 RVA: 0x00047320 File Offset: 0x00045520
		public Token Paren { get; }

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x06000F40 RID: 3904 RVA: 0x00047328 File Offset: 0x00045528
		public List<Expression> Arguments { get; }

		// Token: 0x06000F41 RID: 3905 RVA: 0x00047330 File Offset: 0x00045530
		public CallExpression(Expression callee, Token paren, List<Expression> arguments)
		{
			this.Callee = callee;
			this.Paren = paren;
			this.Arguments = arguments;
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x0004734D File Offset: 0x0004554D
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitCallExpression(this);
		}
	}
}
