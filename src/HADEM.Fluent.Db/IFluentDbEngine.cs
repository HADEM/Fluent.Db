// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Fluent Db engine interface used to create <see cref="Interfaces.IFluentDbCommand"/> command to execute.
    /// Each ORM provider should inherit from this interface and should be name as FluentDbEngine.
    /// </summary>
    public interface IFluentDbEngine
    {
        /// <summary>
        /// Create a <see cref="IFluentDbCommand"/> object to execute in the safe way any database operation.
        /// </summary>
        /// <returns>A <see cref="IFluentDbCommand"/> object.</returns>
        IFluentDbCommand CreateDbCommand();
    }
}
