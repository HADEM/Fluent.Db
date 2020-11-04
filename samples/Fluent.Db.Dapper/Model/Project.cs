using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fluent.Db.Dapper.Model
{
    [Table("dbo.Project")]
    public class Project
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Budget { get; set; }

        public int AssigneeId { get; set; }
    }
}
