using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using QnA.Participant.Interfaces;
using QnA.Participant.Interfaces.Model;
using QnA.Participant.State;

namespace QnA.Participant
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
    internal class ParticipantActor : Actor, IParticipantActor
    {
        private const string StateName = "state";

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

            return StateManager.TryAddStateAsync(StateName, new ParticipantState());
        }
      
        public async Task<ParticipantDetails> GetDetailsAsync()
        {
            var state = await StateManager.GetStateAsync<ParticipantState>(StateName);
            return new ParticipantDetails
            {
                Name = state.Name,
                Email = state.Email
            };
        }

        public async Task SetDetailsAsync(ParticipantDetails participantDetails)
        {
            var state = await StateManager.GetStateAsync<ParticipantState>(StateName);

            state.Name = participantDetails.Name;
            state.Email = participantDetails.Email;

            await StateManager.SetStateAsync(StateName, state);
        }
    }
}
