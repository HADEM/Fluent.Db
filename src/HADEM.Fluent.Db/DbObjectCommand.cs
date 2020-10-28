// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System.Collections.Generic;

    /// <summary>
    /// Represent a command to be executed on the specified <see cref="IDbConnection"/>.
    /// </summary>
    public class DbObjectCommand
    {
        /// <summary>
        /// Gets or Sets the sql script to execute.
        /// </summary>
        public string? ScriptSql { get; set; }

        /// <summary>
        /// Gets or Sets the parameters associated with the sql script.
        /// </summary>
        public IEnumerable<object> Parameters { get; set; }
    }
}
