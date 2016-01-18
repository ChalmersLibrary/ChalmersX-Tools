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
            if (bool.Parse(ConfigurationManager.AppSettings["updateDatabaseToLatestVersion"]))
            {
                var configuration = new Migrations.Configuration();
                var migrator = new DbMigrator(configuration);
                migrator.Update();
            }
        }
    }
}