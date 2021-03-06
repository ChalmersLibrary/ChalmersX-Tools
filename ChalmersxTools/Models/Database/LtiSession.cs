﻿using LtiLibrary.Core.Lti1;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ChalmersxTools.Models.Database
{
    public class LtiSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [StringLength(64)]
        [Index("IX_StudentWithToolInContext", 1, IsUnique = true)]
        public string ConsumerKey { get; set; }
        [StringLength(64)]
        [Index("IX_StudentWithToolInContext", 2, IsUnique = true)]
        public string UserId { get; set; }
        [StringLength(128)]
        [Index("IX_StudentWithToolInContext", 3, IsUnique = true)]
        public string ContextId { get; set; }
        public DateTime Timestamp { get; set; }
        public string LtiRequestSerialized { get; set; }
        public string UserHostAddress { get; set; }

        private bool _valid = false;
        [NotMapped]
        public bool Valid 
        { 
            get 
            { 
                return _valid; 
            } 
            set 
            { 
                _valid = value; 
            } 
        }
        [NotMapped]
        public LtiRequest LtiRequest { get; set; }

        public void SerializeLtiRequest()
        {
            LtiRequestSerialized = new JavaScriptSerializer().Serialize(LtiRequest);
        }

        public void DeserializeLtiRequest()
        {
            LtiRequest = new JavaScriptSerializer().Deserialize<LtiRequest>(LtiRequestSerialized);
        }
    }
}