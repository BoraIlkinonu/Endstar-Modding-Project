using System;
using System.Data;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;

namespace SQLiter
{
	// Token: 0x02000004 RID: 4
	public class Example : MonoBehaviour
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000020BE File Offset: 0x000002BE
		private void Awake()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Awake ---");
			}
			Example._sqlDBLocation = "URI=file:SpellingWords.db";
			Debug.Log(Example._sqlDBLocation);
			Example.Instance = this;
			this.SQLiteInit();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020F2 File Offset: 0x000002F2
		private void Start()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Start ---");
			}
			base.Invoke("Test", 3f);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002118 File Offset: 0x00000318
		private void Test()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Test Invoked ---");
			}
			LoomManager.Loom.QueueOnMainThread(delegate
			{
				this.GetAllWords();
			});
			this.InsertWord("Dogma", "A pack of lovely dogs");
			this.InsertWord("Firehose", "A hose that is on fire");
			this.InsertWord("Tree", "Binary Search Algorithm");
			this.InsertWord("Cake", "Always a lie");
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000218D File Offset: 0x0000038D
		private void OnDestroy()
		{
			this.SQLiteClose();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002195 File Offset: 0x00000395
		public void RunAsyncInit()
		{
			LoomManager.Loom.QueueOnMainThread(delegate
			{
				this.SQLiteInit();
			});
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000021B0 File Offset: 0x000003B0
		private void SQLiteInit()
		{
			Debug.Log("SQLiter - Opening SQLite Connection at " + Example._sqlDBLocation);
			this._connection = new SqliteConnection(Example._sqlDBLocation);
			this._command = this._connection.CreateCommand();
			this._connection.Open();
			this._command.CommandText = "PRAGMA journal_mode = WAL;";
			this._command.ExecuteNonQuery();
			this._command.CommandText = "PRAGMA journal_mode";
			this._reader = this._command.ExecuteReader();
			if (this.DebugMode && this._reader.Read())
			{
				Debug.Log("SQLiter - WAL value is: " + this._reader.GetString(0));
			}
			this._reader.Close();
			this._command.CommandText = "PRAGMA synchronous = OFF";
			this._command.ExecuteNonQuery();
			this._command.CommandText = "PRAGMA synchronous";
			this._reader = this._command.ExecuteReader();
			if (this.DebugMode && this._reader.Read())
			{
				Debug.Log("SQLiter - synchronous value is: " + this._reader.GetInt32(0).ToString());
			}
			this._reader.Close();
			this._command.CommandText = "SELECT name FROM sqlite_master WHERE name='Definitions'";
			this._reader = this._command.ExecuteReader();
			if (!this._reader.Read())
			{
				Debug.Log("SQLiter - Could not find SQLite table Definitions");
				this._createNewTable = true;
			}
			this._reader.Close();
			if (this._createNewTable)
			{
				Debug.Log("SQLiter - Dropping old SQLite table if Exists: Definitions");
				this._command.CommandText = "DROP TABLE IF EXISTS Definitions";
				this._command.ExecuteNonQuery();
				Debug.Log("SQLiter - Creating new SQLite table: Definitions");
				this._sqlString = "CREATE TABLE IF NOT EXISTS Definitions (Word TEXT UNIQUE, Definition TEXT)";
				this._command.CommandText = this._sqlString;
				this._command.ExecuteNonQuery();
			}
			else if (this.DebugMode)
			{
				Debug.Log("SQLiter - SQLite table Definitions was found");
			}
			this._connection.Close();
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000023C0 File Offset: 0x000005C0
		public void InsertWord(string name, string definition)
		{
			name = name.ToLower();
			this._sqlString = string.Concat(new string[] { "INSERT OR REPLACE INTO Definitions (Word,Definition) VALUES ('", name, "','", definition, "');" });
			if (this.DebugMode)
			{
				Debug.Log(this._sqlString);
			}
			this.ExecuteNonQuery(this._sqlString);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002428 File Offset: 0x00000628
		public void GetAllWords()
		{
			StringBuilder stringBuilder = new StringBuilder();
			this._connection.Open();
			this._command.CommandText = "SELECT * FROM Definitions";
			this._reader = this._command.ExecuteReader();
			while (this._reader.Read())
			{
				stringBuilder.Length = 0;
				stringBuilder.Append(this._reader.GetString(0)).Append(" ");
				stringBuilder.Append(this._reader.GetString(1)).Append(" ");
				stringBuilder.AppendLine();
				if (this.DebugMode)
				{
					Debug.Log(stringBuilder.ToString());
				}
			}
			this._reader.Close();
			this._connection.Close();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000024E7 File Offset: 0x000006E7
		public string GetDefinitation(string value)
		{
			return this.QueryString("Definition", value);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000024F8 File Offset: 0x000006F8
		public string QueryString(string column, string value)
		{
			string text = "Not Found";
			this._connection.Open();
			this._command.CommandText = string.Concat(new string[] { "SELECT ", column, " FROM Definitions WHERE Word='", value, "'" });
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				text = this._reader.GetString(0);
			}
			else
			{
				Debug.Log("QueryString - nothing to read...");
			}
			this._reader.Close();
			this._connection.Close();
			return text;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000259C File Offset: 0x0000079C
		public void SetValue(string column, int value, string wordKey)
		{
			this.ExecuteNonQuery(string.Concat(new string[]
			{
				"UPDATE OR REPLACE Definitions SET ",
				column,
				"='",
				value.ToString(),
				"' WHERE Word='",
				wordKey,
				"'"
			}));
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000025EC File Offset: 0x000007EC
		public void DeleteWord(string wordKey)
		{
			this.ExecuteNonQuery("DELETE FROM Definitions WHERE Word='" + wordKey + "'");
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002604 File Offset: 0x00000804
		public void ExecuteNonQuery(string commandText)
		{
			this._connection.Open();
			this._command.CommandText = commandText;
			this._command.ExecuteNonQuery();
			this._connection.Close();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002634 File Offset: 0x00000834
		private void SQLiteClose()
		{
			if (this._reader != null && !this._reader.IsClosed)
			{
				this._reader.Close();
			}
			this._reader = null;
			if (this._command != null)
			{
				this._command.Dispose();
			}
			this._command = null;
			if (this._connection != null && this._connection.State != ConnectionState.Closed)
			{
				this._connection.Close();
			}
			this._connection = null;
		}

		// Token: 0x04000006 RID: 6
		public static Example Instance = null;

		// Token: 0x04000007 RID: 7
		public bool DebugMode;

		// Token: 0x04000008 RID: 8
		private static string _sqlDBLocation = "";

		// Token: 0x04000009 RID: 9
		private const string SQL_DB_NAME = "SpellingWords";

		// Token: 0x0400000A RID: 10
		private const string SQL_TABLE_NAME = "Definitions";

		// Token: 0x0400000B RID: 11
		private const string COL_WORD = "Word";

		// Token: 0x0400000C RID: 12
		private const string COL_DEFINITION = "Definition";

		// Token: 0x0400000D RID: 13
		private IDbConnection _connection;

		// Token: 0x0400000E RID: 14
		private IDbCommand _command;

		// Token: 0x0400000F RID: 15
		private IDataReader _reader;

		// Token: 0x04000010 RID: 16
		private string _sqlString;

		// Token: 0x04000011 RID: 17
		public bool _createNewTable;
	}
}
