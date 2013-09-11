using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HappyFace.Domain;
using HappyFace.Html;

namespace HappyFace.CsQuery
{
    public class DocumentFactory : IDocumentFactory
    {
        public IDocument Create(FetchResult result)
        {
            return new Document(result);
        }
    }
}
