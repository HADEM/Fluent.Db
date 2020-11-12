// Copyright (c) HADEM. All rights reserved.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("HADEM.Fluent.Db.Dapper.Test")]

namespace HADEM.Fluent.Db.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Dapper;
    using global::Dapper.Contrib.Extensions;
    using HADEM.Fluent.Db.Core;
    using HADEM.Fluent.Db.Exception;
    using HADEM.Fluent.Db.Interfaces;

    /// <summary>
    /// Fluent database command.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Internal field, used in test project")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1124:Do not use regions", Justification = "For readability")]

    public sealed class FluentDbCommand : FluentDbCommandBase, IFluentDbCommand
    {
        /// <summary>
        /// Queue list to store each <see cref="DbObjectCommand"/> execution.
        /// </summary>
        internal Queue<Func<Task<DbCommandResult>>> ActionsToExecute;
        internal RetryPolicyOption? RetryPolicy;
        internal IDbTransaction? Transaction;

        /// <summary>
        /// Defined either we are in a multiple command execution or single.
        /// </summary>
        internal bool MultipleCommandExecution = false;

        private readonly IDbConnection dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentDbCommand"/> class.
        /// </summary>
        /// <param name="dbConnection">The <see cref="IDbConnection"/> to use.</param>
        public FluentDbCommand(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            this.ActionsToExecute = new Queue<Func<Task<DbCommandResult>>>();
        }

        /// <inheritdoc />
        public IFluentDbCommand AddCustomCommand(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            DbObjectCommand command = new DbObjectCommand();
            command.ScriptSql = sql;
            command.Parameters = null;

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(command);
            this.ActionsToExecute.Enqueue(execution);

            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddDeleteCommand<T>(T data)
            where T : class, new()
        {
            DbObjectCommand<T> command = new DbObjectCommand<T>();
            command.ObjectParameter = data;
            command.Operation = DbOperation.Delete;

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(command);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddDeleteCommand<T>(IEnumerable<T> datas)
            where T : class, new()
        {
            List<DbObjectCommand<T>> commands = new List<DbObjectCommand<T>>();
            foreach (T data in datas)
            {
                commands.Add(new DbObjectCommand<T>()
                {
                    Operation = DbOperation.Delete,
                    ObjectParameter = data,
                });
            }

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(commands);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddInsertCommand<T>(T data)
            where T : class, new()
        {
            DbObjectCommand<T> command = new DbObjectCommand<T>();
            command.ObjectParameter = data;
            command.Operation = DbOperation.Insert;

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(command);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddInsertCommand<T>(IEnumerable<T> datas)
            where T : class, new()
        {
            List<DbObjectCommand<T>> commands = new List<DbObjectCommand<T>>();
            foreach (T data in datas)
            {
                commands.Add(new DbObjectCommand<T>()
                {
                    Operation = DbOperation.Insert,
                    ObjectParameter = data,
                });
            }

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(commands);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddUpdateCommand<T>(T data)
            where T : class, new()
        {
            DbObjectCommand<T> command = new DbObjectCommand<T>();
            command.ObjectParameter = data;
            command.Operation = DbOperation.Update;

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(command);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand AddUpdateCommand<T>(IEnumerable<T> datas)
            where T : class, new()
        {
            List<DbObjectCommand<T>> commands = new List<DbObjectCommand<T>>();
            foreach (T data in datas)
            {
                commands.Add(new DbObjectCommand<T>()
                {
                    Operation = DbOperation.Update,
                    ObjectParameter = data,
                });
            }

            Func<Task<DbCommandResult>> execution = () => this.ExecuteAsync(commands);
            this.ActionsToExecute.Enqueue(execution);
            this.MultipleCommandExecution = true;
            return this;
        }

        /// <inheritdoc />
        public bool CheickDatabaseConnection()
        {
            try
            {
                this.EnsureDbConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(string sql, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            this.EnsureDbConnection();

            Func<Task<DbCommandResult>> toExecute = async () =>
            {
                DbCommandResult result = new DbCommandResult();
                try
                {
                    this.EnsureDbConnectionIsOpened();
                    result.Result = await this.dbConnection.ExecuteAsync(sql, transaction: this.Transaction);

                    result.IsSuccess = result.Result > 0;

                    dispatcherPostExecution?.Invoke();
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Result = -1;
                    result.IsSuccess = false;

                    dispatcherPostExecution?.Invoke();

                    this.ThrowExceptionIfNecessary(throwException, ex);
                }

                return result;
            };

            // Execute the command
            return await FluentDbCommandBase.SafeExecuteDbCommandAction(toExecute, this.RetryPolicy, throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(DbObjectCommand command, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();

            return await this.SafeExecute(() => this.ExecuteDbObjectCommand(command, dispatcherPostExecution, throwException), throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(IEnumerable<DbObjectCommand> commands, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            // Set the execution in a variable
            Func<Task<DbCommandResult>> execution = async () =>
            {
                bool commitException = false;

                DbCommandResultCollection results = new DbCommandResultCollection();
                foreach (DbObjectCommand command in commands)
                {
                    results.Add(await this.ExecuteDbObjectCommand(command));
                }

                DbCommandResult mergeResult = results.MergeResults();

                // We should not commit if there is any exception that occured
                var commit = this.CommitOrRollBackIfNecessary(mergeResult);
                commitException = !commit.Item1;

                dispatcherPostExecution?.Invoke();

                // Get the eventuals commit exception
                mergeResult.Exception = commit.Item2;
                if (throwException && (!mergeResult.IsSuccess || commitException))
                {
                    // throw the inner exception (Exception of the merge result or the commit exception)
                    throw commit.Item2!;
                }

                return mergeResult;
            };

            return await this.SafeExecute(execution, throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync<T>(DbObjectCommand<T> command, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(command);

            return await this.SafeExecute(() => this.ExecuteDbObjectCommand<T>(command, dispatcherPostExecution, throwException), throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync<T>(IEnumerable<DbObjectCommand<T>> commands, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            Func<Task<DbCommandResult>> execution = async () =>
            {
                bool commitException = false;

                DbCommandResultCollection results = new DbCommandResultCollection();
                foreach (DbObjectCommand<T> command in commands)
                {
                    results.Add(await this.ExecuteDbObjectCommand<T>(command));
                }

                DbCommandResult mergeResult = results.MergeResults();

                // We should not commit if there is any exception that occured
                var commit = this.CommitOrRollBackIfNecessary(mergeResult);
                commitException = !commit.Item1;

                dispatcherPostExecution?.Invoke();

                // Get the eventuals commit exception
                mergeResult.Exception = commit.Item2;
                if (throwException && (!mergeResult.IsSuccess || commitException))
                {
                    // throw the inner exception (Exception of the merge result or the commit exception)
                    throw commit.Item2!;
                }

                return mergeResult;
            };

            return await this.SafeExecute(execution, throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(Action? dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();

            Func<Task<DbCommandResult>> execution = async () =>
            {
                bool commitException = false;

                DbCommandResultCollection results = new DbCommandResultCollection();

                // wait all tasks in the queue
                while (this.ActionsToExecute.Any())
                {
                    Func<Task<DbCommandResult>> dbTask = this.ActionsToExecute.Dequeue();
                    results.Add(await dbTask());
                }

                DbCommandResult mergeResult = results.MergeResults();

                // We should not commit if there is any exception that occured
                var commit = this.CommitOrRollBackIfNecessary(mergeResult);
                commitException = !commit.Item1;

                dispatcherPostExecution?.Invoke();

                // Get the eventuals commit exception
                mergeResult.Exception = commit.Item2;
                if (throwException && (!mergeResult.IsSuccess || commitException))
                {
                    // throw the inner exception (Exception of the merge result or the commit exception)
                    throw commit.Item2!;
                }

                return mergeResult;
            };

            return await FluentDbCommandBase.SafeExecuteDbCommandAction(execution, this.RetryPolicy, throwException);
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(string sql, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            DbCommandResult result = new DbCommandResult();
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            this.EnsureDbConnection();

            Func<Task<DbCommandResult>> execution = async () =>
            {
                try
                {
                    this.EnsureDbConnectionIsOpened();
                    result.Result = await this.dbConnection.ExecuteAsync(sql, transaction: this.Transaction);

                    result.IsSuccess = true;

                    dispatcherPostExecution?.Invoke();
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    result.Result = -1;
                    result.IsSuccess = false;

                    dispatcherPostExecution?.Invoke();

                    if (throwException)
                    {
                        throw;
                    }
                }

                return result;
            };

            await FluentDbCommandBase.SafeExecuteDbCommandAction(execution, this.RetryPolicy, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(DbObjectCommand command, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            await this.ExecuteAsync(command, dispatcherPostExecution, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn, T>(DbObjectCommand<T> command, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            await this.ExecuteAsync(command, dispatcherPostExecution, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(IEnumerable<DbObjectCommand> commands, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            await this.ExecuteAsync(commands, dispatcherPostExecution, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn, T>(IEnumerable<DbObjectCommand<T>> commands, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            await this.ExecuteAsync(commands, dispatcherPostExecution, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            await this.ExecuteAsync(dispatcherPostExecution, throwException);
            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public IFluentDbCommand WithRetry(RetryPolicyOption retryPolicy)
        {
            this.RetryPolicy = retryPolicy;
            return this;
        }

        /// <inheritdoc />
        public IFluentDbCommand WithTransaction()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.EnsureClearTransaction();
            this.Transaction = this.dbConnection.BeginTransaction();
            return this;
        }

        #region Privates methods

        private void EnsureDbConnectionIsOpened()
        {
            if (this.dbConnection.State != ConnectionState.Open)
            {
                this.dbConnection.Open();
            }
        }

        private void EnsureClearTransaction()
        {
            if (this.Transaction != null)
            {
                this.Transaction = null;
            }
        }

        private void EnsureDbConnection()
        {
            if (this.dbConnection == null)
            {
                throw new ArgumentNullException(nameof(this.dbConnection));
            }
        }

        private void EnsureCommit()
        {
            if (this.Transaction != null && !this.ActionsToExecute.Any())
            {
                this.Transaction.Commit();

                // after the commit we need to set the transaction to null and close the existing. To avoid
                // future command execution with no needed transaction
                this.Transaction = null;
                this.dbConnection.Close();
            }
        }

        private void EnsureRollBack()
        {
            if (this.Transaction != null && !this.ActionsToExecute.Any())
            {
                this.Transaction.Rollback();

                // Close the connection.
                this.Transaction = null;
                this.dbConnection.Close();
            }
        }

        private Tuple<bool, Exception?> CommitOrRollBackIfNecessary(DbCommandResult result)
        {
            if (result.IsSuccess)
            {
                try
                {
                    this.EnsureCommit();
                    return new Tuple<bool, Exception?>(true, null);
                }
                catch (Exception ex)
                {
                    this.EnsureRollBack();
                    return new Tuple<bool, Exception?>(false, ex);
                }
            }

            // if the result is not success, return false and the underlying exception
            return new Tuple<bool, Exception?>(false, result.Exception);
        }

        // DbObjectCommand execute
        private async Task<DbCommandResult> ExecuteDbObjectCommand(DbObjectCommand command, Action? dispatcherPostExecution = null, bool throwException = false)
        {
            DbCommandResult result = new DbCommandResult();

            try
            {
                this.CheickDbObjectCommand(command);

                if (command.Parameters != null && command.Parameters.Any())
                {
                    result.Result = await this.dbConnection.ExecuteAsync(
                        command.ScriptSql,
                        command.Parameters,
                        transaction: this.Transaction);
                }
                else
                {
                    result.Result = await this.dbConnection.ExecuteAsync(command.ScriptSql, transaction: this.Transaction);
                }

                result.IsSuccess = result.Result > 0;
                dispatcherPostExecution?.Invoke();
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.Result = -1;
                result.IsSuccess = false;

                dispatcherPostExecution?.Invoke();

                this.ThrowExceptionIfNecessary(throwException, ex);
            }

            return result;
        }

        // DbObjectCommand execute
        private async Task<DbCommandResult> ExecuteDbObjectCommand<T>(DbObjectCommand<T> command, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            DbCommandResult result = new DbCommandResult();

            try
            {
                this.CheickDbObjectCommand(command);
                bool operation = command.Operation switch
                {
                    DbOperation.Delete => await this.dbConnection.DeleteAsync<T>(command.ObjectParameter!, transaction: this.Transaction),
                    DbOperation.Insert => await this.dbConnection.InsertAsync<T>(command.ObjectParameter!, transaction: this.Transaction) > 0,
                    DbOperation.Update => await this.dbConnection.UpdateAsync<T>(command.ObjectParameter!, transaction: this.Transaction),
                    DbOperation.ExecuteSql => await this.ExecuteScriptAsync<T>(command.ScriptSql!, command.ObjectParameter!),
                    _ => false
                };

                result.Result = operation ? 1 : 0;
                result.IsSuccess = operation;

                dispatcherPostExecution?.Invoke();
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.Result = -1;
                result.IsSuccess = false;

                dispatcherPostExecution?.Invoke();

                this.ThrowExceptionIfNecessary(throwException, ex);
            }

            if (!result.IsSuccess && result.Exception == null)
            {
                result.Exception = new FluentDbExecutionException("Data was not found");
            }

            return result;
        }

        private async Task<bool> ExecuteScriptAsync<T>(string sql, T param)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            if (param == null)
            {
                return await this.dbConnection.ExecuteAsync(sql, transaction: this.Transaction) != -1;
            }
            else
            {
                return await this.dbConnection.ExecuteAsync(sql, param, transaction: this.Transaction) != -1;
            }
        }

        /// <summary>
        /// Execute safely the func, either we are in a retry policy or in a multiple command context.
        /// </summary>
        /// <param name="execution">Function to execute.</param>
        /// <param name="throwException">Define if we should throw the exception that occured.</param>
        private async Task<DbCommandResult> SafeExecute(Func<Task<DbCommandResult>> execution, bool throwException)
        {
            if (!this.MultipleCommandExecution)
            {
                // Execute this in the Safe Execution (with retry policy)
                return await FluentDbCommandBase.SafeExecuteDbCommandAction(execution, this.RetryPolicy, throwException);
            }
            else
            {
                // it will be executed inside the SafeExecuteDbCommandAction
                return await execution();
            }
        }

        /// <summary>
        /// Specifiy if we should throw the exception.
        /// </summary>
        /// <param name="throwException">Throw Exception option defined by the user.</param>
        /// <param name="anyException">The exception throwed.</param>
        [DoesNotReturn]
        private void ThrowExceptionIfNecessary(bool throwException, System.Exception anyException)
        {
            if (throwException || (this.RetryPolicy != null && this.RetryPolicy.ShouldThrowExceptionForRetry(anyException)))
            {
                throw anyException;
            }
#pragma warning disable CS8763 // Une méthode marquée [DoesNotReturn] ne doit pas être retournée.
        }
#pragma warning restore CS8763 // Une méthode marquée [DoesNotReturn] ne doit pas être retournée.

        #endregion
    }
}
