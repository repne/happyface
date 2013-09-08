using System;
using System.Collections.Generic;

namespace HappyFace.Domain
{
    public class Result
    {
        public DateTime LastModified { get; set; }
        public Uri ResponseUri { get; set; }
        public IEnumerable<string> Paragraphs { get; set; }
        public IEnumerable<Uri> Links { get; set; }

        public string GetKey()
        {
            return ResponseUri.ToString();
        }
    }
}