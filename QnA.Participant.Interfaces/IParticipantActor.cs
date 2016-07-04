﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using QnA.Participant.Interfaces.Model;

namespace QnA.Participant.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IParticipantActor : IActor
    {
        Task<ParticipantDetails> GetDetailsAsync();
        Task SetDetailsAsync(ParticipantDetails participantDetails);
    }
}
