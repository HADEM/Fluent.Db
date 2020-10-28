// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    }
}
