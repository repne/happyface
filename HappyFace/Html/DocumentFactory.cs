using HappyFace.Domain;

namespace HappyFace.Html
{
    public class DocumentFactory : IDocumentFactory
    {
        public IDocument Create(FetchResponse response)
        {
            return new Document(response);
        }
    }
}