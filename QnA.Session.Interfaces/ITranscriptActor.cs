using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace QnA.Session.Interfaces
{
    public interface ITranscriptActor : IActor
    {
        Task<List<string>> GetEntriesAsync();
        Task RegisterViewAsync(ITranscriptViewActor transcriptViewActor);

        Task UnregisterViewAsync(ITranscriptViewActor transcriptViewActor);
    }
}