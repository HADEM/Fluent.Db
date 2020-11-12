// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    /// <summary>
    /// Represent the result of a <see cref="DbObjectCommand{T}"/> or <see cref="DbObjectCommand"/>
    /// executed on the database.
    /// </summary>
    public class DbCommandResult
    {
        /// <summary>
        /// Gets or Sets the number of rows impacted by the command.
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or Sets whether the command has successfully be executed.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or Sets the underlying exception that occured during the command execution.
        /// </summary>
        public System.Exception? Exception { get; set; }
    }
}
