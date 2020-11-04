// Copyright (c) HADEM. All rights reserved.

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("HADEM.Fluent.Db.Dapper.Test")]

namespace HADEM.Fluent.Db.Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Dapper;
    using global::Dapper.Contrib.Extensions;
    using HADEM.Fluent.Db.Core;
    using HADEM.Fluent.Db.Interfaces;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Internal field, used in test project")]

    /// <summary>
    /// Fluent database command.
    /// </summary>
    public sealed class FluentDbCommand : FluentDbCommandBase, IFluentDbCommand
    {
        /// <summary>
        /// Queue list to store each <see cref="DbObjectCommand"/> execution.
        /// </summary>
        internal Queue<Func<Task<DbCommandResult>>> ActionsToExecute;
        internal RetryPolicyOption RetryPolicy;
        internal IDbTransaction Transaction;

        private readonly IDbConnection dbConnection;

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
        public async Task<DbCommandResult> ExecuteAsync(string sql, Action dispatcherPostExecution = null, bool throwException = false)
        {
            DbCommandResult result = new DbCommandResult();
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            this.EnsureDbConnection();

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

                if (throwException)
                {
                    throw;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(DbObjectCommand command, Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            return await this.ExecuteDbObjectCommand(command, dispatcherPostExecution, throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(IEnumerable<DbObjectCommand> commands, Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            bool commitException = false;

            DbCommandResultCollection results = new DbCommandResultCollection();
            foreach (DbObjectCommand command in commands)
            {
                results.Add(await this.ExecuteDbObjectCommand(command));
            }

            DbCommandResult mergeResult = results.MergeResults();

            try
            {
                this.EnsureCommit();
            }
            catch (Exception)
            {
                this.EnsureRollBack();
                commitException = true;
            }

            dispatcherPostExecution?.Invoke();

            if (throwException && (!mergeResult.IsSuccess || commitException))
            {
                throw mergeResult.Exception;
            }

            return mergeResult;
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync<T>(DbObjectCommand<T> command, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(command);

            return await this.ExecuteDbObjectCommand<T>(command, dispatcherPostExecution, throwException);
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync<T>(IEnumerable<DbObjectCommand<T>> commands, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            bool commitException = false;

            DbCommandResultCollection results = new DbCommandResultCollection();
            foreach (DbObjectCommand<T> command in commands)
            {
                results.Add(await this.ExecuteDbObjectCommand<T>(command));
            }

            DbCommandResult mergeResult = results.MergeResults();

            try
            {
                this.EnsureCommit();
            }
            catch (Exception)
            {
                this.EnsureRollBack();
                commitException = true;
            }

            dispatcherPostExecution?.Invoke();

            if (throwException && (!mergeResult.IsSuccess || commitException))
            {
                throw mergeResult.Exception;
            }

            return mergeResult;
        }

        /// <inheritdoc />
        public async Task<DbCommandResult> ExecuteAsync(Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();

            DbCommandResultCollection results = new DbCommandResultCollection();

            // wait all tasks in the queue
            while (this.ActionsToExecute.Any())
            {
                Func<Task<DbCommandResult>> dbTask = this.ActionsToExecute.Dequeue();
                results.Add(await dbTask());
            }

            DbCommandResult mergeResult = results.MergeResults();

            dispatcherPostExecution?.Invoke();

            if (throwException && !mergeResult.IsSuccess)
            {
                throw mergeResult.Exception;
            }

            return mergeResult;
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(string sql, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
        {
            DbCommandResult result = new DbCommandResult();
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            this.EnsureDbConnection();

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

            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(DbObjectCommand command, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            await this.ExecuteDbObjectCommand(command, dispatcherPostExecution, throwException);

            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn, T>(DbObjectCommand<T> command, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(command);

            await this.ExecuteDbObjectCommand<T>(command, dispatcherPostExecution, throwException);

            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(IEnumerable<DbObjectCommand> commands, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            bool commitException = false;

            DbCommandResultCollection results = new DbCommandResultCollection();
            foreach (DbObjectCommand command in commands)
            {
                results.Add(await this.ExecuteDbObjectCommand(command));
            }

            DbCommandResult mergeResult = results.MergeResults();

            try
            {
                this.EnsureCommit();
            }
            catch (Exception)
            {
                this.EnsureRollBack();
                commitException = true;
            }

            dispatcherPostExecution?.Invoke();

            if (throwException && (!mergeResult.IsSuccess || commitException))
            {
                throw mergeResult.Exception;
            }

            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn, T>(IEnumerable<DbObjectCommand<T>> commands, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();
            this.CheickDbObjectCommand(commands);

            bool commitException = false;

            DbCommandResultCollection results = new DbCommandResultCollection();
            foreach (DbObjectCommand<T> command in commands)
            {
                results.Add(await this.ExecuteDbObjectCommand<T>(command));
            }

            DbCommandResult mergeResult = results.MergeResults();

            try
            {
                this.EnsureCommit();
            }
            catch (Exception)
            {
                this.EnsureRollBack();
                commitException = true;
            }

            dispatcherPostExecution?.Invoke();

            if (throwException && (!mergeResult.IsSuccess || commitException))
            {
                throw mergeResult.Exception;
            }

            return customValueProviderToExecute();
        }

        /// <inheritdoc />
        public async Task<TReturn> ExecuteAsync<TReturn>(Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
        {
            this.EnsureDbConnection();
            this.EnsureDbConnectionIsOpened();

            DbCommandResultCollection results = new DbCommandResultCollection();

            // wait all tasks in the queue
            while (this.ActionsToExecute.Any())
            {
                Func<Task<DbCommandResult>> dbTask = this.ActionsToExecute.Dequeue();
                results.Add(await dbTask());
            }

            DbCommandResult mergeResult = results.MergeResults();

            dispatcherPostExecution?.Invoke();

            if (throwException && !mergeResult.IsSuccess)
            {
                throw mergeResult.Exception;
            }

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

                // after the commit we need to set the transaction to null. To avoid
                // future command execution with no needed transaction
                this.Transaction = null;
            }
        }

        private void EnsureRollBack()
        {
            if (this.Transaction != null && !this.ActionsToExecute.Any())
            {
                this.Transaction.Rollback();
            }
        }

        // DbObjectCommand execute
        private async Task<DbCommandResult> ExecuteDbObjectCommand(DbObjectCommand command, Action dispatcherPostExecution = null, bool throwException = false)
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

                if (throwException)
                {
                    throw;
                }
            }

            return result;
        }

        // DbObjectCommand execute
        private async Task<DbCommandResult> ExecuteDbObjectCommand<T>(DbObjectCommand<T> command, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new()
        {
            DbCommandResult result = new DbCommandResult();

            try
            {
                this.CheickDbObjectCommand(command);
                bool operation = command.Operation switch
                {
                    DbOperation.Delete => await this.dbConnection.DeleteAsync<T>(command.ObjectParameter, transaction: this.Transaction),
                    DbOperation.Insert => await this.dbConnection.InsertAsync<T>(command.ObjectParameter, transaction: this.Transaction) > 0,
                    DbOperation.Update => await this.dbConnection.UpdateAsync<T>(command.ObjectParameter, transaction: this.Transaction),
                    DbOperation.ExecuteSql => await this.ExecuteScriptAsync<T>(command.ScriptSql, command.ObjectParameter),
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

                if (throwException)
                {
                    throw;
                }
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

        #endregion
    }
}
