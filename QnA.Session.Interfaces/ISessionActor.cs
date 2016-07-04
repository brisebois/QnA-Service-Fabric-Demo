using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using QnA.Participant.Interfaces;
using QnA.Question.Interfaces;
using QnA.Session.Interfaces.Models;

namespace QnA.Session.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ISessionActor : IActor
    {
        Task<SessionDetails> GetDetailsAsync();
        Task AddAttendeeAsync(IParticipantActor attendee);
        Task AddQuestionAsync(IQuestionActor questionActor);
        Task StartAsync();
        Task EndAsync();
        Task SetDetailsAsync(SessionDetails sessionDetails);
        Task<List<IQuestionActor>>  GetQuestionsAsync();
    }
}
