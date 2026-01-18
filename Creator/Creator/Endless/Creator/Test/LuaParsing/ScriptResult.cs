using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x0200031E RID: 798
	public class ScriptResult
	{
		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000E9F RID: 3743 RVA: 0x00045BD4 File Offset: 0x00043DD4
		public List<Token> Tokens { get; } = new List<Token>();

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000EA0 RID: 3744 RVA: 0x00045BDC File Offset: 0x00043DDC
		public List<string> DeclaredVariables { get; } = new List<string>();

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000EA1 RID: 3745 RVA: 0x00045BE4 File Offset: 0x00043DE4
		public List<string> DeclaredFreeFunctions { get; } = new List<string>();

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000EA2 RID: 3746 RVA: 0x00045BEC File Offset: 0x00043DEC
		public Dictionary<string, List<string>> TableFunctions { get; } = new Dictionary<string, List<string>>();

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000EA3 RID: 3747 RVA: 0x00045BF4 File Offset: 0x00043DF4
		public Dictionary<string, List<string>> Properties { get; } = new Dictionary<string, List<string>>();

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000EA4 RID: 3748 RVA: 0x00045BFC File Offset: 0x00043DFC
		public Dictionary<string, string> VariableNameToType { get; } = new Dictionary<string, string>();

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000EA5 RID: 3749 RVA: 0x00045C04 File Offset: 0x00043E04
		public HashSet<string> Types { get; } = new HashSet<string>();

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000EA6 RID: 3750 RVA: 0x00045C0C File Offset: 0x00043E0C
		// (set) Token: 0x06000EA7 RID: 3751 RVA: 0x00045C14 File Offset: 0x00043E14
		public List<ParsingError> Errors { get; set; } = new List<ParsingError>();

		// Token: 0x06000EA8 RID: 3752 RVA: 0x00045C20 File Offset: 0x00043E20
		public ScriptResult()
		{
			this.Types = ScriptResultHelper.GetDefaultRegisteredTypes();
			this.TableFunctions = ScriptResultHelper.GetDefaultTableFunctions();
			this.VariableNameToType = ScriptResultHelper.GetDefaultVariableToTypes();
			this.Properties = ScriptResultHelper.GetDefaultProperties();
			this.TableFunctions.Add("API", new List<string> { "Log", "Print" });
			foreach (string text in ScriptResultHelper.DefaultDeclaredVariables)
			{
				this.DeclaredVariables.Add(text);
			}
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x00045D08 File Offset: 0x00043F08
		public void AddTableFunction(Token typeName, Token functionName)
		{
			if (!this.TableFunctions.ContainsKey(typeName.Lexeme))
			{
				this.TableFunctions.Add(typeName.Lexeme, new List<string>());
			}
			this.TableFunctions[typeName.Lexeme].Add(functionName.Lexeme);
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x00045D5A File Offset: 0x00043F5A
		public void AddTableFunction(string typeName, string functionName)
		{
			if (!this.TableFunctions.ContainsKey(typeName))
			{
				this.TableFunctions.Add(typeName, new List<string>());
			}
			this.TableFunctions[typeName].Add(functionName);
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x00045D90 File Offset: 0x00043F90
		public List<string> GetAllCallablesByVariable(string name)
		{
			IEnumerable<string> functionsCallableByVariable = this.GetFunctionsCallableByVariable(name);
			List<string> propertiesCallableByVariable = this.GetPropertiesCallableByVariable(name);
			return functionsCallableByVariable.Concat(propertiesCallableByVariable).Distinct<string>().ToList<string>();
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x00045DBC File Offset: 0x00043FBC
		public List<string> GetFunctionsCallableByVariable(string name)
		{
			string text;
			List<string> list;
			if (this.VariableNameToType.TryGetValue(name, out text) && this.TableFunctions.TryGetValue(text, out list))
			{
				return list;
			}
			return new List<string>();
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x00045DF0 File Offset: 0x00043FF0
		public List<string> GetPropertiesCallableByVariable(string name)
		{
			string text;
			List<string> list;
			if (this.VariableNameToType.TryGetValue(name, out text) && this.Properties.TryGetValue(text, out list))
			{
				return list;
			}
			return new List<string>();
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x00045E24 File Offset: 0x00044024
		public void AddProperty(string typeName, string propertyName)
		{
			if (!this.Properties.ContainsKey(typeName))
			{
				this.Properties.Add(typeName, new List<string>());
			}
			this.Properties[typeName].Add(propertyName);
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x00045E58 File Offset: 0x00044058
		public void AddVariableAndType(string variableName, string type)
		{
			if (!this.Types.Contains(type))
			{
				return;
			}
			if (!this.DeclaredVariables.Contains(variableName))
			{
				this.DeclaredVariables.Add(variableName);
			}
			if (this.VariableNameToType.ContainsKey(variableName))
			{
				this.VariableNameToType[variableName] = type;
				return;
			}
			this.VariableNameToType.Add(variableName, type);
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x00045EB7 File Offset: 0x000440B7
		public void AddType(string type)
		{
			this.Types.Add(type);
		}
	}
}
