// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Dapper
{
    using System.Data;
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Fluent db engine class, used to create <see cref="IFluentDbCommand"/>.
    /// </summary>
    public class FluentDbEngine : IFluentDbEngine
    {
        private readonly IDbConnection dbConnection;

        public FluentDbEngine(IDbConnection dbConnection) => this.dbConnection = dbConnection;

        /// <inheritdoc />
        public IFluentDbCommand CreateDbCommand()
        {
            FluentDbCommand command = new FluentDbCommand(this.dbConnection);
            return command;
        }
    }
}
