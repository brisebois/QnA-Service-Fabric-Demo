using System;
using System.Runtime.Serialization;
using QnA.Participant.Interfaces;

namespace QnA.Session.Interfaces.Models
{
    [DataContract]
    public class SessionDetails
    {
        [DataMember]
        public IParticipantActor Presenter { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime StartDateTime { get; set; }
        [DataMember]
        public DateTime EndDateTime { get; set; }
        [DataMember]
        public int AttendeeCount { get; set; }
    }
}