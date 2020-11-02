// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Dapper
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using GenFu;
    using HADEM.Fluent.Db.Test.Core;
    using Moq;
    using Xunit;

    [Collection(FluentDbFixture.FixtureName)]
    public class FluentDbCommandTest : FluentDbTestCore
    {
        private readonly FluentDbFixture fluentDbFixture;

        private IDbConnection dbConnection;
        private FluentDbCommand fluentDbCommand;

        public FluentDbCommandTest(FluentDbFixture fixture)
        {
            this.fluentDbFixture = fixture;
            this.dbConnection = this.fluentDbFixture.DbConnectionProvider;
            FakeTableClass.CreateTableInDb(this.dbConnection as SqlConnection);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async void FulentDbCommand_Should_Not_Execute_Script_When_Sql_IsNullOrEmpty(string sql)
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.fluentDbCommand.ExecuteAsync(sql, null, false));
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.fluentDbCommand.ExecuteAsync<int>(sql, () => 0, null, false));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.AddCustomCommand(sql));

            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
        }

        [Fact]
        public void FluentDbCommand_Add_AnyCommand_Should_Enqueue_FuncExecution()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);

            // Act
            this.fluentDbCommand.AddCustomCommand("INSERT FAKE");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.ListOf<FakeTableClass>(5));
            this.fluentDbCommand.AddUpdateCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddUpdateCommand<FakeTableClass>(A.ListOf<FakeTableClass>(5));
            this.fluentDbCommand.AddDeleteCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddDeleteCommand<FakeTableClass>(A.ListOf<FakeTableClass>(5));

            // Assert
            Assert.NotEmpty(this.fluentDbCommand.ActionsToExecute);
            Assert.True(this.fluentDbCommand.ActionsToExecute.Count == 7);
        }

        [Fact]
        public async void FluentDbCommand_Execute_From_ActionQueue_But_NoCommit_Without_Transaction()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.AddCustomCommand("INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.ListOf<FakeTableClass>(5));

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(dispatcherPostExecution: null, throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
            Assert.Null(this.fluentDbCommand.Transaction);
        }

        [Fact]
        public async void FluentDbCommand_Execute_From_ActionQueue_WithCommit()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.ListOf<FakeTableClass>(5));

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(dispatcherPostExecution: null, throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
        }

        [Fact]
        public async void FluentDbCommand_Should_Throw_Execption_When_ThrowOption_IsTrue()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);

            // Act
            Func<Task<DbCommandResult>> execute = () => this.fluentDbCommand.ExecuteAsync(
                "fake insert",
                dispatcherPostExecution: null,
                throwException: true);

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(execute);
        }

        [Fact]
        public async void FluentDbCommand_Should_Execute_Dispatcher()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            var mockFakeInterface = new Mock<IFakeInterface>();

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(
                "INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')",
                dispatcherPostExecution: () => mockFakeInterface.Object.DoSomething(),
                throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            mockFakeInterface.Verify(o => o.DoSomething(), Times.Once);
        }

        [Fact]
        public async void FluentDbCommand_Should_Execute_Dispatcher_After_ActionQueue_Executed()
        {
            // Arrange
            var mockFakeInterface = new Mock<IFakeInterface>();

            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(
                dispatcherPostExecution: () => mockFakeInterface.Object.DoSomething(),
                throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
            mockFakeInterface.Verify(o => o.DoSomething(), Times.Once);
        }

        [Fact]
        public async void FluentDbCommand_Should_ThrowException_After_ActionQueue_Executed()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("fake insert");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddUpdateCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddDeleteCommand<FakeTableClass>(A.New<FakeTableClass>());

            // Act
            Func<Task<DbCommandResult>> execute = () => this.fluentDbCommand.ExecuteAsync(
                dispatcherPostExecution: null,
                throwException: true);

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(() => execute());

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
        }

        [Fact]
        public async void FluentDbCommand_Should_Execute_Dispatcher_And_Throw_Exception_After_ActionQueue_Executed()
        {
            // Arrange
            var mockFakeInterface = new Mock<IFakeInterface>();

            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("fake insert");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());

            // Act
            Func<Task<DbCommandResult>> execute = () => this.fluentDbCommand.ExecuteAsync(
                dispatcherPostExecution: () => mockFakeInterface.Object.DoSomething(),
                throwException: true);

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(() => execute());

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
            mockFakeInterface.Verify(o => o.DoSomething(), Times.Once);
        }

        [Fact]
        public async void FluentDbCommand_Should_Return_CustomValue_From_ReturnDispatcher()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);

            Func<string> returnDispatcher = () => "IsOk";

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(
                "INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')",
                customValueProviderToExecute: returnDispatcher,
                dispatcherPostExecution: null,
                throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Equal("IsOk", result);
        }

        [Fact]
        public async void FluentDbCommand_Should_Return_CustomValue_After_ActionQueue_Executed()
        {
            // Arrange
            Func<string> returnDispatcher = () => "IsOk";
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(
                customValueProviderToExecute: returnDispatcher,
                dispatcherPostExecution: null,
                throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Equal("IsOk", result);

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
        }

        [Fact]
        public async void FluentDbCommand_Should_Return_CustomValue_And_ExecuteDispatcher_After_ActionQueue_Executed()
        {
            // Arrange
            Func<string> returnDispatcher = () => "IsOk";
            var mockFakeInterface = new Mock<IFakeInterface>();

            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("INSERT INTO FakeTable(FakeName, FakeDescription) VALUES('fakename', 'fakedescription')");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());

            // Act
            var result = await this.fluentDbCommand.ExecuteAsync(
                customValueProviderToExecute: returnDispatcher,
                dispatcherPostExecution: () => mockFakeInterface.Object.DoSomething(),
                throwException: false);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Equal("IsOk", result);

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
            mockFakeInterface.Verify(o => o.DoSomething(), Times.Once);
        }

        [Fact]
        public async void FluentDbCommand_Should_ExecuteDispatcher_And_ThrowException_With_ThrowExceptionSetTrue_After_ActionQueue_Executed()
        {
            // Arrange
            var mockFakeInterface = new Mock<IFakeInterface>();

            // Act
            this.fluentDbCommand = new FluentDbCommand(this.fluentDbFixture.DbConnectionProvider);
            this.fluentDbCommand.WithTransaction();
            this.fluentDbCommand.AddCustomCommand("fake insert");
            this.fluentDbCommand.AddInsertCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddUpdateCommand<FakeTableClass>(A.New<FakeTableClass>());
            this.fluentDbCommand.AddDeleteCommand<FakeTableClass>(A.New<FakeTableClass>());

            Func<Task<DbCommandResult>> execute = () => this.fluentDbCommand.ExecuteAsync(
                dispatcherPostExecution: () => mockFakeInterface.Object.DoSomething(),
                throwException: true);

            // Assert
            await Assert.ThrowsAnyAsync<Exception>(() => execute());

            // Action queue should be empty
            Assert.Empty(this.fluentDbCommand.ActionsToExecute);
            mockFakeInterface.Verify(o => o.DoSomething(), Times.Once);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "use in class test")]
    public interface IFakeInterface
    {
        void DoSomething();
    }
}
