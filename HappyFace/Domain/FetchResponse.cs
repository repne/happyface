using System;
using System.Net;

namespace HappyFace.Domain
{
    public class FetchResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public Uri ResponseUri { get; set; }
        public DateTime LastModified { get; set; }
        public string Content { get; set; }
    }
}