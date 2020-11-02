// Copyright (c) HADEM. All rights reserved.

namespace HADEM.Fluent.Db.Dapper.Test
{
    using HADEM.Fluent.Db.Test.Core;
    using Xunit;

    [CollectionDefinition(FluentDbFixture.FixtureName)]
    public class FluentDbDapperTestFixture : ICollectionFixture<FluentDbFixture>
    {
    }
}
