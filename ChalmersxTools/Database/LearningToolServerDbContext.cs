using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ChalmersxTools.Models;
using System.Configuration;

namespace ChalmersxTools.Database
{
    public class LearningToolServerDbContext : DbContext
    {
        public LearningToolServerDbContext() : base(ConfigurationManager.ConnectionStrings["chalmersxToolsConnectionString"].ToString()) {}

        public DbSet<LtiSession> LtiSessions { get; set; }
        public DbSet<StudentPresentation> StudentPresentations { get; set; }
    }
}