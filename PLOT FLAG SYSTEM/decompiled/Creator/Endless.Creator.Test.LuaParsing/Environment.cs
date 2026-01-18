using System.Collections.Generic;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;

namespace Endless.Creator.Test.LuaParsing;

public class Environment
{
	private Environment parent;

	private List<VariableExpression> variables = new List<VariableExpression>();

	public Environment()
	{
		parent = null;
	}

	public Environment(Environment parent)
	{
		this.parent = parent;
	}

	public void AddDeclaration(VariableExpression variable)
	{
		if (!VariableIsDeclared(variable))
		{
			variables.Add(variable);
		}
	}

	public bool VariableIsDeclared(VariableExpression variable)
	{
		for (int i = 0; i < variables.Count; i++)
		{
			if (variables[i].Name.Lexeme == variable.Name.Lexeme)
			{
				return true;
			}
		}
		return parent.VariableIsDeclared(variable);
	}
}
