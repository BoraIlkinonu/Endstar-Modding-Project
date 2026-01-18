using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors
{
	// Token: 0x02000328 RID: 808
	public class ScriptFinalizerVisitor : ILuaVisitor
	{
		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000EEB RID: 3819 RVA: 0x00046B78 File Offset: 0x00044D78
		public ScriptResult Result
		{
			get
			{
				return this.scriptResult;
			}
		}

		// Token: 0x06000EEC RID: 3820 RVA: 0x00046B80 File Offset: 0x00044D80
		public void VisitBinaryExpression(BinaryExpression binaryExpression)
		{
			binaryExpression.Left.Accept(this);
			binaryExpression.Right.Accept(this);
		}

		// Token: 0x06000EED RID: 3821 RVA: 0x00046B9C File Offset: 0x00044D9C
		public void VisitCallExpression(CallExpression callExpression)
		{
			callExpression.Callee.Accept(this);
			foreach (Expression expression in callExpression.Arguments)
			{
				if (expression != null)
				{
					expression.Accept(this);
				}
			}
		}

		// Token: 0x06000EEE RID: 3822 RVA: 0x00046C00 File Offset: 0x00044E00
		public void VisitGetExpression(GetExpression getExpression)
		{
			Expression obj = getExpression.Obj;
			if (obj == null)
			{
				return;
			}
			obj.Accept(this);
		}

		// Token: 0x06000EEF RID: 3823 RVA: 0x00046C13 File Offset: 0x00044E13
		public void VisitSetExpression(SetExpression setExpression)
		{
			setExpression.Object.Accept(this);
		}

		// Token: 0x06000EF0 RID: 3824 RVA: 0x00046C21 File Offset: 0x00044E21
		public void VisitGroupingExpression(GroupingExpression groupingExpression)
		{
			groupingExpression.Expression.Accept(this);
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x000056F3 File Offset: 0x000038F3
		public void VisitLiteralExpression(LiteralExpression literalExpression)
		{
		}

		// Token: 0x06000EF2 RID: 3826 RVA: 0x00046C2F File Offset: 0x00044E2F
		public void VisitLogicalExpression(LogicalExpression logicalExpression)
		{
			logicalExpression.Left.Accept(this);
			logicalExpression.Right.Accept(this);
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x000056F3 File Offset: 0x000038F3
		public void VisitUnaryExpression(UnaryExpression unaryExpression)
		{
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x000056F3 File Offset: 0x000038F3
		public void VisitConcatenateExpression(ConcatenateExpression concatenateExpression)
		{
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x000056F3 File Offset: 0x000038F3
		public void VisitVariableExpression(VariableExpression variableExpression)
		{
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x00046C49 File Offset: 0x00044E49
		public void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
		{
			assignmentExpression.Value.Accept(this);
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x00046C58 File Offset: 0x00044E58
		public void VisitBlockStatement(BlockStatement blockStatement)
		{
			foreach (Statement statement in blockStatement.Statements)
			{
				statement.Accept(this);
			}
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x00046CA4 File Offset: 0x00044EA4
		public void VisitExpressionStatement(ExpressionStatement expressionStatement)
		{
			if (expressionStatement.Expression == null)
			{
				Debug.LogWarning("Expression Statement Expression was null");
				return;
			}
			expressionStatement.Expression.Accept(this);
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x00046CC5 File Offset: 0x00044EC5
		public void VisitIfStatement(IfStatement ifStatement)
		{
			Expression condition = ifStatement.Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			Statement thenBranch = ifStatement.ThenBranch;
			if (thenBranch != null)
			{
				thenBranch.Accept(this);
			}
			Statement elseBranch = ifStatement.ElseBranch;
			if (elseBranch == null)
			{
				return;
			}
			elseBranch.Accept(this);
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x00046CFC File Offset: 0x00044EFC
		public void VisitPrintStatement(PrintStatement printStatement)
		{
			Expression expression = printStatement.Expression;
			if (expression == null)
			{
				return;
			}
			expression.Accept(this);
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x00046D10 File Offset: 0x00044F10
		public void VisitFunctionStatement(FunctionStatement functionStatement)
		{
			if (functionStatement.Name == null)
			{
				return;
			}
			if (functionStatement.SubName != null)
			{
				this.scriptResult.AddTableFunction(functionStatement.Name, functionStatement.SubName);
				return;
			}
			foreach (Statement statement in functionStatement.Body)
			{
				statement.Accept(this);
			}
			this.scriptResult.DeclaredFreeFunctions.Add(functionStatement.Name.Lexeme);
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x00046DA8 File Offset: 0x00044FA8
		public void VisitReturnStatement(ReturnStatement returnStatement)
		{
			Expression value = returnStatement.Value;
			if (value == null)
			{
				return;
			}
			value.Accept(this);
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x00046DBC File Offset: 0x00044FBC
		public void VisitVariableStatement(VariableStatement variableStatement)
		{
			this.scriptResult.DeclaredVariables.Add(variableStatement.Name.Lexeme);
			if (variableStatement.Initializer != null)
			{
				variableStatement.Initializer.Accept(this);
				CallExpression callExpression = variableStatement.Initializer as CallExpression;
				if (callExpression != null)
				{
					GetExpression getExpression = callExpression.Callee as GetExpression;
					if (getExpression != null)
					{
						VariableExpression variableExpression = getExpression.Obj as VariableExpression;
						if (variableExpression != null)
						{
							if (getExpression.Name.Lexeme == "TryGetComponent")
							{
								CallExpression callExpression2 = variableStatement.Initializer as CallExpression;
								if (callExpression2 != null)
								{
									GetExpression getExpression2 = callExpression2.Arguments.FirstOrDefault<Expression>() as GetExpression;
									if (getExpression2 != null && getExpression2.Name != null)
									{
										string lexeme = getExpression2.Name.Lexeme;
										this.scriptResult.AddType(lexeme);
										this.scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, lexeme);
										goto IL_01D8;
									}
									goto IL_01D8;
								}
							}
							if (getExpression.Name.Lexeme == "GetContext" && variableStatement.Initializer is CallExpression)
							{
								if (callExpression.Arguments.Count == 0)
								{
									this.scriptResult.AddType("Context");
									this.scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, "Context");
								}
							}
							else if (getExpression.Name.Lexeme == "Create" && variableStatement.Initializer is CallExpression)
							{
								if (variableExpression.Name.Lexeme == "Vector3")
								{
									this.scriptResult.AddType("unityVector3");
									this.scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, "unityVector3");
								}
							}
							else
							{
								this.scriptResult.AddVariableAndType(variableStatement.Name.Lexeme, variableExpression.Name.Lexeme);
							}
						}
					}
				}
			}
			IL_01D8:
			if (variableStatement.TableStatements != null)
			{
				this.scriptResult.AddType(variableStatement.Name.Lexeme);
				variableStatement.TableStatements.ForEach(delegate(Statement statement)
				{
					statement.Accept(this);
				});
			}
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x00046FD6 File Offset: 0x000451D6
		public void VisitWhileStatement(WhileStatement whileStatement)
		{
			Expression condition = whileStatement.Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			Statement body = whileStatement.Body;
			if (body == null)
			{
				return;
			}
			body.Accept(this);
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x00046FFC File Offset: 0x000451FC
		public void VisitForStatement(ForStatement forStatement)
		{
			Expression initialAssignment = forStatement.InitialAssignment;
			if (initialAssignment != null)
			{
				initialAssignment.Accept(this);
			}
			Expression maxValue = forStatement.MaxValue;
			if (maxValue != null)
			{
				maxValue.Accept(this);
			}
			Expression step = forStatement.Step;
			if (step != null)
			{
				step.Accept(this);
			}
			Statement body = forStatement.Body;
			if (body == null)
			{
				return;
			}
			body.Accept(this);
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x00047050 File Offset: 0x00045250
		public void VisitTableStatement(TableStatement tableStatement)
		{
			List<Statement> statements = tableStatement.Statements;
			if (statements == null)
			{
				return;
			}
			statements.ForEach(delegate(Statement statement)
			{
				statement.Accept(this);
			});
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x000056F3 File Offset: 0x000038F3
		public void VisitMetaTableIndexStatement(MetaTableIndexStatement metaTableIndexStatement)
		{
		}

		// Token: 0x04000C99 RID: 3225
		private ScriptResult scriptResult = new ScriptResult();
	}
}
