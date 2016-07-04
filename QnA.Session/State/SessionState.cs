using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using QnA.Participant.Interfaces;
using QnA.Question.Interfaces;

namespace QnA.Session.State
{
    [DataContract]
    internal class SessionState
    {
        [DataMember]
        public IParticipantActor Presenter { get; set; }
        public string Name { get; set; }
        [DataMember]
        public DateTime StartDateTime { get; set; }
        [DataMember]
        public DateTime EndDateTime { get; set; }
        [DataMember]
        public List<IParticipantActor> Attendees { get; set; }
        [DataMember]
        public List<IQuestionActor> Questions { get; set; }
    }
}