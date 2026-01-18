using System;
using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x02000319 RID: 793
	public class Environment
	{
		// Token: 0x06000E5B RID: 3675 RVA: 0x00043E15 File Offset: 0x00042015
		public Environment()
		{
			this.parent = null;
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00043E2F File Offset: 0x0004202F
		public Environment(Environment parent)
		{
			this.parent = parent;
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x00043E49 File Offset: 0x00042049
		public void AddDeclaration(VariableExpression variable)
		{
			if (!this.VariableIsDeclared(variable))
			{
				this.variables.Add(variable);
			}
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x00043E60 File Offset: 0x00042060
		public bool VariableIsDeclared(VariableExpression variable)
		{
			for (int i = 0; i < this.variables.Count; i++)
			{
				if (this.variables[i].Name.Lexeme == variable.Name.Lexeme)
				{
					return true;
				}
			}
			return this.parent.VariableIsDeclared(variable);
		}

		// Token: 0x04000C24 RID: 3108
		private Environment parent;

		// Token: 0x04000C25 RID: 3109
		private List<VariableExpression> variables = new List<VariableExpression>();
	}
}
