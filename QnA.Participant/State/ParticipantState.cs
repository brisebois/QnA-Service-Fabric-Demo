using System.Runtime.Serialization;

namespace QnA.Participant.State
{
    [DataContract]
    internal class ParticipantState
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}