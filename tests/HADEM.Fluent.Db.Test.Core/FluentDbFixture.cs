// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Test.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Data.SQLite;
    using Dapper;

    public class FluentDbFixture : DisposableObject
    {
        public const string FixtureName = "FluentDb.Test.Fixture";

        private IDbConnection dbConnection;
        private string connectionString;

        public FluentDbFixture()
        {
            // Set a connectionstring (in memory database
            this.connectionString = "Server=127.0.0.1,1402;Database=FluentDbTest;User=sa;Password=!#123Pwd@!";
            this.dbConnection = new SqlConnection(this.connectionString);
        }

        public IDbConnection DbConnectionProvider => new SqlConnection(this.connectionString);

        public override void DoDispose()
        {
            this.dbConnection = new SqlConnection(this.connectionString);

            // Clean every data from the application Db
            try
            {
                using (this.dbConnection)
                {
                    this.dbConnection.Open();

                    IEnumerable<string> tables;
                    string sql = @"select CONCAT(s.name,'.',t.name) as tableName 
                                    from sys.tables t 
                                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id";
                    tables = this.dbConnection.Query<string>(sql);
                    foreach (string table in tables)
                    {
                        this.dbConnection.Execute($"DELETE FROM {table}");
                    }

                    // Delete the FakeClass Table if create
                    this.dbConnection.Execute("DROP TABLE [FakeTable]");

                    this.dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Cannot clean the Unittest DataBase. {ex.Message}");
            }

            this.dbConnection = null;
        }

        public void CleanData(string table)
        {
            try
            {
                this.dbConnection = new SqlConnection(this.connectionString);
                using (this.dbConnection)
                {
                    this.dbConnection.Open();
                    this.dbConnection.Execute($"delete from {table}");

                    this.dbConnection.Close();
                }
            }
            catch
            {
                // Table may not exist
            }
        }
    }
}
