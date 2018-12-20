using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EmployeeWebsite
{
    public class mdbcontext : DbContext
    {
        public mdbcontext() : base("name=Employee")
        {
            var cs = ConfigurationManager.ConnectionStrings["Employee"]
                                         .ConnectionString;

            this.Database.Connection.ConnectionString = cs;
        }

        public System.Data.Entity.DbSet<EmployeeWebsite.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<EmployeeWebsite.Models.Employee> Employees { get; set; }

        public System.Data.Entity.DbSet<EmployeeWebsite.Models.Department> Departments { get; set; }
    }
}