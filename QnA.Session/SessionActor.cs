using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using QnA.Participant.Interfaces;
using QnA.Question.Interfaces;
using QnA.Session.Interfaces;
using QnA.Session.Interfaces.Models;
using QnA.Session.State;

namespace QnA.Session
{
    /// <remarks>
    ///     This class represents an actor.
    ///     Every ActorID maps to an instance of this class.
    ///     The StatePersistence attribute determines persistence and replication of actor state:
    ///     - Persisted: State is written to disk and replicated.
    ///     - Volatile: State is kept in memory only and replicated.
    ///     - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class SessionActor : Actor, ISessionActor
    {
        private const string StateName = "state";

        public async Task<SessionDetails> GetDetailsAsync()
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            var sessionDetailView = new SessionDetails
            {
                StartDateTime = state.StartDateTime,
                EndDateTime = state.EndDateTime,
                Name = state.Name,
                Presenter = state.Presenter, 
                AttendeeCount = state.Attendees.Count
            };

            return sessionDetailView;
        }

        public async Task AddAttendeeAsync(IParticipantActor attendee)
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            if (IsAnAttendee(attendee, state))
                return;

            state.Attendees.Add(attendee);

            await StateManager.SetStateAsync(StateName, state);
        }

        private static bool IsAnAttendee(IParticipantActor attendee, SessionState state)
        {
            return state.Attendees.Any(a => a.GetActorId() == attendee.GetActorId());
        }

        public async Task AddQuestionAsync(IQuestionActor questionActor)
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            state.Questions.Add(questionActor);

            await StateManager.SetStateAsync(StateName, state);
        }

        public async Task StartAsync()
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            if (IsAfterExpectedStartDateTime(state))
                return;

            state.StartDateTime = DateTime.UtcNow;
            await StateManager.SetStateAsync(StateName, state);
        }

        public async Task EndAsync()
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            if (IsNotStarted(state))
                return;

            state.EndDateTime = DateTime.UtcNow;
            await StateManager.SetStateAsync(StateName, state);
        }

        public async Task SetDetailsAsync(SessionDetails sessionDetails)
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);

            state.Presenter = sessionDetails.Presenter;
            state.StartDateTime = sessionDetails.StartDateTime;
            state.EndDateTime = sessionDetails.EndDateTime;
            state.Name = sessionDetails.Name;
           
            await StateManager.SetStateAsync(StateName, state);
        }

        public async Task<List<IQuestionActor>> GetQuestionsAsync()
        {
            var state = await StateManager.GetStateAsync<SessionState>(StateName);
            return state.Questions;
        }

        /// <summary>
        ///     This method is called whenever an actor is activated.
        ///     An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see http://aka.ms/servicefabricactorsstateserialization

            return StateManager.TryAddStateAsync(StateName, new SessionState
            {
                Attendees = new List<IParticipantActor>(),
                Questions = new List<IQuestionActor>()
            });
        }

        private static bool IsAfterExpectedStartDateTime(SessionState state)
        {
            return state.StartDateTime < DateTime.UtcNow;
        }

        private static bool IsNotStarted(SessionState state)
        {
            return state.StartDateTime > DateTime.UtcNow;
        }
    }
}