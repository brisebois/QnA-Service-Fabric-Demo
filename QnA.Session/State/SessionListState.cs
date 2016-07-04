using System.Collections.Generic;
using System.Runtime.Serialization;
using QnA.Session.Interfaces;

namespace QnA.Session.State
{
    [DataContract]
    internal class SessionListState
    {
        [DataMember]
        internal List<ISessionActor> Sessions { get; set; }
    }
}