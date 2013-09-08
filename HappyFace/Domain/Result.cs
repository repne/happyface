using System;
using System.Collections.Generic;

namespace HappyFace.Domain
{
    public class Result
    {
        public DateTime LastModified { get; set; }
        public Uri ResponseUri { get; set; }
        public string[] Paragraphs { get; set; }
        public Uri[] Links { get; set; }

        public string GetKey()
        {
            return ResponseUri.ToString();
        }
    }
}