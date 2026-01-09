using Microsoft.Data.Sqlite;
using System.IO;
using System.Reflection;
using System.Windows.Documents;
using Utility.Lib.BitBucketRepositories;

namespace Utility.SQL
{
    public class SQLLib : BaseUtility
    {
        private readonly string _dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\test.db");
        private readonly SqliteConnection? sqliteConnection;
        object _lock = new object();

        public const string ProjectTable = "ProjectTable";
        public const string RepositoryTable = "RepositoryTable";
        public const string BranchTable = "BranchTable";
        public const string CommitTable = "CommitTable";
        public const string MainPatchTable = "MainPatchTable";
        public const string UnitPatchTable = "UnitPatchTable";


        List<string> sqlCreateTables = new List<string>()
        {
            "CREATE TABLE IF NOT EXISTS ProjectTable (\r\n    " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n  " +
            "key TEXT NOT NULL UNIQUE,\r\n  " +
            "name TEXT NOT NULL,\r\n    " +
            "description TEXT NOT NULL,\r\n    " +
            "type TEXT NOT NULL\r\n    "+
            ");",

            "PRAGMA foreign_keys = ON;\r\n  " +
            "CREATE TABLE IF NOT EXISTS RepositoryTable (\r\n " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n  " +
            "slug TEXT NOT NULL UNIQUE,\r\n " +
            "name TEXT NOT NULL,\r\n    " +
            "project_id INTEGER NOT NULL,\r\n   " +
            "CONSTRAINT fk_RepositoryTable_project_id FOREIGN KEY(project_id)\r\n   " +
            "REFERENCES ProjectTable(id)\r\n    " +
            "ON DELETE CASCADE\r\n  " +
            "ON UPDATE CASCADE\r\n  " +
            ");",

            "PRAGMA foreign_keys = ON;\r\n  " +
            "CREATE TABLE IF NOT EXISTS BranchTable (\r\n " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n  " +
            "branchid TEXT NOT NULL UNIQUE,\r\n " +
            "displayId TEXT NOT NULL UNIQUE,\r\n    " +
            "latestCommit TEXT NOT NULL UNIQUE,\r\n " +
            "isDefault INTEGER NOT NULL CHECK (isDefault IN (0,1)),\r\n " +
            "repository_id INTEGER NOT NULL,\r\n    " +
            "CONSTRAINT fk_BranchTable_repositoryid FOREIGN KEY(repository_id)\r\n  " +
            "REFERENCES RepositoryTable(id)\r\n " +
            "ON DELETE CASCADE\r\n  " +
            "ON UPDATE CASCADE\r\n  " +
            ");",

            "PRAGMA foreign_keys = ON;\r\n  " +
            "CREATE TABLE IF NOT EXISTS CommitTable (\r\n " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n  " +
            "commitid TEXT NOT NULL UNIQUE,\r\n " +
            "displayId TEXT NOT NULL UNIQUE,\r\n    " +
            "latestCommit TEXT NOT NULL UNIQUE,\r\n " +
            "branch_id INTEGER NOT NULL,\r\n    " +
            "CONSTRAINT fk_CommitTable_branchid FOREIGN KEY(branch_id)\r\n  " +
            "REFERENCES BranchTable(id)\r\n " +
            "ON DELETE CASCADE\r\n  " +
            "ON UPDATE CASCADE\r\n  " +
            ");",

            "CREATE TABLE IF NOT EXISTS MainPatchTable (\r\n    " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n    " +
            "patchname TEXT NOT NULL UNIQUE\r\n    " +
            ");",

            "PRAGMA foreign_keys = ON;\r\n  " +
            "CREATE TABLE IF NOT EXISTS UnitPatchTable (\r\n    " +
            "id INTEGER PRIMARY KEY AUTOINCREMENT,\r\n    " +
            "unitpatchname TEXT NOT NULL UNIQUE,\r\n    " +
            "mainpatch_id INTEGER NOT NULL,\r\n " +
            "CONSTRAINT fk_CommitTable_patchid FOREIGN KEY(mainpatch_id)\r\n    " +
            "REFERENCES MainPatchTable(id)\r\n  " +
            "ON DELETE CASCADE\r\n  " +
            "ON UPDATE CASCADE\r\n  " +
            ");"
        };

