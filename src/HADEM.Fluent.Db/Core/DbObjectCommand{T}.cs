// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    /// <summary>
    /// Represent a database command with a object parameter.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    public class DbObjectCommand<T>
    {
        /// <summary>
        /// Gets or Sets the object parameter concerned by the <see cref="DbObjectCommand{T}"/> command.
        /// </summary>
        public T ObjectParameter { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="DbOperation"/> to be executed.
        /// </summary>
        public DbOperation Operation { get; set; }

        /// <summary>
        /// Gets or Sets the optional SQL script to be executed.
        /// </summary>
        public string? ScriptSql { get; set; }
    }
}
