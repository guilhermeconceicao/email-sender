using System.IO;
using System.Threading.Tasks;

namespace Email.Sender.BlobStorage
{
    public interface IBlobStorage
    {
        Task<Stream> GetStreamAsync(IBlob blob);
    }
}