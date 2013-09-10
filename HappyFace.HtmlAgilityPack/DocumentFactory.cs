using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.HtmlAgilityPack
{
    public class DocumentFactory : IDocumentFactory
    {
        public IDocument Create(FetchResult result)
        {
            return new Document(result);
        }
    }
}