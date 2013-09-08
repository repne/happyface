using HappyFace.Domain;

namespace HappyFace.Html
{
    public interface IDocumentFactory
    {
        IDocument Create(FetchResponse response);
    }
}