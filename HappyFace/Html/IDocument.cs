using System;
using System.Collections.Generic;

namespace HappyFace.Html
{
    public interface IDocument
    {
        Uri BaseUri { get; }
        IEnumerable<Uri> Links { get; }
        IEnumerable<string> Paragraphs { get; }
    }
}