﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ChalmersxTools.Models;
using System.Configuration;
using ChalmersxTools.Models.Database;

namespace ChalmersxTools.Database
{
    public class LearningToolServerDbContext : DbContext
    {
        public LearningToolServerDbContext() : base("name=chalmersxToolsConnectionString") {}

        public DbSet<LtiSession> LtiSessions { get; set; }
        public DbSet<StudentPresentation> StudentPresentations { get; set; }
        public DbSet<EarthSpheresImagesSubmission> EarthSpheresImagesSubmissions { get; set; }
        public DbSet<EarthMassSubmission> EarthMassSubmissions { get; set; }
        public DbSet<TemperatureMeasurementSubmission> TemperatureMeasurementSubmissions { get; set; }
        public DbSet<SingleTemperatureMeasurementSubmission> SingleTemperatureMeasurementSubmissions { get; set; }
        public DbSet<EarthMassV2Submission> EarthMassV2Submissions { get; set; }
        public DbSet<LocationSubmission> LocationSubmissions { get; set; }
    }
}