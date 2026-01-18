using System;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Statements
{
	// Token: 0x02000330 RID: 816
	public class ReturnStatement : Statement
	{
		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000F22 RID: 3874 RVA: 0x000471E0 File Offset: 0x000453E0
		public Token Keyword { get; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000F23 RID: 3875 RVA: 0x000471E8 File Offset: 0x000453E8
		public Expression Value { get; }

		// Token: 0x06000F24 RID: 3876 RVA: 0x000471F0 File Offset: 0x000453F0
		public ReturnStatement(Token keyword, Expression value)
		{
			this.Keyword = keyword;
			this.Value = value;
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x00047206 File Offset: 0x00045406
		public override void Accept(ILuaVisitor visitor)
		{
			visitor.VisitReturnStatement(this);
		}
	}
}
