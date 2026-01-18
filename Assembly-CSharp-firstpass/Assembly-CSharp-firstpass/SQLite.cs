using System;
using System.Data;
using System.Text;
using Mono.Data.Sqlite;
using UnityEngine;

namespace SQLiter
{
	// Token: 0x02000009 RID: 9
	public class SQLite : MonoBehaviour
	{
		// Token: 0x06000021 RID: 33 RVA: 0x000027E7 File Offset: 0x000009E7
		private void Awake()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Awake ---");
			}
			SQLite._sqlDBLocation = "URI=file:PlayersSQLite.db";
			Debug.Log(SQLite._sqlDBLocation);
			SQLite.Instance = this;
			this.SQLiteInit();
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000281B File Offset: 0x00000A1B
		private void Start()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Start ---");
			}
			base.Invoke("Test", 3f);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002840 File Offset: 0x00000A40
		private void Test()
		{
			if (this.DebugMode)
			{
				Debug.Log("--- Test Invoked ---");
			}
			LoomManager.Loom.QueueOnMainThread(delegate
			{
				this.GetAllPlayers();
			});
			this.InsertPlayer("Alex", 3, 2, 3, 2, 4, 12);
			this.InsertPlayer("Bob", 3, 2, 3, 2, 4, 12);
			this.InsertPlayer("Frank", 3, 2, 3, 2, 4, 12);
			this.InsertPlayer("Joe", 3, 2, 3, 2, 4, 12);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000028BD File Offset: 0x00000ABD
		private void OnDestroy()
		{
			this.SQLiteClose();
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000028C5 File Offset: 0x00000AC5
		public void RunAsyncInit()
		{
			LoomManager.Loom.QueueOnMainThread(delegate
			{
				this.SQLiteInit();
			});
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000028E0 File Offset: 0x00000AE0
		private void SQLiteInit()
		{
			Debug.Log("SQLiter - Opening SQLite Connection");
			this._connection = new SqliteConnection(SQLite._sqlDBLocation);
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
			this._command.CommandText = "SELECT name FROM sqlite_master WHERE name='GenericTableName'";
			this._reader = this._command.ExecuteReader();
			if (!this._reader.Read())
			{
				Debug.Log("SQLiter - Could not find SQLite table GenericTableName");
				this._createNewTavle = true;
			}
			this._reader.Close();
			if (this._createNewTavle)
			{
				Debug.Log("SQLiter - Creating new SQLite table GenericTableName");
				this._command.CommandText = "DROP TABLE IF EXISTS GenericTableName";
				this._command.ExecuteNonQuery();
				this._sqlString = "CREATE TABLE IF NOT EXISTS GenericTableName (name TEXT UNIQUE, race INTEGER, class INTEGER, gold INTEGER, login INTEGER, level INTEGER, xp INTEGER)";
				this._command.CommandText = this._sqlString;
				this._command.ExecuteNonQuery();
			}
			else if (this.DebugMode)
			{
				Debug.Log("SQLiter - SQLite table GenericTableName was found");
			}
			this._connection.Close();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002ADC File Offset: 0x00000CDC
		public void InsertPlayer(string name, int raceType, int classType, int gold, int login, int level, int xp)
		{
			name = name.ToLower();
			this._sqlString = string.Concat(new string[]
			{
				"INSERT OR REPLACE INTO GenericTableName (name,race,class,gold,login,level,xp) VALUES ('",
				name,
				"',",
				raceType.ToString(),
				",",
				classType.ToString(),
				",",
				gold.ToString(),
				",",
				login.ToString(),
				",",
				level.ToString(),
				",",
				xp.ToString(),
				");"
			});
			if (this.DebugMode)
			{
				Debug.Log(this._sqlString);
			}
			this.ExecuteNonQuery(this._sqlString);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002BA8 File Offset: 0x00000DA8
		public void GetAllPlayers()
		{
			StringBuilder stringBuilder = new StringBuilder();
			this._connection.Open();
			this._command.CommandText = "SELECT * FROM GenericTableName";
			this._reader = this._command.ExecuteReader();
			while (this._reader.Read())
			{
				stringBuilder.Length = 0;
				stringBuilder.Append(this._reader.GetString(0)).Append(" ");
				stringBuilder.Append(this._reader.GetInt32(1)).Append(" ");
				stringBuilder.Append(this._reader.GetInt32(2)).Append(" ");
				stringBuilder.Append(this._reader.GetInt32(3)).Append(" ");
				stringBuilder.Append(this._reader.GetInt32(4)).Append(" ");
				stringBuilder.Append(this._reader.GetInt32(5)).Append(" ");
				stringBuilder.AppendLine();
				if (this.DebugMode)
				{
					Debug.Log(stringBuilder.ToString());
				}
			}
			this._reader.Close();
			this._connection.Close();
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002CE1 File Offset: 0x00000EE1
		public int GetRace(string value)
		{
			return this.QueryInt("race", value);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002CF0 File Offset: 0x00000EF0
		public string QueryString(string column, string value)
		{
			string text = "Not Found";
			this._connection.Open();
			this._command.CommandText = string.Concat(new string[] { "SELECT ", column, " FROM GenericTableName WHERE name='", value, "'" });
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

		// Token: 0x0600002B RID: 43 RVA: 0x00002D94 File Offset: 0x00000F94
		public int QueryInt(string column, string value)
		{
			int num = -1;
			this._connection.Open();
			this._command.CommandText = string.Concat(new string[] { "SELECT ", column, " FROM GenericTableName WHERE name='", value, "'" });
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				num = this._reader.GetInt32(0);
			}
			else
			{
				Debug.Log("QueryInt - nothing to read...");
			}
			this._reader.Close();
			this._connection.Close();
			return num;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002E34 File Offset: 0x00001034
		public short QueryShort(string column, string value)
		{
			short num = -1;
			this._connection.Open();
			this._command.CommandText = string.Concat(new string[] { "SELECT ", column, " FROM GenericTableName WHERE name='", value, "'" });
			this._reader = this._command.ExecuteReader();
			if (this._reader.Read())
			{
				num = this._reader.GetInt16(0);
			}
			else
			{
				Debug.Log("QueryShort - nothing to read...");
			}
			this._reader.Close();
			this._connection.Close();
			return num;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002ED4 File Offset: 0x000010D4
		public void SetValue(string column, int value, string name)
		{
			this.ExecuteNonQuery(string.Concat(new string[]
			{
				"UPDATE OR REPLACE GenericTableName SET ",
				column,
				"=",
				value.ToString(),
				" WHERE name='",
				name,
				"'"
			}));
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002F24 File Offset: 0x00001124
		public void DeletePlayer(string nameKey)
		{
			this.ExecuteNonQuery("DELETE FROM GenericTableName WHERE name='" + nameKey + "'");
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002F3C File Offset: 0x0000113C
		public void ExecuteNonQuery(string commandText)
		{
			this._connection.Open();
			this._command.CommandText = commandText;
			this._command.ExecuteNonQuery();
			this._connection.Close();
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002F6C File Offset: 0x0000116C
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

		// Token: 0x04000015 RID: 21
		public static SQLite Instance = null;

		// Token: 0x04000016 RID: 22
		public bool DebugMode;

		// Token: 0x04000017 RID: 23
		private static string _sqlDBLocation = "";

		// Token: 0x04000018 RID: 24
		private const string SQL_DB_NAME = "PlayersSQLite";

		// Token: 0x04000019 RID: 25
		private const string SQL_TABLE_NAME = "GenericTableName";

		// Token: 0x0400001A RID: 26
		private const string COL_NAME = "name";

		// Token: 0x0400001B RID: 27
		private const string COL_LOGIN_LAST = "login";

		// Token: 0x0400001C RID: 28
		private const string COL_RACE = "race";

		// Token: 0x0400001D RID: 29
		private const string COL_CLASS = "class";

		// Token: 0x0400001E RID: 30
		private const string COL_LEVEL = "level";

		// Token: 0x0400001F RID: 31
		private const string COL_XP = "xp";

		// Token: 0x04000020 RID: 32
		private const string COL_GOLD = "gold";

		// Token: 0x04000021 RID: 33
		private IDbConnection _connection;

		// Token: 0x04000022 RID: 34
		private IDbCommand _command;

		// Token: 0x04000023 RID: 35
		private IDataReader _reader;

		// Token: 0x04000024 RID: 36
		private string _sqlString;

		// Token: 0x04000025 RID: 37
		private bool _createNewTavle;
	}
}
