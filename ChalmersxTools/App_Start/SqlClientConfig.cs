using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace ChalmersxTools.App_Start
{
    public class SqlClientConfig
    {
        public static void EnableCodeFirstMigrations()
        {
            var configuration = new Migrations.Configuration();
            var migrator = new DbMigrator(configuration);
            if (migrator.GetPendingMigrations().Count() > 0)
            {
                migrator.Update();
            }
        }
    }
}