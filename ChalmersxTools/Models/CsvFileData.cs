using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChalmersxTools.Models
{
    public class CsvFileData
    {
        public static readonly string CONTENT_TYPE = "text/csv";

        public string Filename { get; set; }
        public byte[] Data;

        public CsvFileData(string filename, byte[] data)
        {
            Filename = filename;
            Data = data;
        }
    }
}