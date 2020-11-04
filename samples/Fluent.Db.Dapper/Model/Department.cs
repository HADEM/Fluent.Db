using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fluent.Db.Dapper.Model
{
    [Table("dbo.Department")]
    public class Department
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Manager { get; set; }
    }
}
