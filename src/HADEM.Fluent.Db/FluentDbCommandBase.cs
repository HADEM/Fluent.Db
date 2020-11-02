// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Fluent Db Command base.
    /// </summary>
    public class FluentDbCommandBase
    {
        /// <summary>
        /// Cheick if the <see cref="DbObjectCommand{T}"/> parameters are not null.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand{T}"/>.</param>
        public void CheickDbObjectCommand<T>(IEnumerable<DbObjectCommand<T>> commands)
        {
            if (commands == null || commands.Any(o => o.ObjectParameter == null))
            {
                throw new ArgumentNullException(nameof(commands));
            }
            else if (commands.Any(o => o.Operation == DbOperation.ExecuteSql && string.IsNullOrWhiteSpace(o.ScriptSql)))
            {
                throw new ArgumentNullException(nameof(commands));
            }
        }

        public void CheickDbObjectCommand<T>(DbObjectCommand<T> commands)
        {
            if (commands == null || commands.ObjectParameter == null)
            {
                throw new ArgumentNullException(nameof(commands));
            }
            else if (commands.Operation == DbOperation.ExecuteSql && string.IsNullOrWhiteSpace(commands.ScriptSql))
            {
                throw new ArgumentNullException(nameof(commands));
            }
        }

        public void CheickDbObjectCommand(DbObjectCommand command)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.ScriptSql))
            {
                throw new ArgumentNullException(nameof(command));
            }
        }

        public void CheickDbObjectCommand(IEnumerable<DbObjectCommand> commands)
        {
            if (commands == null || !commands.Any() || commands.Any(c => string.IsNullOrWhiteSpace(c.ScriptSql)))
            {
                throw new ArgumentNullException(nameof(commands));
            }
        }
    }
}
