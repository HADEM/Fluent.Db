# Fluent.Db 

![FluentDb - Build Workflow](https://github.com/HADEM/Fluent.Db/workflows/FluentDb%20-%20Build%20Workflow/badge.svg?branch=main)

HADEM.Fluent.Db provides a fluent abstraction layer for execute database command, using an existing ORM (like Dapper) 

Following ORM are currently supported:
- Dapper

## Nugets
- HADEM.Fluent.Db: [![NuGet version](https://badge.fury.io/nu/HADEM.Fluent.Db.svg)](https://badge.fury.io/nu/HADEM.Fluent.Db)
- HADEM.Fluent.Db.Dapper: [![NuGet version](https://badge.fury.io/nu/HADEM.Fluent.Db.Dapper.svg)](https://badge.fury.io/nu/HADEM.Fluent.Db.Dapper)

## Why use HADEM.Fluent.Db
Using an ORM is now our main focus when we are developing a software.
ORM helps us avoid write SQL statement to interact with the database for common operation like (INSERT, UPDATE, DELETE, SELECT, etc.). <b>HADEM.Fluent.Db</b> provides a fluent abstraction to execute `INSERT`, `UPDATE`, `DELETE`, `SELECT` with your favorite ORM.

### How to use HADEM.Fluent.Db
```csharp
// We create a FluentDbEngine object. It's keep track of the IDbConnection to use
IDbConnection dbConnection = new SqlConnection("Server=<YourServer>;Database=<YourDatabase>;User=<userId>;Password=<Password>");
var fluentDb = new FluentDbEngine(dbConnection);

// Use the FluentDbEngine to create a FluentDbCommand with DbObjectCommand (represent the command to be executed)
var newEmp = new Employee()
{
    FirstName = "FirstName111",
    LastName = "LastName111",
    Salary = 120000
};

var insertEmpCommand = new DbObjectCommand()
{
    Parameters = new List<object>() { newEmp },
    ScriptSql = "INSERT INTO[dbo].[Employee](FirstName, LastName, Salary) Values(@FirstName, @LastName, @Salary)"
};
var result = await fluentDb.CreateDbCommand().ExecuteAsync(insertEmpCommand);

```

### Use a generic command with HADEM.Fluent.Db
```csharp     
// Use the FluentDbEngine to create a FluentDbCommand with a generic DbObjectCommand (represent the command to be executed)
var project = new Project()
{
    AssigneeId = 1,
    Description = "Project | Fluent Db Sample",
    Budget = 250000,
    Name = "Show FluentDb library"
};

var insertPrjCommand = new DbObjectCommand<Project>()
{
    ObjectParameter = project,
    Operation = DbOperation.Insert,
};

var result = await fluentDb.CreateDbCommand().ExecuteAsync<Project>(insertPrjCommand);
```

### What is behind the `ExecuteAsync` Method

The method `ExecuteAsync` is executed inside a `try/catch` and give to you any information about the command execute in a `DbCommandResult` object

```csharp
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
        public Exception Exception { get; set; }
    }
}
```
You can specify either:
- throwing the underlying exception when it occurs
- Execute any custom action after the command is executed (example logging)
- Return any custom value, by giving a `Func<TReturn>` parameter.

```csharp
/// <summary>
/// Execute asynchronously a generic database command object in a safe way.
/// </summary>
/// <param name="command">The command to execute.</param>
/// <param name="dispatcherPostExecution">Optional post action to execute.</param>
/// <param name="throwException">When TRUE, rethrows the exception.</param>
/// <returns>The result with the information about the execution.</returns>
Task<DbCommandResult> ExecuteAsync<T>(DbObjectCommand<T> command, Action dispatcherPostExecution = null, bool throwException = false)
    where T : class, new();

/// <summary>
/// Execute asynchronously a generic database command object in a safe way and return a custom value.
/// </summary>
/// <param name="command">The generic command to execute.</param>
/// <param name="customValueProviderToExecute">Function to execute in order to return the <typeparamref name="TReturn"/> value.</param>
/// <param name="dispatcherPostExecution">Optional post action to execute.</param>
/// <param name="throwException">When TRUE, rethrows the exception.</param>
/// <returns>The custom return value.</returns>
Task<TReturn> ExecuteAsync<TReturn, T>(DbObjectCommand<T> command, Func<TReturn> customValueProviderToExecute, Action dispatcherPostExecution = null, bool throwException = false)
            where T : class, new();
```

### Execute multiple command (Insert/Update/Delete) inside `ExecuteAsync`
You can execute multiple command in one step like follow :
```csharp

var result = await this.dbEngine.CreateDbCommand().WithTransaction()
                .AddInsertCommand<Employee>(emp)
                .AddUpdateCommand<Project>(prj)
                .AddCustomCommand("delete from dbo.Employee where FirstName = 'FirstName111'")
                .AddDeleteCommand<Department>(dept)
                .ExecuteAsync(dispatcherPostExecution: () => logger?.LogInformation("logging !"), throwException: true);

```


### More Examples
Please have a look in the example folder: 
- [Fluent.Db samples](./samples)

#### Step to use the database for the test
1. Install [Docker](https://www.docker.com/)
2. Download the fluentdb SqlServer image and run the container 
```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<YourPassword>" -p 1402:1433 --name fluentDb -h fluentDb -d hadem/fluentdb-mssql-linux:fluentDbMsSqlLinux
```
3. Changes the password in the [FluentDbFixture](https://github.com/HADEM/Fluent.Db/blob/main/tests/HADEM.Fluent.Db.Test.Core/FluentDbFixture.cs) 



