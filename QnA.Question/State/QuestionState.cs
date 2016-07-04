using System;
using System.Runtime.Serialization;
using QnA.Participant.Interfaces;

namespace QnA.Question.State
{
    [DataContract]
    public class QuestionState
    {
        private string answer;
        private string content;

        [DataMember]
        public IParticipantActor AskedByParticipant { get; set; }

        [DataMember]
        public IParticipantActor AnsweredByParticipant { get; set; }

        [DataMember]
        public DateTime AskDateTime { get; private set; } = DateTime.MinValue;

        [DataMember]
        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                AskDateTime = DateTime.UtcNow;
            }
        }

        [DataMember]
        public DateTime AnswerDateTime { get; private set; } = DateTime.MinValue;

        [DataMember]
        public string Answer
        {
            get { return answer; }
            set
            {
                answer = value;
                AnswerDateTime = DateTime.UtcNow;
            }
        }
    }
}