// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Test.Core
{
    using System.Data.SqlClient;
    using Dapper.Contrib.Extensions;

    [Table("FakeTable")]
    public class FakeTableClass
    {
        [Key]
        public long FakeId { get; set; }

        public string FakeName { get; set; }

        public string FakeDescription { get; set; }

        public static void CreateTableInDb(SqlConnection sqlConnection)
        {
            try
            {
                using (sqlConnection)
                {
                    sqlConnection.Open();

                    string createSql = @"
                        CREATE TABLE [dbo].[FakeTable](
	                        [FakeId] [bigint] IDENTITY(1,1) NOT NULL,
	                        [FakeName] [varchar](250) NOT NULL,
	                        [FakeDescription] [varchar](250) NOT NULL
                         CONSTRAINT [PK_FakeTableId] PRIMARY KEY CLUSTERED  ([FakeId] ASC))";
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = createSql;
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();

                    sqlConnection.Close();
                }
            }
            catch
            {
            }
        }

        public static void CleanTableInDb(SqlConnection sqlConnection)
        {
            try
            {
                using (sqlConnection)
                {
                    string createSql = " DROP TABLE [dbo].[FakeTable]";
                    SqlCommand command = sqlConnection.CreateCommand();
                    command.CommandText = createSql;
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }
    }
}
