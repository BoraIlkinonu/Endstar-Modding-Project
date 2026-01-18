using System.Linq;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;

public class ScriptFinalizerVisitor : ILuaVisitor
{
	private ScriptResult scriptResult = new ScriptResult();

	public ScriptResult Result => scriptResult;

	public void VisitBinaryExpression(BinaryExpression binaryExpression)
	{
		binaryExpression.Left.Accept(this);
		binaryExpression.Right.Accept(this);
	}

	public void VisitCallExpression(CallExpression callExpression)
	{
		callExpression.Callee.Accept(this);
		foreach (Expression argument in callExpression.Arguments)
		{
			argument?.Accept(this);
		}
	}

	public void VisitGetExpression(GetExpression getExpression)
	{
		getExpression.Obj?.Accept(this);
	}

	public void VisitSetExpression(SetExpression setExpression)
	{
		setExpression.Object.Accept(this);
	}

	public void VisitGroupingExpression(GroupingExpression groupingExpression)
	{
		groupingExpression.Expression.Accept(this);
	}

	public void VisitLiteralExpression(LiteralExpression literalExpression)
	{
	}

	public void VisitLogicalExpression(LogicalExpression logicalExpression)
	{
		logicalExpression.Left.Accept(this);
		logicalExpression.Right.Accept(this);
	}

	public void VisitUnaryExpression(UnaryExpression unaryExpression)
	{
	}

	public void VisitConcatenateExpression(ConcatenateExpression concatenateExpression)
	{
	}

	public void VisitVariableExpression(VariableExpression variableExpression)
	{
	}

	public void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
	{
		assignmentExpression.Value.Accept(this);
	}

	public void VisitBlockStatement(BlockStatement blockStatement)
	{
		foreach (Statement statement in blockStatement.Statements)
		{
			statement.Accept(this);
		}
	}

	public void VisitExpressionStatement(ExpressionStatement expressionStatement)
	{
		if (expressionStatement.Expression == null)
		{
			Debug.LogWarning("Expression Statement Expression was null");
		}
		else
		{
			expressionStatement.Expression.Accept(this);
		}
	}

	public void VisitIfStatement(IfStatement ifStatement)
	{
		ifStatement.Condition?.Accept(this);
		ifStatement.ThenBranch?.Accept(this);
		ifStatement.ElseBranch?.Accept(this);
	}

	public void VisitPrintStatement(PrintStatement printStatement)
	{
		printStatement.Expression?.Accept(this);
	}

	public void VisitFunctionStatement(FunctionStatement functionStatement)
	{
		if (functionStatement.Name == null)
		{
			return;
		}
		if (functionStatement.SubName != null)
		{
			scriptResult.AddTableFunction(functionStatement.Name, functionStatement.SubName);
			return;
		}
		foreach (Statement item in functionStatement.Body)
		{
			item.Accept(this);
		}
		scriptResult.DeclaredFreeFunctions.Add(functionStatement.Name.Lexeme);
	}

	public void VisitReturnStatement(ReturnStatement returnStatement)
	{
		returnStatement.Value?.Accept(this);
	}

	public void VisitVariableStatement(VariableStatement variableStatement)
	{
		scriptResult.DeclaredVariables.Add(variableStatement.Name.Lexeme);
		if (variableStatement.Initializer != null)
		{
			variableStatement.Initializer.Accept(this);
			if (variableStatement.Initializer is CallExpression { Callee: GetExpression { Obj: VariableExpression obj } callee } callExpression)
			{
				if (callee.Name.Lexeme == "TryGetComponent" && variableStatement.Initializer is CallExpression callExpression2)
				{
					if (callExpression2.Arguments.FirstOrDefault() is GetExpression { Name: not null } getExpression)
					{
						string lexeme = getExpression.Name.Lexeme;
						scriptResult.AddType(lexeme);
						scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, lexeme);
					}
				}
				else if (callee.Name.Lexeme == "GetContext" && variableStatement.Initializer is CallExpression)
				{
					if (callExpression.Arguments.Count == 0)
					{
						scriptResult.AddType("Context");
						scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, "Context");
					}
				}
				else if (callee.Name.Lexeme == "Create" && variableStatement.Initializer is CallExpression)
				{
					if (obj.Name.Lexeme == "Vector3")
					{
						scriptResult.AddType("unityVector3");
						scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, "unityVector3");
					}
				}
				else
				{
					scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, obj.Name.Lexeme);
				}
			}
		}
		if (variableStatement.TableStatements != null)
		{
			scriptResult.AddType(variableStatement.Name.Lexeme);
			variableStatement.TableStatements.ForEach(delegate(Statement statement)
			{
				statement.Accept(this);
			});
		}
	}

	public void VisitWhileStatement(WhileStatement whileStatement)
	{
		whileStatement.Condition?.Accept(this);
		whileStatement.Body?.Accept(this);
	}

	public void VisitForStatement(ForStatement forStatement)
	{
		forStatement.InitialAssignment?.Accept(this);
		forStatement.MaxValue?.Accept(this);
		forStatement.Step?.Accept(this);
		forStatement.Body?.Accept(this);
	}

	public void VisitTableStatement(TableStatement tableStatement)
	{
		tableStatement.Statements?.ForEach(delegate(Statement statement)
		{
			statement.Accept(this);
		});
	}

	public void VisitMetaTableIndexStatement(MetaTableIndexStatement metaTableIndexStatement)
	{
	}
}
