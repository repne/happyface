using System;
using System.Collections.Generic;

namespace HappyFace.Domain
{
    public class ScrapeResponse
    {
        public IEnumerable<Uri> Links { get; set; }
    }
}