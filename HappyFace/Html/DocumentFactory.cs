using HappyFace.Domain;

namespace HappyFace.Html
{
    public class DocumentFactory : IDocumentFactory
    {
        public IDocument Create(FetchResult result)
        {
            return new Document(result);
        }
    }
}