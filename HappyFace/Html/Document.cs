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
                        return new Uri(baseHref, UriKind.Absolute);
                    }
                }
            }
            return _response.ResponseUri;
        }

        public IEnumerable<Uri> Links
        {
            get
            {
                return _document.DocumentNode
                                .SelectSingleNode("//body")
                                .SelectNodes("//a[@href]")
                                .Select(link => link.GetAttributeValue("href", String.Empty))
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
                return _document.DocumentNode
                                .SelectSingleNode("//body")
                                .Descendants("p")
                                .Select(x => HtmlEntity.DeEntitize(x.InnerText))
                                .Select(x => x.Trim());
            }
        }
    }
}