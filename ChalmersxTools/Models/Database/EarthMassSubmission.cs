using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models.Database
{
    public class EarthMassSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(64)]
        [Index("IX_UserInContext", 1, IsUnique = true)]
        public string UserId { get; set; }
        [StringLength(128)]
        [Index("IX_UserInContext", 2, IsUnique = true)]
        public string ContextId { get; set; }
        public double MeanGravityAcceleration { get; set; }
        public double TotalEarthMass { get; set; }
    }
}