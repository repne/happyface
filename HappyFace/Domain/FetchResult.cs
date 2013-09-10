using System;
using System.Net;

namespace HappyFace.Domain
{
    public class FetchResult
    {
        public int Level { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Uri ResponseUri { get; set; }
        public DateTime LastModified { get; set; }
        public string Content { get; set; }
    }
}