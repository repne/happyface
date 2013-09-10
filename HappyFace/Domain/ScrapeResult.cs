using System;
using System.Collections.Generic;

namespace HappyFace.Domain
{
    public class ScrapeResult
    {
        public IEnumerable<Uri> Links { get; set; }
    }
}