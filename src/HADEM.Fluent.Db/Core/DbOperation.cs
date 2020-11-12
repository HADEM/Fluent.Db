// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System.Data;

    /// <summary>
    /// Enumeration that specify the operation to be executed on the <see cref="IDbConnection"/>.
    /// </summary>
    public enum DbOperation : uint
    {
        /// <summary>
        /// Insert operation
        /// </summary>
        Insert,

        /// <summary>
        /// Update operation
        /// </summary>
        Update,

        /// <summary>
        /// Delete operation
        /// </summary>
        Delete,

        /// <summary>
        /// Execute a specific SQL script (Stored Procedure, Function, etc.)
        /// </summary>
        ExecuteSql,
    }
}
