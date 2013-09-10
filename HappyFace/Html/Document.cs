using System;
using System.Collections.Generic;
using System.Linq;
using HappyFace.Domain;
using HtmlAgilityPack;

namespace HappyFace.Html
{
    public class Document : IDocument
    {
        public Document(FetchResponse response)
        {
            _response = response;
            _document = new HtmlDocument();
            _document.LoadHtml(response.Content);

            _baseUri = new Lazy<Uri>(GetBaseUri);
        }

        private readonly HtmlDocument _document;
        private readonly Lazy<Uri> _baseUri;
        private readonly FetchResponse _response;

        public Uri BaseUri
        {
            get
            {
                return _baseUri.Value;
            }
        }

        private Uri GetBaseUri()
        {
            var baseElements = _document.DocumentNode.SelectNodes("//base/@href");
            if (baseElements != null)
            {
                var baseElement = baseElements.FirstOrDefault();
                if (baseElement != null)
                {
                    var baseHref = baseElement.GetAttributeValue("href", String.Empty);
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
            return _response.ResponseUri;
        }

        public IEnumerable<Uri> Links
        {
            get
            {
                if (_document == null || _document.DocumentNode == null)
                {
                    return Enumerable.Empty<Uri>();
                }

                var documentNode = _document.DocumentNode;
                var body = documentNode.SelectSingleNode("//body");
                var links = (body ?? documentNode).SelectNodes("//a[@href]");

                if (links == null)
                {
                    return Enumerable.Empty<Uri>();
                }

                return links.Select(link => link.GetAttributeValue("href", String.Empty))
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
                if (_document == null || _document.DocumentNode == null)
                {
                    return Enumerable.Empty<string>();
                }

                var documentNode = _document.DocumentNode;
                var root = documentNode.SelectSingleNode("//body") ?? documentNode;

                return root.Descendants("p")
                           .Select(DeEntitize)
                           .Select(x => x.Trim())
                           .Where(x => !String.IsNullOrEmpty(x));

            }
        }

        private static string DeEntitize(HtmlNode x)
        {
            try
            {
                return HtmlEntity.DeEntitize(x.InnerText);
            }
            catch
            {
                return String.Empty;
            }
        }
    }
}