using System;
using System.Runtime.Serialization;
using QnA.Participant.Interfaces;

namespace QnA.Question.Interfaces.Views
{
    [DataContract]
    public class QuestionDetail
    {
        [DataMember]
        public DateTime AskDateTime { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public DateTime AnswerDateTime { get; set; }
        [DataMember]
        public string Answer { get; set; }
        [DataMember]
        public IParticipantActor AskedByParticipant { get; set; }
        [DataMember]
        public IParticipantActor AnsweredByParticipant { get; set; }
    }
}