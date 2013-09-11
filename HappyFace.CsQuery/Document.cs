using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsQuery;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.CsQuery
{
    public class Document : IDocument
    {
        private readonly FetchResult _result;
        private readonly CQ _document;

        public Document(FetchResult result)
        {
            _result = result;
            _document = CQ.CreateDocument(result.Content);

            _baseUri = new Lazy<Uri>(GetBaseUri);
        }

        private readonly Lazy<Uri> _baseUri;

        public Uri BaseUri
        {
            get
            {
                return _baseUri.Value;
            }
        }

        private Uri GetBaseUri()
        {
            var baseElements = _document["base"];
            if (baseElements != null)
            {
                var baseElement = baseElements[0];
                if (baseElement != null)
                {
                    var baseHref = baseElement["href"];
                    if (!String.IsNullOrWhiteSpace(baseHref))
                    {
                        Uri uri;
                        if (Uri.TryCreate(baseHref, UriKind.Absolute, out uri))
                        {
                            return uri;
                        }
                    }
                }
            }
            return _result.ResponseUri;
        }

        public IEnumerable<Uri> Links
        {
            get
            {
                if (_document == null)
                {
                    return Enumerable.Empty<Uri>();
                }

                var links = _document["a"];

                if (links == null)
                {
                    return Enumerable.Empty<Uri>();
                }

                return links.Select(link => link["href"])
                            .Where(x => !String.IsNullOrWhiteSpace(x))
                            .Where(x => Uri.IsWellFormedUriString(x, UriKind.RelativeOrAbsolute))
                            .Distinct()
                            .Select(x => new Uri(x, UriKind.RelativeOrAbsolute));
            }
        }

        public IEnumerable<string> Paragraphs
        {
            get
            {
                if (_document == null)
                {
                    return Enumerable.Empty<string>();
                }

                return _document["p"].Select(DeEntitize)
                                     .Select(x => x.Trim())
                                     .Where(x => !String.IsNullOrEmpty(x));

            }
        }

        // I have no clue on how CsQuery works

        private static string GetText(IDomObject x, Func<IDomObject, string> render)
        {
            if (x.HasChildren)
            {
                return string.Join("", x.ChildNodes
                                        .Where(y => y.ChildrenAllowed == false || !string.IsNullOrWhiteSpace(y.InnerText))
                                        .Where(y => y.NodeName != "BR" && y.NodeName != "SCRIPT")
                                        .Select(y => GetText(y, render)));
            }
            else
            {
                return string.Format(" {0} ", render(x).Trim());
            }
        }

        private static string GetParagraphText(IDomObject x)
        {
            if (string.IsNullOrWhiteSpace(x.InnerText))
            {
                return String.Empty;
            }

            return GetText(x, y => y.Render(OutputFormatters.HtmlEncodingMinimum)).Replace("  ", " ")
                                                                                  .Replace("  ", " ")
                                                                                  .Trim();
        }

        private static string DeEntitize(IDomObject x)
        {
            try
            {
                return GetParagraphText(x);
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}