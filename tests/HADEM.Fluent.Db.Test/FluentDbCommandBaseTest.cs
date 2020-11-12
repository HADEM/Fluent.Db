// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HADEM.Fluent.Db.Exception;
    using Moq;
    using Xunit;

    public class FluentDbCommandBaseTest
    {
        private FluentDbCommandBase fluentDbCommand;

        [Fact]
        public void CheickDbObjectCommand_Should_Throw_ArgumentNull_Exception()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommandBase();
            DbObjectCommand nullCommand = null;
            DbObjectCommand emptyCommand = new DbObjectCommand();
            emptyCommand.Parameters = Enumerable.Empty<object>();
            emptyCommand.ScriptSql = "SELECT 1";

            DbObjectCommand emptyCommand_2 = new DbObjectCommand();
            emptyCommand.Parameters = new object[] { 10 };
            emptyCommand.ScriptSql = string.Empty;

            // Assert
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(nullCommand));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(emptyCommand_2));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(emptyCommand));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(new List<DbObjectCommand>() { emptyCommand }));
        }

        [Fact]
        public void CheickDbObjectCommand_Generic_Should_Throw_ArgumentNull_Exception()
        {
            // Arrange
            this.fluentDbCommand = new FluentDbCommandBase();
            DbObjectCommand<string> nullCommand = null;
            DbObjectCommand<string> emptyCommand = new DbObjectCommand<string>();
            emptyCommand.ObjectParameter = null;
            emptyCommand.Operation = DbOperation.Delete;
            emptyCommand.ScriptSql = string.Empty;

            DbObjectCommand<string> emptyCommand_2 = new DbObjectCommand<string>();
            emptyCommand_2.ObjectParameter = "fake param";
            emptyCommand_2.Operation = DbOperation.ExecuteSql;
            emptyCommand_2.ScriptSql = string.Empty;

            // Assert
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(nullCommand));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(emptyCommand));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(emptyCommand_2));
            Assert.Throws<ArgumentNullException>(() => this.fluentDbCommand.CheickDbObjectCommand(new List<DbObjectCommand<string>>() { emptyCommand }));
        }

        [Fact]
        public async Task SafeExecuteDbCommandAction_Should_Execute_Once_With_RetryPolicyNull()
        {
            // Arrange
            var mockFakeFuncInterface = new Mock<IFakeFuncInterface>();
            mockFakeFuncInterface.Setup(x => x.DoSomething()).Returns(Task.FromResult<DbCommandResult>(new DbCommandResult() { Exception = null, IsSuccess = true, Result = 1 }));

            // Act
            var result = await FluentDbCommandBase.SafeExecuteDbCommandAction(() => mockFakeFuncInterface.Object.DoSomething(), null, false);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Exception);
            Assert.Equal(1, result.Result);
            mockFakeFuncInterface.Verify(x => x.DoSomething(), Times.Once);
        }

        [Fact]
        public async Task SafeExecuteDbCommandAction_Should_Execute_Once_With_RetryPolicy_And_NoException()
        {
            // Arrange
            var mockFakeFuncInterface = new Mock<IFakeFuncInterface>();
            mockFakeFuncInterface.Setup(x => x.DoSomething()).Returns(Task.FromResult<DbCommandResult>(new DbCommandResult() { Exception = null, IsSuccess = true, Result = 1 }));
            RetryPolicyOption retry = new RetryPolicyOption();
            retry.MaxRetries = 10;
            retry.ShouldRetryOn<Exception>();

            // Act
            var result = await FluentDbCommandBase.SafeExecuteDbCommandAction(() => mockFakeFuncInterface.Object.DoSomething(), retry, true);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Null(result.Exception);
            Assert.Equal(1, result.Result);
            mockFakeFuncInterface.Verify(x => x.DoSomething(), Times.Once);
        }

        [Fact]
        public async Task SafeExecuteDbCommandAction_Should_Execute_MultipleTimes_Defined_By_RetryPolicy_And_But_ShouldNot_Throw_Exception()
        {
            // Arrange
            var mockFakeFuncInterface = new Mock<IFakeFuncInterface>();
            mockFakeFuncInterface.Setup(x => x.DoSomething()).Throws<Exception>();
            RetryPolicyOption retry = new RetryPolicyOption();
            retry.MaxRetries = 10;
            retry.ShouldRetryOn<Exception>();

            // Act
            var result = await FluentDbCommandBase.SafeExecuteDbCommandAction(() => mockFakeFuncInterface.Object.DoSomething(), retry, false);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Exception);
            Assert.IsType<RetryReachedException>(result.Exception);
            mockFakeFuncInterface.Verify(x => x.DoSomething(), Times.Exactly(10));
        }

        [Fact]
        public async Task SafeExecuteDbCommandAction_Should_Execute_MultipleTimes_Defined_By_RetryPolicy_And_But_Should_Throw_Exception()
        {
            // Arrange
            var mockFakeFuncInterface = new Mock<IFakeFuncInterface>();
            mockFakeFuncInterface.Setup(x => x.DoSomething()).Throws<Exception>();
            RetryPolicyOption retry = new RetryPolicyOption();
            retry.MaxRetries = 10;
            retry.ShouldRetryOn<Exception>();

            Func<Task<DbCommandResult>> execution = () => FluentDbCommandBase.SafeExecuteDbCommandAction(() => mockFakeFuncInterface.Object.DoSomething(), retry, true);

            // Assert
            await Assert.ThrowsAnyAsync<RetryReachedException>(execution);
            mockFakeFuncInterface.Verify(x => x.DoSomething(), Times.Exactly(10));
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "use in class test")]
    public interface IFakeFuncInterface
    {
        Task<DbCommandResult> DoSomething();
    }
}
