using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using QnA.Session.Interfaces;
using QnA.Session.Interfaces.Models;
using QnA.Session.State;

namespace QnA.Session
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
    internal class SessionListActor : Actor, ISessionListActor
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

            return StateManager.TryAddStateAsync(StateName, new SessionListState {Sessions = new List<ISessionActor>()});
        }
        
        public async Task<ISessionActor> AddAsync(SessionDetails sessionDetails)
        {
            var session = ActorProxy.Create<ISessionActor>(ActorId.CreateRandom());

            await session.SetDetailsAsync(sessionDetails);

            var state = await StateManager.GetStateAsync<SessionListState>(StateName);

            state.Sessions.Add(session);

            await StateManager.SetStateAsync(StateName, state);

            return session;
        }

        public async Task RemoveAsync(ISessionActor sessionActor)
        {
            var state = await StateManager.GetStateAsync<SessionListState>(StateName);

            state.Sessions.Remove(sessionActor);

            await StateManager.SetStateAsync(StateName, state);
        }

        public async Task<List<ISessionActor>> GetSessionsAsync()
        {
            var state = await StateManager.GetStateAsync<SessionListState>(StateName);
            return state.Sessions;
        }
    }
}