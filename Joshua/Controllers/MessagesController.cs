using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Joshua.Dialogs;
using System.Net.Http;
using System.Diagnostics;

namespace Joshua
{
    //[BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity message)
        {
            if (message != null)
            {
                // one of these will have an interface and process it
                switch (message.GetActivityType())
                {
                    case ActivityTypes.Message:
                await Conversation.SendAsync(message, () => new WarGamesDialog());
                        break;
                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {message.GetActivityType()}");
                        break;
                }
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}