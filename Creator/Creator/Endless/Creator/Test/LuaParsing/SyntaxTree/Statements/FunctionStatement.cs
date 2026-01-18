using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x0200032C RID: 812
	public class FunctionStatement : Statement
	{
		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x00047105 File Offset: 0x00045305
		public Token Name { get; }

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000F11 RID: 3857 RVA: 0x0004710D File Offset: 0x0004530D
		public Token SubName { get; }

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000F12 RID: 3858 RVA: 0x00047115 File Offset: 0x00045315
		public List<Token> Parameters { get; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x0004711D File Offset: 0x0004531D
		public List<Statement> Body { get; }

		// Token: 0x06000F14 RID: 3860 RVA: 0x00047125 File Offset: 0x00045325
		public FunctionStatement(Token name, Token subname, List<Token> parameters, List<Statement> body)
		{
			this.Name = name;
			this.SubName = subname;
			this.Parameters = parameters;
			this.Body = body;
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x0004714A File Offset: 0x0004534A
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitFunctionStatement(this);
		}
	}
}
