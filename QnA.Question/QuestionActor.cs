using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using QnA.Participant.Interfaces;
using QnA.Question.Interfaces;
using QnA.Question.Interfaces.Views;
using QnA.Question.State;

namespace QnA.Question
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class QuestionActor : Actor, IQuestionActor
    {
        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            var defaultQuestionDetail = new QuestionState
            {
                Answer = string.Empty,
                Content = string.Empty
            };

            return StateManager.TryAddStateAsync("details", defaultQuestionDetail);
        }
        
        public async Task SetContentAsync(IParticipantActor participant, string content)
        {
            var details = await StateManager.GetStateAsync<QuestionState>("details");

            details.AskedByParticipant = participant;
            details.Content = content;
          
            await StateManager.SetStateAsync("details", details);
        }

        public async Task SetAnswerAsync(IParticipantActor participant, string answer)
        {
            var details = await StateManager.GetStateAsync<QuestionState>("details");

            details.AnsweredByParticipant = participant;
            details.Answer = answer;
           
            await StateManager.SetStateAsync("details", details);
        }

        public async Task<bool> GetIsAnsweredStateAsync()
        {
            var details = await StateManager.GetStateAsync<QuestionState>("details");
            return !string.IsNullOrWhiteSpace(details.Answer);
        }

        public async Task<QuestionDetail> GetDetailsAsync()
        {
            var details = await StateManager.GetStateAsync<QuestionState>("details");

            return new QuestionDetail
            {
                AskDateTime = details.AskDateTime,
                AnswerDateTime = details.AnswerDateTime,
                Answer = details.Answer,
                Content = details.Content,
                AskedByParticipant = details.AskedByParticipant,
                AnsweredByParticipant = details.AnsweredByParticipant
            };
        }
    }
}
