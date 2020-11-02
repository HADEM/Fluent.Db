// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Test.Core
{
    using System.Data;
    using System.Data.SQLite;

    public class FluentDbTestCore
    {
        public IDbConnection GetDbConnection()
        {
            var sqlLite = new SQLiteConnection("Data Source=:memory:");
            //SQLiteConnection.CreateFile("fakeDbTest.db3");

            //return new SQLiteConnection("fakeDbTest.db3");
            return sqlLite;
        }
    }
}
