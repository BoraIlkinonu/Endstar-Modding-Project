using System.Collections.Generic;
using System.Linq;

namespace Endless.Creator.Test.LuaParsing;

public class ScriptResult
{
	public List<Token> Tokens { get; } = new List<Token>();

	public List<string> DeclaredVariables { get; } = new List<string>();

	public List<string> DeclaredFreeFunctions { get; } = new List<string>();

	public Dictionary<string, List<string>> TableFunctions { get; } = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> Properties { get; } = new Dictionary<string, List<string>>();

	public Dictionary<string, string> VariableNameToType { get; } = new Dictionary<string, string>();

	public HashSet<string> Types { get; } = new HashSet<string>();

	public List<ParsingError> Errors { get; set; } = new List<ParsingError>();

	public ScriptResult()
	{
		Types = ScriptResultHelper.GetDefaultRegisteredTypes();
		TableFunctions = ScriptResultHelper.GetDefaultTableFunctions();
		VariableNameToType = ScriptResultHelper.GetDefaultVariableToTypes();
		Properties = ScriptResultHelper.GetDefaultProperties();
		TableFunctions.Add("API", new List<string> { "Log", "Print" });
		string[] defaultDeclaredVariables = ScriptResultHelper.DefaultDeclaredVariables;
		foreach (string item in defaultDeclaredVariables)
		{
			DeclaredVariables.Add(item);
		}
	}

	public void AddTableFunction(Token typeName, Token functionName)
	{
		if (!TableFunctions.ContainsKey(typeName.Lexeme))
		{
			TableFunctions.Add(typeName.Lexeme, new List<string>());
		}
		TableFunctions[typeName.Lexeme].Add(functionName.Lexeme);
	}

	public void AddTableFunction(string typeName, string functionName)
	{
		if (!TableFunctions.ContainsKey(typeName))
		{
			TableFunctions.Add(typeName, new List<string>());
		}
		TableFunctions[typeName].Add(functionName);
	}

	public List<string> GetAllCallablesByVariable(string name)
	{
		List<string> functionsCallableByVariable = GetFunctionsCallableByVariable(name);
		List<string> propertiesCallableByVariable = GetPropertiesCallableByVariable(name);
		return functionsCallableByVariable.Concat(propertiesCallableByVariable).Distinct().ToList();
	}

	public List<string> GetFunctionsCallableByVariable(string name)
	{
		if (VariableNameToType.TryGetValue(name, out var value) && TableFunctions.TryGetValue(value, out var value2))
		{
			return value2;
		}
		return new List<string>();
	}

	public List<string> GetPropertiesCallableByVariable(string name)
	{
		if (VariableNameToType.TryGetValue(name, out var value) && Properties.TryGetValue(value, out var value2))
		{
			return value2;
		}
		return new List<string>();
	}

	public void AddProperty(string typeName, string propertyName)
	{
		if (!Properties.ContainsKey(typeName))
		{
			Properties.Add(typeName, new List<string>());
		}
		Properties[typeName].Add(propertyName);
	}

	public void AddVariableAndType(string variableName, string type)
	{
		if (Types.Contains(type))
		{
			if (!DeclaredVariables.Contains(variableName))
			{
				DeclaredVariables.Add(variableName);
			}
			if (VariableNameToType.ContainsKey(variableName))
			{
				VariableNameToType[variableName] = type;
			}
			else
			{
				VariableNameToType.Add(variableName, type);
			}
		}
	}

	public void AddType(string type)
	{
		Types.Add(type);
	}
}
