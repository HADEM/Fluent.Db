// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Fluent database command interface.
    /// </summary>
    public interface IFluentDbCommand
    {
        /// <summary>
        /// Cheick the database connection, before execute the command.
        /// </summary>
        /// <returns>True or False.</returns>
        bool CheickDatabaseConnection();

        /// <summary>
        /// Specify whether the command should be executed within a <see cref="IDbTransaction"/>.
        /// </summary>
        IFluentDbCommand WithTransaction();

        /// <summary>
        /// Indicate the Retry policy.
        /// </summary>
        /// <param name="retryPolicy">The <see cref="RetryPolicyOption"/>.</param>
        IFluentDbCommand WithRetry(RetryPolicyOption retryPolicy);

        /// <summary>
        /// Execute asynchronously a SQL script in a safe way and return a <see cref="DbCommandResult"/>.
        /// </summary>
        /// <param name="sql">The SQL script to execute.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync(string sql, Action dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a database command object in a safe way and return a <see cref="DbCommandResult"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbObjectCommand"/> to execute.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync(DbObjectCommand command, Action? dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a generic database command object in a safe way and return a <see cref="DbCommandResult"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbObjectCommand{T}"/> to execute.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync<T>(DbObjectCommand<T> command, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new();

        /// <summary>
        /// Execute asynchronously a list of database commands object in a safe way and return a <see cref="DbCommandResult"/>.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand"/> to execute.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync(IEnumerable<DbObjectCommand> commands, Action? dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a list of generic database commands object in a safe way and return a <see cref="DbCommandResult"/>.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand{T}"/> to execute.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync<T>(IEnumerable<DbObjectCommand<T>> commands, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new();

        /// <summary>
        /// Execute asynchronously a database command in a safe way and return a <see cref="DbCommandResult"/>.
        /// This method is used to execute a list of many generics commands provide by the inner commands queue.
        /// <code>
        /// var result = await fluendDbEngine.WithTransaction().AddInsert(insert).AddUpdate(update).ExecuteAsync();
        /// </code>
        /// </summary>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<DbCommandResult> ExecuteAsync(Action? dispatcherPostExecution = null, bool throwException = false);

        #region With Custom return

        /// <summary>
        /// Execute asynchronously a SQL script in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// </summary>
        /// <param name="sql">The SQL script to execute.</param>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn>(string sql, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a database command object in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// </summary>
        /// <param name="command">The <see cref="DbObjectCommand"/> to execute.</param>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn>(DbObjectCommand command, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a generic database command object in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// </summary>
        /// <param name="command">The <see cref="DbObjectCommand{T}"/> to execute.</param>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn, T>(DbObjectCommand<T> command, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new();

        /// <summary>
        /// Execute asynchronously a list of database commands object in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand"/> to execute.</param>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn>(IEnumerable<DbObjectCommand> commands, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false);

        /// <summary>
        /// Execute asynchronously a list of generic database commands object in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// </summary>
        /// <param name="commands">The list of <see cref="DbObjectCommand{T}"/> to execute.</param>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn, T>(IEnumerable<DbObjectCommand<T>> commands, Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false)
            where T : class, new();

        /// <summary>
        /// Execute asynchronously a database command in a safe way and return a <see cref="DbCommandResult"/>
        /// and return a <typeparamref name="TReturn"/> value.
        /// This method is used to execute a list of many generics commands provide by the inner commands queue.
        /// <code>
        /// var result = await fluendDbEngine.WithTransaction().AddInsert(insert).AddUpdate(update).ExecuteAsync();
        /// </code>
        /// </summary>
        /// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
        /// <param name="dispatcherPostExecution">Optional post action to execute.</param>
        /// <param name="throwException">When TRUE, rethrows the exception.</param>
        /// <returns>The <see cref="DbCommandResult"/> with the information about the execution.</returns>
        Task<TReturn> ExecuteAsync<TReturn>(Func<TReturn> customValueProviderToExecute, Action? dispatcherPostExecution = null, bool throwException = false);

        #endregion

        /// <summary>
        /// Create a <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Insert"/>.
        /// </summary>
        /// <param name="data">The data to insert.</param>
        IFluentDbCommand AddInsertCommand<T>(T data)
            where T : class, new();

        /// <summary>
        /// Create a list of <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Insert"/>.
        /// </summary>
        /// <param name="datas">The list of <typeparamref name="T"/> data to insert.</param>
        IFluentDbCommand AddInsertCommand<T>(IEnumerable<T> datas)
            where T : class, new();

        /// <summary>
        /// Create a <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Update"/>.
        /// </summary>
        /// <param name="data">The data to insert.</param>
        IFluentDbCommand AddUpdateCommand<T>(T data)
            where T : class, new();

        /// <summary>
        /// Create a list of <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Update"/>.
        /// </summary>
        /// <param name="datas">The list of <typeparamref name="T"/> data to update.</param>
        IFluentDbCommand AddUpdateCommand<T>(IEnumerable<T> datas)
            where T : class, new();

        /// <summary>
        /// Create a <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Delete"/>.
        /// </summary>
        /// <param name="data">The data to delete.</param>
        IFluentDbCommand AddDeleteCommand<T>(T data)
            where T : class, new();

        /// <summary>
        /// Create a list of <see cref="DbObjectCommand{T}"/> with a <see cref="DbOperation.Insert"/>.
        /// </summary>
        /// <param name="datas">The data to insert.</param>
        IFluentDbCommand AddDeleteCommand<T>(IEnumerable<T> datas)
            where T : class, new();

        /// <summary>
        /// Create a <see cref="DbObjectCommand"/> to execute.
        /// </summary>
        /// <param name="sql">The sql script to execute.</param>
        IFluentDbCommand AddCustomCommand(string sql);
    }
}
