using System.Collections.Generic;
using System.Diagnostics;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Statements;
using Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors;
using UnityEngine;

namespace Endless.Creator.Test.LuaParsing;

public class LuaIntellisensor
{
	private bool hasErrors;

	public ScriptResult Execute(string text, bool debug = false)
	{
		hasErrors = false;
		Stopwatch stopwatch = new Stopwatch();
		Scanner scanner = new Scanner(text, Error);
		stopwatch.Start();
		List<Token> list = scanner.ScanSourceForTokens();
		Parser parser = new Parser(list);
		List<Statement> list2 = parser.Parse();
		stopwatch.Stop();
		ScriptFinalizerVisitor scriptFinalizerVisitor = new ScriptFinalizerVisitor();
		List<ParsingError> list3 = new List<ParsingError>();
		int num = 0;
		foreach (Statement item in list2)
		{
			if (item is ExpressionStatement expressionStatement && expressionStatement.Expression is CallExpression)
			{
				list3.Add(new ParsingError("Endstar does not support calling functions within Global Scope.", 0, 0)
				{
					GlobalError = true
				});
			}
			item.Accept(scriptFinalizerVisitor);
			num++;
		}
		ScriptResult result = scriptFinalizerVisitor.Result;
		result.Tokens.AddRange(list);
		if (debug)
		{
			UnityEngine.Debug.Log($"Token Count: {result.Tokens.Count}");
			UnityEngine.Debug.Log($"Declared Variables Count: {result.DeclaredVariables.Count}");
			foreach (string declaredVariable in result.DeclaredVariables)
			{
				UnityEngine.Debug.Log("Declared Variable: " + declaredVariable);
			}
			UnityEngine.Debug.Log($"Declared Free Function Count: {result.DeclaredVariables.Count}");
			foreach (string declaredFreeFunction in result.DeclaredFreeFunctions)
			{
				UnityEngine.Debug.Log("Declared Free Function Name: " + declaredFreeFunction);
			}
			UnityEngine.Debug.Log($"Declared Table Function Count: {result.TableFunctions.Count}");
			foreach (KeyValuePair<string, List<string>> tableFunction in result.TableFunctions)
			{
				UnityEngine.Debug.Log("Table Name: " + tableFunction.Key);
				foreach (string item2 in tableFunction.Value)
				{
					UnityEngine.Debug.Log("Table Function Name: " + item2);
				}
			}
			UnityEngine.Debug.Log($"Variable to Type Count: {result.VariableNameToType.Count}");
			foreach (KeyValuePair<string, string> item3 in result.VariableNameToType)
			{
				UnityEngine.Debug.Log("Variable Name: " + item3.Key + ", Type Name: " + item3.Value);
			}
		}
		result.Errors.AddRange(list3);
		result.Errors.AddRange(parser.ErrorMessages);
		return result;
	}

	public void Error(int line, string message)
	{
		Error(line, "", message);
	}

	public void Error(int line, string where, string message)
	{
		hasErrors = true;
		UnityEngine.Debug.Log($"[line {line}] Error - {where}: {message}");
	}
}
