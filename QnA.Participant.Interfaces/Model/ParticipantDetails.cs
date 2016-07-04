using System.Runtime.Serialization;

namespace QnA.Participant.Interfaces.Model
{
    [DataContract]
    public class ParticipantDetails
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}