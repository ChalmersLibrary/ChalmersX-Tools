﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.Database
{
    public class EarthSpheresImagesSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(64)]
        [Index("IX_UserInCourseRun", 1, IsUnique = true)]
        public string UserId { get; set; }
        [StringLength(64)]
        [Index("IX_UserInCourseRun", 2, IsUnique = true)]
        public string CourseOrg { get; set; }
        [StringLength(64)]
        [Index("IX_UserInCourseRun", 3, IsUnique = true)]
        public string CourseId { get; set; }
        [StringLength(64)]
        [Index("IX_UserInCourseRun", 4, IsUnique = true)]
        public string CourseRun { get; set; }
        public string Sphere1Url { get; set; }
        public string Sphere1Name { get; set; }
        public string Sphere2Url { get; set; }
        public string Sphere2Name { get; set; }
    }
}