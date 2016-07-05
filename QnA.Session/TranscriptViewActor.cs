using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using QnA.Session.Interfaces;

namespace QnA.Session
{
    [StatePersistence(StatePersistence.None)]
    internal class TranscriptViewActor : Actor, ITranscriptViewActor
    {
        private List<string> cachedEntires;
        private long cachedSessionId;

        public async Task<List<string>> GetEntriesAsync(long sessionId)
        {
            if (cachedEntires != null)
                return cachedEntires;

            cachedSessionId = sessionId;

            var transcriptActor = ActorProxy.Create<ITranscriptActor>(new ActorId(sessionId));

            await transcriptActor.RegisterViewAsync(this);

            return cachedEntires;
        }

        public Task UpdateLocalCopyAsync(List<string> entries)
        {
            cachedEntires = entries;

            return Task.FromResult(true);
        }

        protected override Task OnDeactivateAsync()
        {
            var transcriptActor = ActorProxy.Create<ITranscriptActor>(new ActorId(cachedSessionId));

            return base.OnDeactivateAsync();
        }
    }
}