using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChalmersxTools.Models.Database
{
    public class TemperatureMeasurementSubmission
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
        public Coordinate Position { get; set; }
        public double? Measurement1 { get; set; }
        public double? Measurement2 { get; set; }

    }
}