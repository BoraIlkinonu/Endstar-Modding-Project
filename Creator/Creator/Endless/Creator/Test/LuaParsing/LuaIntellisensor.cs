using System;
using System.Collections.Generic;
using System.Diagnostics;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x0200031A RID: 794
	public class LuaIntellisensor
	{
		// Token: 0x06000E60 RID: 3680 RVA: 0x00043EBC File Offset: 0x000420BC
		public ScriptResult Execute(string text, bool debug = false)
		{
			this.hasErrors = false;
			Stopwatch stopwatch = new Stopwatch();
			Scanner scanner = new Scanner(text, new Action<int, string>(this.Error));
			stopwatch.Start();
			List<Token> list = scanner.ScanSourceForTokens();
			Parser parser = new Parser(list);
			List<Statement> list2 = parser.Parse();
			stopwatch.Stop();
			ScriptFinalizerVisitor scriptFinalizerVisitor = new ScriptFinalizerVisitor();
			List<ParsingError> list3 = new List<ParsingError>();
			int num = 0;
			foreach (Statement statement in list2)
			{
				ExpressionStatement expressionStatement = statement as ExpressionStatement;
				if (expressionStatement != null && expressionStatement.Expression is CallExpression)
				{
					list3.Add(new ParsingError("Endstar does not support calling functions within Global Scope.", 0, 0)
					{
						GlobalError = true
					});
				}
				statement.Accept(scriptFinalizerVisitor);
				num++;
			}
			ScriptResult result = scriptFinalizerVisitor.Result;
			result.Tokens.AddRange(list);
			if (debug)
			{
				global::UnityEngine.Debug.Log(string.Format("Token Count: {0}", result.Tokens.Count));
				global::UnityEngine.Debug.Log(string.Format("Declared Variables Count: {0}", result.DeclaredVariables.Count));
				foreach (string text2 in result.DeclaredVariables)
				{
					global::UnityEngine.Debug.Log("Declared Variable: " + text2);
				}
				global::UnityEngine.Debug.Log(string.Format("Declared Free Function Count: {0}", result.DeclaredVariables.Count));
				foreach (string text3 in result.DeclaredFreeFunctions)
				{
					global::UnityEngine.Debug.Log("Declared Free Function Name: " + text3);
				}
				global::UnityEngine.Debug.Log(string.Format("Declared Table Function Count: {0}", result.TableFunctions.Count));
				foreach (KeyValuePair<string, List<string>> keyValuePair in result.TableFunctions)
				{
					global::UnityEngine.Debug.Log("Table Name: " + keyValuePair.Key);
					foreach (string text4 in keyValuePair.Value)
					{
						global::UnityEngine.Debug.Log("Table Function Name: " + text4);
					}
				}
				global::UnityEngine.Debug.Log(string.Format("Variable to Type Count: {0}", result.VariableNameToType.Count));
				foreach (KeyValuePair<string, string> keyValuePair2 in result.VariableNameToType)
				{
					global::UnityEngine.Debug.Log("Variable Name: " + keyValuePair2.Key + ", Type Name: " + keyValuePair2.Value);
				}
			}
			result.Errors.AddRange(list3);
			result.Errors.AddRange(parser.ErrorMessages);
			return result;
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x00044214 File Offset: 0x00042414
		public void Error(int line, string message)
		{
			this.Error(line, "", message);
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x00044223 File Offset: 0x00042423
		public void Error(int line, string where, string message)
		{
			this.hasErrors = true;
			global::UnityEngine.Debug.Log(string.Format("[line {0}] Error - {1}: {2}", line, where, message));
		}

		// Token: 0x04000C26 RID: 3110
		private bool hasErrors;
	}
}
