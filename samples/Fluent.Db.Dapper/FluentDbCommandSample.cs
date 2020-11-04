using Dapper;
using Fluent.Db.Dapper.Model;
using HADEM.Fluent.Db;
using HADEM.Fluent.Db.Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Fluent.Db.Dapper
{
    public class FluentDbCommandSample
    {
        private readonly IDbConnection dbConnection;
        private FluentDbEngine dbEngine;

        public FluentDbCommandSample()
        {
            this.dbConnection = new SqlConnection("Server=127.0.0.1,1402;Database=SampleDb;User=sa;Password=!#123Pwd@!");
            this.dbEngine = new FluentDbEngine(dbConnection);
        }

        public async Task CleanData()
        {
            // Clean data
            this.dbEngine = new FluentDbEngine(dbConnection);
            await this.dbConnection.ExecuteAsync("delete from dbo.Employee");
            await this.dbConnection.ExecuteAsync("delete from dbo.Project");
            await this.dbConnection.ExecuteAsync("delete from dbo.Department");
        }

        public async Task InsertDataByObjectCommand()
        {
            this.dbEngine = new FluentDbEngine(dbConnection);
            Stopwatch stopwatch = new Stopwatch();

            // Insert by using an DbObjectCommand
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

            Console.WriteLine("await dbEngine.CreateDbCommand().ExecuteAsync with a DbObjectCommand");
            stopwatch.Start();
            var result = await dbEngine.CreateDbCommand().ExecuteAsync(insertEmpCommand);
            stopwatch.Stop();
            Console.WriteLine($"Result : {result.IsSuccess} | Time Elapsed : {stopwatch.Elapsed}");

            stopwatch.Reset();
            Console.WriteLine("---------------------------------------------------------------------------");

            // Insert with generic DbObjectCommand and return a custom value
            Func<string> returnFunction = () => "Everything is ok";
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

            Console.WriteLine("await dbEngine.CreateDbCommand().ExecuteAsync with a Generic DbObjectCommand and return a custom value");
            stopwatch.Start();
            string returnValue = await dbEngine.CreateDbCommand().ExecuteAsync<string, Project>(
                insertPrjCommand,
                customValueProviderToExecute: returnFunction);
            stopwatch.Stop();
            Console.WriteLine($"Result returned : {returnValue} | Time Elapsed : {stopwatch.Elapsed}");

            stopwatch.Reset();
            Console.WriteLine("---------------------------------------------------------------------------");
        }

        public async Task ExecuteManyCommandWithTransaction()
        {
            this.dbEngine = new FluentDbEngine(dbConnection);
            Stopwatch stopwatch = new Stopwatch();

            // Insert many data

            // employe data
            var newEmp = new Employee()
            {
                FirstName = "FirstName1222",
                LastName = "LastName1222",
                Salary = 160000
            };

            // project data
            var project = new Project()
            {
                AssigneeId = 1,
                Description = "Project 2 | Fluent Db Sample",
                Budget = 250000,
                Name = "Show FluentDb library"
            };

            // Department
            var department = new Department()
            {
                Manager = "Employe FirstName1222 LastName1222",
                Name = "Department 1222"
            };

            Console.WriteLine("Execute many insert in one step");
            stopwatch.Start();
            var result = await this.dbEngine.CreateDbCommand().WithTransaction()
                .AddInsertCommand<Employee>(newEmp)
                .AddInsertCommand<Project>(project)
                .AddInsertCommand<Department>(department)
                .ExecuteAsync(dispatcherPostExecution: null, throwException: true);
            stopwatch.Stop();
            Console.WriteLine($"Result : {result.IsSuccess} | Time Elapsed : {stopwatch.Elapsed}");

            stopwatch.Reset();
            Console.WriteLine("---------------------------------------------------------------------------");
        }

        public async Task ExecuteWithDispatcher(ILogger logger)
        {
            this.dbEngine = new FluentDbEngine(dbConnection);
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Execute dispatcher inside the command");

            var project = new Project()
            {
                AssigneeId = 1,
                Description = "Project 3 | Fluent Db Sample",
                Budget = 260000,
                Name = "Show FluentDb library 3"
            };
            var insertPrjCommand = new DbObjectCommand<Project>()
            {
                ObjectParameter = project,
                Operation = DbOperation.Insert,
            };

            stopwatch.Start();
            var result = await this.dbEngine.CreateDbCommand().ExecuteAsync<Project>(
                command: insertPrjCommand,
                dispatcherPostExecution: () => logger?.LogInformation("dispatcher execute"),
                throwException: false);
            Console.WriteLine($"Result : {result.IsSuccess} | Time Elapsed : {stopwatch.Elapsed}");

            stopwatch.Reset();
            Console.WriteLine("---------------------------------------------------------------------------");

            Console.WriteLine("Execute dispatcher inside many command execution");
            
            Project prj = await this.dbConnection.QueryFirstAsync<Project>("select * from dbo.Project where Name = 'Show FluentDb library 3'");

            // Update data
            prj.Budget = 340000;

            // Apply update and insert new data

            // employe data
            var newEmp = new Employee()
            {
                FirstName = "FirstName133",
                LastName = "LastName133",
                Salary = 200000
            };

            Console.WriteLine("Execute many insert in one step");
            stopwatch.Start();
            var result2 = await this.dbEngine.CreateDbCommand().WithTransaction()
                .AddInsertCommand<Employee>(newEmp)
                .AddUpdateCommand<Project>(prj)
                .AddCustomCommand("delete from dbo.Employee where FirstName = 'FirstName111'")
                .ExecuteAsync(
                    dispatcherPostExecution: () => logger?.LogInformation("Dispatcher executed"),
                    throwException: true);
            stopwatch.Stop();
            Console.WriteLine($"Result : {result.IsSuccess} | Time Elapsed : {stopwatch.Elapsed}");

            stopwatch.Reset();
            Console.WriteLine("---------------------------------------------------------------------------");
        }
    }
}
