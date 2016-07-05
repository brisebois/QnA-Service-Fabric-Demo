using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace QnA.Session.Interfaces
{
    public interface ITranscriptViewActor : IActor
    {
        Task<List<string>> GetEntriesAsync(long sessionId);

        Task UpdateLocalCopyAsync(List<string> entries);
    }
}