using System;
using System.Runtime.Serialization;
using QnA.Participant.Interfaces;

namespace QnA.Session.Interfaces.Views
{
    [DataContract]
    public class SessionDetailView
    {
        [DataMember]
        public int ParticipantCount { get; set; }
        [DataMember]
        public IParticipantActor Presenter { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime StartDateTime { get; set; }
        [DataMember]
        public DateTime EndDateTime { get; set; }
        [DataMember]
        public bool IsOngoing => DateTime.UtcNow > StartDateTime && DateTime.UtcNow < EndDateTime;
    }
}