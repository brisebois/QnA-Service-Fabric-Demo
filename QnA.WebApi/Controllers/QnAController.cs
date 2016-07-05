using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json.Linq;
using QnA.WebApi.Framework;
using QnA.Participant.Interfaces;
using QnA.Participant.Interfaces.Model;
using QnA.Question.Interfaces;
using QnA.Session.Interfaces;
using QnA.Session.Interfaces.Models;

namespace QnA.WebApi.Controllers
{
    [System.Web.Http.RoutePrefix("api")]
    public class QnAController : ApiController
    {
        private const string SessionListActorId = "session-list";
        // Get api/
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("", Name = "Default")]
        public HttpResponseMessage Default()
        {
            var r = Representation.Make();

            r.AddValue("message", "hello");
            r.AddLink("register", Url.Route("Register", new { }));

            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, r.Json());
            return httpResponseMessage;
        }

        // POST api/
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("", Name = "Register")]
        public async Task<HttpResponseMessage> RegisterAsync([FromBody] Models.Participant participant)
        {
            var id = GuidUtility.Create(GuidUtility.UrlNamespace, participant.Email);

            var participantActor = ActorProxy.Create<IParticipantActor>(new ActorId(id));
            var participantDetails = new ParticipantDetails
            {
                Name = participant.Name,
                Email = participant.Email
            };
            await participantActor.SetDetailsAsync(participantDetails);

            var r = Representation.Make();

            r.AddValue("name", participant.Name);
            r.AddValue("id", id.ToString());
            r.AddLink("add-session", Url.Route("AddSession", new { participantId = id }));
            r.AddLink("list-sessions", Url.Route("ListSessions", new { participantId = id }));

            return Request.CreateResponse(HttpStatusCode.Created, r.Json());
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions", Name = "AddSession")]
        public async Task<HttpResponseMessage> AddSessionAsync([FromUri] Guid participantId,
                                                               [FromBody] Models.Session session)
        {
            var participantActor = ActorProxy.Create<IParticipantActor>(new ActorId(participantId));

            var sessionListActor = ActorProxy.Create<ISessionListActor>(new ActorId(SessionListActorId));

            var sessionActor = await sessionListActor.AddAsync(new SessionDetails
            {
                Presenter = participantActor,
                Name = session.Name,
                StartDateTime = session.StartTime,
                EndDateTime = session.EndTime
            });

            var r = Representation.Make();

            r.AddValue("message", $"Created session : {session.Name}");

            var sessionId = sessionActor.GetActorId().GetLongId().ToString();

            r.AddLink("list-sessions", Url.Route("ListSessions", new { participantId = participantId }));
            r.AddLink("session", Url.Route("Session", new { participantId = participantId, sessionId = sessionId }));

            return Request.CreateResponse(HttpStatusCode.Created, r.Json());
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("{participantId:guid}/sessions", Name = "ListSessions")]
        public async Task<HttpResponseMessage> ListSessionsAsync([FromUri] Guid participantId,
                                                                 [FromBody] Models.Session session)
        {
            var sessionListActor = ActorProxy.Create<ISessionListActor>(new ActorId(SessionListActorId));

            var sessionActors = await sessionListActor.GetSessionsAsync();

            var list = new List<JObject>();

            foreach (var s in sessionActors)
            {
                var details = await s.GetDetailsAsync();

                var sr = Representation.Make()
                    .AddValue("name", details.Name)
                    .AddValue("start", details.StartDateTime.ToString("F"))
                    .AddLink("join-session", Url.Route("JoinSession", new { participantId, sessionId = s.GetActorId().GetLongId() }));
                list.Add(sr.Json());
            }

            var r = Representation.Make();

            r.AddArray("sessions", list.ToArray());
            r.AddLink("add-session", Url.Route("AddSession", new { participantId = participantId }));

            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK, r.Json());
            return httpResponseMessage;
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/join", Name = "JoinSession")]
        public async Task<HttpResponseMessage> JoinSessionAsync([FromUri] Guid participantId,
                                                                [FromUri] long sessionId)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));

            var details = await sessionActor.GetDetailsAsync();

            if (IsNotPresenter(participantId, details))
            {
                var participantActor = ActorProxy.Create<IParticipantActor>(new ActorId(participantId));

                await sessionActor.AddAttendeeAsync(participantActor);
            }

            var r = Representation.Make();

            r.AddValue("message", "Joined session");

            if (IsPresenter(participantId, details))
                r.AddLink("session", Url.Route("Session", new { participantId = participantId, sessionId = sessionId }));

            r.AddLink("session-transcript", Url.Route("Transcript", new { participantId = participantId, sessionId = sessionId }));

            return Request.CreateResponse(HttpStatusCode.OK, r.Json());
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/start", Name = "StartSession")]
        public async Task<HttpResponseMessage> StartSessionAsync([FromUri] Guid participantId,
                                                                [FromUri] long sessionId)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));
            
            await sessionActor.StartAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/end", Name = "EndSession")]
        public async Task<HttpResponseMessage> EndSessionAsync([FromUri] Guid participantId,
                                                               [FromUri] long sessionId)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));

            await sessionActor.EndAsync();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private static bool IsNotPresenter(Guid participantId, SessionDetails details)
        {
            return details.Presenter.GetActorId().GetGuidId() != participantId;
        }

        private static bool IsPresenter(Guid participantId, SessionDetails details)
        {
            return details.Presenter.GetActorId().GetGuidId() == participantId;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}", Name = "Session")]
        public async Task<HttpResponseMessage> GetSessionAsync([FromUri] Guid participantId,
                                                               [FromUri] long sessionId)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));

            var sessionDetails = await sessionActor.GetDetailsAsync();

            var presenter = await sessionDetails.Presenter.GetDetailsAsync();

            var r = Representation.Make();

            r.AddValue("name", sessionDetails.Name);
            r.AddValue("attendeeCount", sessionDetails.AttendeeCount);
            r.AddValue("end", sessionDetails.EndDateTime.ToString("F"));
            r.AddValue("start", sessionDetails.StartDateTime.ToString("F"));
            r.AddValue("presenter", presenter.Name);

            if (IsPresenter(participantId, sessionDetails))
            {
                var questions = await sessionActor.GetQuestionsAsync();

                var questionRepresentations = new List<JObject>();

                foreach (var questionActor in questions)
                {
                    var isAnswered = await questionActor.GetIsAnsweredStateAsync();
                    if (isAnswered)
                    {
                        var questionDetails = await questionActor.GetDetailsAsync();
                        var q = Representation.Make();
                        q.AddValue("question", questionDetails.Content);
                        q.AddLink("answer-question", Url.Route("PostAnswer", new { participantId = participantId, sessionId = sessionId, questionId = questionActor.GetActorId().GetLongId() }));
                        questionRepresentations.Add(q.Json());
                    }
                }

                r.AddArray("questions", questionRepresentations.ToArray());
            }

            r.AddLink("session-transcript", Url.Route("Transcript", new { participantId = participantId, sessionId = sessionId }));
            r.AddLink("list-sessions", Url.Route("ListSessions", new { participantId = participantId }));

            return Request.CreateResponse(HttpStatusCode.OK, r.Json());
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/transcript", Name = "Transcript")]
        public async Task<HttpResponseMessage> GetTranscriptAsync([FromUri] Guid participantId,
                                                                  [FromUri] long sessionId)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));
            var sessionDetails = await sessionActor.GetDetailsAsync();

            var r = Representation.Make();

            r.AddValue("name", sessionDetails.Name);
            r.AddValue("attendeeCount", sessionDetails.AttendeeCount);
            r.AddValue("end", sessionDetails.EndDateTime.ToString("F"));
            r.AddValue("start", sessionDetails.StartDateTime.ToString("F"));

            var presenterDetails = await sessionDetails.Presenter.GetDetailsAsync();

            r.AddValue("presenter", presenterDetails.Name);

            var transcriptActor = ActorProxy.Create<ITranscriptViewActor>(new ActorId(participantId));

            var entries = await transcriptActor.GetEntriesAsync(sessionId);

            r.AddArray("events", entries.Select(e => e).ToArray());

            r.AddLink("session-transcript", Url.Route("Transcript", new { participantId = participantId, sessionId = sessionId }));
            r.AddLink("ask-question", Url.Route("PostQuestion", new { participantId = participantId, sessionId = sessionId }));

            r.AddLink("list-sessions", Url.Route("ListSessions", new { participantId = participantId }));

            return Request.CreateResponse(HttpStatusCode.OK, r.Json());
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/questions", Name = "PostQuestion")]
        public async Task<HttpResponseMessage> PostQuestionAsync([FromUri] Guid participantId,
                                                                 [FromUri] long sessionId,
                                                                 [FromBody] Models.Question question)
        {
            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));

            var participantActor = ActorProxy.Create<IParticipantActor>(new ActorId(participantId));

            var questionActor = ActorProxy.Create<IQuestionActor>(ActorId.CreateRandom());

            await questionActor.SetContentAsync(participantActor, question.Content);

            await sessionActor.AddQuestionAsync(questionActor);

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/questions/{questionId:long}", Name = "PostAnswer")]
        public async Task<HttpResponseMessage> PostAnswerAsync([FromUri] Guid participantId,
                                                               [FromUri] long sessionId,
                                                               [FromUri] long questionId,
                                                               [FromBody] Models.Answer answer)
        {
            var participantActor = ActorProxy.Create<IParticipantActor>(new ActorId(participantId));

            var questionActor = ActorProxy.Create<IQuestionActor>(new ActorId(questionId));

            await questionActor.SetAnswerAsync(participantActor, answer.Content);

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("{participantId:guid}/sessions/{sessionId:long}/questions/{questionId:long}", Name = "GetQuestion")]
        public async Task<HttpResponseMessage> GetQuestionAsync([FromUri] Guid participantId,
                                                                [FromUri] long sessionId,
                                                                [FromUri] long questionId)
        {
            var questionActor = ActorProxy.Create<IQuestionActor>(new ActorId(questionId));

            var questionDetails = await questionActor.GetDetailsAsync();

            var r = Representation.Make();

            r.AddValue("question", questionDetails.Content);

            var participant = await questionDetails.AskedByParticipant.GetDetailsAsync();

            r.AddValue("askedBy", participant.Name);

            r.AddValue("answer", questionDetails.Answer);

            r.AddValue("asked", questionDetails.AskDateTime.ToString("F"));

            r.AddValue("answered", questionDetails.AnswerDateTime.ToString("F"));

            if (string.IsNullOrWhiteSpace(questionDetails.Answer))
                r.AddLink("answer-question", Url.Route("PostAnswer", new { participantId = participantId, sessionId = sessionId, questionId = questionId }));

            var sessionActor = ActorProxy.Create<ISessionActor>(new ActorId(sessionId));
            var sessionDetails = await sessionActor.GetDetailsAsync();

            if (IsPresenter(participantId, sessionDetails))
                r.AddLink("session", Url.Route("Session", new { participantId = participantId, sessionId = sessionId }));

            r.AddLink("session-transcript", Url.Route("Transcript", new { participantId = participantId, sessionId = sessionId }));

            return Request.CreateResponse(HttpStatusCode.OK, r.Json());
        }
    }
}