using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using QnA.Session.Interfaces.Models;

namespace QnA.Session.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface ISessionListActor : IActor
    {
        Task<ISessionActor> AddAsync(SessionDetails sessionDetails);
        Task RemoveAsync(ISessionActor sessionActor);
        Task<List<ISessionActor>> GetSessionsAsync();
    }
}