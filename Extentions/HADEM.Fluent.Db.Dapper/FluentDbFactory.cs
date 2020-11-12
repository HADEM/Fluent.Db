// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Dapper
{
    using System.Data;
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Fluent db engine class, used to create <see cref="IFluentDbCommand"/>.
    /// </summary>
    public class FluentDbFactory : IFluentDbFactory
    {
        private readonly IDbConnection dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbFactory"/> class.
        /// </summary>
        /// <param name="dbConnection">The <see cref="IDbConnection"/> to use.</param>
        public FluentDbFactory(IDbConnection dbConnection) => this.dbConnection = dbConnection;

        /// <inheritdoc />
        public IFluentDbCommand CreateDbCommand()
        {
            FluentDbCommand command = new FluentDbCommand(this.dbConnection);
            return command;
        }
    }
}
