using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Fabric.Health;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using QnA.Session.Interfaces;
using QnA.Session.Interfaces.Models;

namespace QnA.Session
{
    [StatePersistence(StatePersistence.Volatile)]
    internal class TranscriptActor : Actor, ITranscriptActor, IRemindable
    {
        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            await RegisterReminderAsync("materialize",
                                        new byte[0],
                                        TimeSpan.FromSeconds(0),
                                        TimeSpan.FromSeconds(2));

            await StateManager.TryAddStateAsync("lines", new List<string>());
            await StateManager.TryAddStateAsync("views", new List<ITranscriptViewActor>());
        }

        public async Task<List<string>> GetEntriesAsync()
        {
            var list = await StateManager.GetStateAsync<List<string>>("lines");
            return list;
        }

        public async Task RegisterViewAsync(ITranscriptViewActor transcriptViewActor)
        {
            var views = await StateManager.GetStateAsync<List<ITranscriptViewActor>>("views");
            if (views.Contains(transcriptViewActor))
                return;

            views.Add(transcriptViewActor);

            await StateManager.SetStateAsync("views", views);

            var list = await StateManager.GetStateAsync<List<string>>("lines");
            await transcriptViewActor.UpdateLocalCopyAsync(list);
        }

        public async Task UnregisterViewAsync(ITranscriptViewActor transcriptViewActor)
        {
            var views = await StateManager.GetStateAsync<List<ITranscriptViewActor>>("views");
            if (!views.Contains(transcriptViewActor))
                return;

            views.Remove(transcriptViewActor);

        }

        public async Task ReceiveReminderAsync(string reminderName,
                                               byte[] context,
                                               TimeSpan dueTime,
                                               TimeSpan period)
        {
            var sessionId = new ActorId(this.GetActorId().GetLongId());

            var session = ActorProxy.Create<ISessionActor>(sessionId);

            var sessionDetails = await session.GetDetailsAsync();

            if (IsSessionEnded(sessionDetails))
            {
                await UnregisterReminderAsync(GetReminder("reminderName"));
            }

            var questions = await session.GetQuestionsAsync();
            var lines = new List<string>();
            foreach (var questionActor in questions)
            {
                var questionDetails = await questionActor.GetDetailsAsync();

                var participantDetails = await questionDetails.AskedByParticipant.GetDetailsAsync();

                lines.Add($"{questionDetails.AskDateTime.ToString("t")}: {participantDetails.Name} asked '{questionDetails.Content}'");

                if (string.IsNullOrWhiteSpace(questionDetails.Answer)) continue;

                var answeringParticipantDetails = await questionDetails.AnsweredByParticipant.GetDetailsAsync();

                lines.Add($"{questionDetails.AnswerDateTime.ToString("t")}: {answeringParticipantDetails.Name} answered '{questionDetails.Content}'");
            }

            await StateManager.SetStateAsync("lines", lines);

            var views = await StateManager.GetStateAsync<List<ITranscriptViewActor>>("views");

            views.AsParallel().ForAll(v =>
            {
                v.UpdateLocalCopyAsync(lines);
            });
        }

        private static bool IsSessionEnded(SessionDetails sessionDetails)
        {
            return sessionDetails.EndDateTime < DateTime.UtcNow;
        }
    }
}