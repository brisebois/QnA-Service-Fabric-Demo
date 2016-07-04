using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QnA.Session.Interfaces.Models
{
    [DataContract]
    public class SessionListDetails
    {
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public List<SessionDetails> Sessions { get; set; }
    }
}