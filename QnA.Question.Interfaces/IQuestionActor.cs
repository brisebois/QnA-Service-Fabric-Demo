using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using QnA.Participant.Interfaces;
using QnA.Question.Interfaces.Views;

namespace QnA.Question.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>

    public interface IQuestionActor : IActor
    {
        Task SetContentAsync(IParticipantActor participant, string content);
        Task SetAnswerAsync(IParticipantActor participant, string content);
        Task<bool> GetIsAnsweredStateAsync();
        Task<QuestionDetail> GetDetailsAsync();
    }
}