        public void CreateTables()
        {
            try
            {
                lock (this)
                {
                    using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
                    {
                        connection.Open();
                        using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                        {
                            pragmaCmd.ExecuteNonQuery();
                        }
                        foreach (var sql in sqlCreateTables)
                        {
                            using var command = new SqliteCommand(sql, connection);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        private void OpenCnSetPragmaForeignKeysOn(SqliteConnection connection)
        {
            connection.Open();
            using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                pragmaCmd.ExecuteNonQuery();
            }
        }

        private void DeleteRowById(SqliteConnection connection, string tablename, int id)
        {
            try
            {
                lock (this)
                {
                    string deleteSql = $"DELETE FROM {tablename} WHERE id = @id;";

                    using (var command = new SqliteCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            // No row found with the given id
                            // Handle accordingly (e.g., throw exception or log)
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        private void DeleteAllRows(SqliteConnection connection, string table)
        {
            try
            {
                lock (this)
                {
                    string deleteSql = $"DELETE FROM {table};";

                    using (var command = new SqliteCommand(deleteSql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        // rowsAffected is the number of rows deleted
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }

        }
        private void DeleteRowsById(SqliteConnection connection, string tablename, int mainpatchId)
        {
            try
            {
                lock (this)
                {
                    string deleteSql = $"DELETE FROM {tablename} WHERE mainpatch_id = @mainpatch_id;";

                    using (var command = new SqliteCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@mainpatch_id", mainpatchId);
                        int rowsDeleted = command.ExecuteNonQuery();

                        // Optionally, handle rowsDeleted (e.g., log or confirm deletion)
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        private void Insert<T>(string tablename, List<T> source) where T : BaseUtility
        {
            try
            {
                lock (this)
                {
                    var type = typeof(T);
                    var properties = type.GetProperties();
                    var column = string.Empty;
                    var values = string.Empty;
                    List<PropertyInfo> entries = new List<PropertyInfo>();
                    foreach (var property in properties)
                    {
                        entries.Add(property);
                        column += $"{property.Name},";
                        values += $"@{property.Name},";
                    }
                    column = column.TrimEnd(',');
                    values = values.TrimEnd(',');
                    using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
                    {
                        OpenCnSetPragmaForeignKeysOn(connection);
                        string insertSql = $"INSERT OR IGNORE INTO {tablename} ({column}) VALUES ({column});";
                        using (var command = new SqliteCommand(insertSql, connection))
                        {
                            foreach (var entry in entries)
                            {
                                command.Parameters.AddWithValue($"@{entry.Name}", entry.GetValue(source));
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        private void Insert<T>(string tablename, T source) where T : BaseUtility
        {
            try
            {
                lock (this)
                {
                    var type = typeof(T);
                    var properties = type.GetProperties();
                    var column = string.Empty;
                    var values = string.Empty;
                    List<PropertyInfo> entries = new List<PropertyInfo>();
                    foreach (var property in properties)
                    {
                        entries.Add(property);
                        column += $"{property.Name},";
                        values += $"@{property.Name},";
                    }
                    column = column.TrimEnd(',');
                    values = values.TrimEnd(',');
                    using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
                    {
                        OpenCnSetPragmaForeignKeysOn(connection);
                        string insertSql = $"INSERT OR IGNORE INTO {tablename} ({column}) VALUES ({values});";
                        using (var command = new SqliteCommand(insertSql, connection))
                        {
                            foreach (var entry in entries)
                            {
                                command.Parameters.AddWithValue($"@{entry.Name}", entry.GetValue(source));
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        private void Insert(SqliteConnection connection, string tablename, string column, string source)
        {
            try
            {
                lock (this)
                {
                    string insertSql = $"INSERT OR IGNORE INTO {tablename} ({column}) VALUES (@{column});";
                    using (var command = new SqliteCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue($"@{column}", source);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }


        private int? GetIdByColumnandValue(SqliteConnection connection, string tablename, string column, string value)
        {
            try
            {
                lock (this)
                {
                    string query = $"SELECT id FROM {tablename} WHERE {column} = @{column} LIMIT 1;";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue($"@{column}", value);

                        var result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int id))
                        {
                            return id;
                        }
                        else
                        {
                            return null; // Not found
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
            return null;
        }
        private string? GetValueByTableColumnandId(SqliteConnection connection, string tablename, string column, int id)
        {
            try
            {
                lock (this)
                {
                    string query = $"SELECT {column} FROM {tablename} WHERE id = @id LIMIT 1;";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        var result = command.ExecuteScalar();

                        return result?.ToString(); // returns null if not found
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
            return null;
        }
        private int? GetOrInsertValue(string tablename, string column, string value)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException($"{column} cannot be null or empty");
            using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
            {
                OpenCnSetPragmaForeignKeysOn(connection);
                Insert(connection, tablename, column, value);
                return GetIdByColumnandValue(connection, tablename, column, value);
            }
        }
        public void AddProject(Project source)
        {
            Insert(ProjectTable, source);
        }
        public void AddProject(List<Project> listsource)
        {
            Insert(ProjectTable, listsource);
        }
        public void AddMainPatch(string source)
        {
            using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
            {
                connection.Open();
                using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    pragmaCmd.ExecuteNonQuery();
                }
                // Insert or ignore (won't insert if value exists)
                Insert(connection, MainPatchTable, "patchname", source);
            }

        }




        private void InsertOrIgnoreUnitPatch(SqliteConnection connection, string unitpatchname, int mainpatchId)
        {
            try
            {
                lock (this)
                {
                    string insertSql = @"
                        INSERT OR IGNORE INTO UnitPatchTable (unitpatchname, mainpatch_id) 
                        VALUES (@unitpatchname, @mainpatch_id);
                        ";

                    using (var command = new SqliteCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@unitpatchname", unitpatchname);
                        command.Parameters.AddWithValue("@mainpatch_id", mainpatchId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                CatchAndPromptErr(e);
            }
        }
        public void AddUnitPatch(string mainpatchname, string unitpatchname)
        {
            using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
            {
                OpenCnSetPragmaForeignKeysOn(connection);
                var id = GetOrInsertValue(MainPatchTable, "patchname", mainpatchname);
                if (id != null)
                    InsertOrIgnoreUnitPatch(connection, unitpatchname, (int)id);
            }
        }
        public void AddUnitPatch(string mainpatchname, List<string> unitpatchname)
        {
            using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
            {
                OpenCnSetPragmaForeignKeysOn(connection);
                var id = GetOrInsertValue(MainPatchTable, "patchname", mainpatchname);
                if (id != null)
                {
                    foreach (var patchname in unitpatchname)
                        InsertOrIgnoreUnitPatch(connection, patchname, (int)id);
                }
            }
        }
        public void ClearandAddUnitPatch(string mainpatchname, List<string> unitpatchname)
        {
            using (var connection = new SqliteConnection($"Data Source={_dbpath}"))
            {
                OpenCnSetPragmaForeignKeysOn(connection);
                var id = GetOrInsertValue(MainPatchTable, "patchname", mainpatchname);
                if (id != null)
                {
                    DeleteRowsById(connection, UnitPatchTable, (int)id);
                    foreach (var patchname in unitpatchname)
                        InsertOrIgnoreUnitPatch(connection, patchname, (int)id);
                }
            }
        }
    }
}
