// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using HADEM.Fluent.Db.Exception;
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Fluent Db Command base.
    /// </summary>
    public class FluentDbCommandBase
    {
        /// <summary>
        /// Execute any actions of the <see cref="IFluentDbCommand"/>, with the retry policy.
        /// </summary>
        /// <param name="dbActions">Actions to be executed.</param>
        /// <param name="retry">The <see cref="RetryPolicyOption"/>.</param>
        /// <param name="throwException">Define if the exception should be throwned.</param>
        /// <returns>The <see cref="DbCommandResult"/>.</returns>
        public static async Task<DbCommandResult> SafeExecuteDbCommandAction(Func<Task<DbCommandResult>> dbActions, RetryPolicyOption? retry, bool throwException = false)
        {
            int retries = 1;
            bool isExecuted = false;

            Stopwatch stopwatch = new Stopwatch();

            if (retry != null && retry.MaxRetries >= 1)
            {
                retries = retry.MaxRetries;
            }
            else
            {
                // Create the default retry policy
                retry = RetryPolicyOption.DefaultRetryPolicy();
                retries = retry.MaxRetries;
            }

            DbCommandResult? result = null;
            stopwatch.Start();

            Stack<System.Exception> exceptions = new Stack<System.Exception>();

            // Execute the actions inside the retry policy
            while (!isExecuted && retries > 0)
            {
                try
                {
                    result = await dbActions();

                    // Mark that actions have been executed.
                    isExecuted = true;
                }
                catch (System.Exception e)
                {
                    // Cheick if the exception is concerned by the retry policy
                    if (retry != null && retry.Exceptions!.Contains(e.GetType()))
                    {
                        // decrease the retries count;
                        retries -= 1;
                        isExecuted = false;
                    }
                    else
                    {
                        // No retry
                        isExecuted = true;

                        // push the underlying exception
                        exceptions.Push(e);
                    }
                }
            }

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            if (result == null)
            {
                result = new DbCommandResult();
                result.IsSuccess = false;
                result.Result = -1;
                result.Exception = new AggregateException("One or more exception(s) occured", exceptions.ToArray());
            }

            // We passed the max retries.
            if (!isExecuted || retries <= 0)
            {
                result.IsSuccess = false;
                result.Result = -1;
                result.Exception = new RetryReachedException(
                    "Number of retry reached",
                    result.Exception ?? new System.Exception("Internal error occured"));
            }

            if (throwException && !result.IsSuccess && result.Exception != null)
            {
                throw result.Exception;
            }

            return result;
        }

        /// <summary>
        /// Cheick if the <see cref="DbObjectCommand{T}"/> parameters are not null.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand{T}"/>.</param>
        public void CheickDbObjectCommand<T>(IEnumerable<DbObjectCommand<T>> commands)
            where T : class
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

        /// <summary>
        /// Cheick if the <see cref="DbObjectCommand{T}"/> parameter is not null.
        /// </summary>
        /// <param name="commands">The param to be checked.</param>
        public void CheickDbObjectCommand<T>(DbObjectCommand<T> commands)
            where T : class
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

        /// <summary>
        /// Cheick if the <see cref="DbObjectCommand"/> parameter is not null.
        /// </summary>
        /// <param name="command">The param to be checked.</param>
        public void CheickDbObjectCommand(DbObjectCommand command)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.ScriptSql))
            {
                throw new ArgumentNullException(nameof(command));
            }
        }

        /// <summary>
        /// Cheick if the <see cref="DbObjectCommand"/> parameters are not null.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand"/>.</param>
        public void CheickDbObjectCommand(IEnumerable<DbObjectCommand> commands)
        {
            if (commands == null || !commands.Any() || commands.Any(c => string.IsNullOrWhiteSpace(c.ScriptSql)))
            {
                throw new ArgumentNullException(nameof(commands));
            }
        }
    }
}
