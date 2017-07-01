using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using StockBot.Stockbot;
using System;
using Newtonsoft.Json;
using System.Linq;

namespace StockBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private async Task<string> GetStock(string StockSymbol)
        {
            double? dblStockValue = await YahooBot.GetStockRateAsync(StockSymbol);
            if (dblStockValue == null)
            {
                return string.Format("This \"{0}\" is not an valid stock symbol", StockSymbol);
            }
            else
            {
                return string.Format("Stock Price of {0} is {1}", StockSymbol, dblStockValue);
            }
        }
        private static async Task<Rootobject> GetEntityFromLUIS(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            Rootobject Data = new Rootobject();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/99bbe58a-6560-4cf3-bc26-4f73a0fcbcc6?subscription-key=567c3926939e41eab54ebc7be7c331e2&timezoneOffset=-480&verbose=true&q=" + Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<Rootobject>(JsonDataResponse);
                }
            }
            return Data;
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        //public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        //{
        //    if (activity.Type == "Message")
        //    {
        //        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
        //        string StockRateString;
        //        Rootobject StLUIS = await GetEntityFromLUIS(activity.Text);
        //        if (StLUIS.intents.Length > 0)
        //        {
        //            switch (StLUIS.intents[0].intent)
        //            {
        //                case "StockPrice":
        //                    StockRateString = await GetStock(StLUIS.entities[0].entity);
        //                    break;
        //                case "StockPrice2":
        //                    StockRateString = await GetStock(StLUIS.entities[0].entity);
        //                    break;
        //                default:
        //                    StockRateString = "Sorry, I am not getting you...";
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            StockRateString = "Sorry, I am not getting you...";
        //        }
        //        Activity reply = activity.CreateReply(StockRateString);
        //        await connector.Conversations.ReplyToActivityAsync(reply);
        //        // return our reply to the user  
        //        //return activity.CreateReply(StockRateString);
        //    }
        //    else
        //    {
        //        await this.HandleSystemMessage(activity);
        //    }
        //    var response = this.Request.CreateResponse(HttpStatusCode.OK);
        //    return response;
        //}
        public async Task<HttpResponseMessage> Post([FromBody]Microsoft.Bot.Connector.Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                string StockRateString;
                Rootobject StLUIS = await GetEntityFromLUIS(activity.Text);
                if (StLUIS.intents.Length > 0)
                {
                    switch (StLUIS.intents[0].intent)
                    {
                        case "StockPrice":
                            StockRateString = await GetStock(StLUIS.entities[0].entity);
                            break;
                        case "StockPrice2":
                            StockRateString = await GetStock(StLUIS.entities[0].entity);
                            break;
                        default:
                            StockRateString = "Sorry, I am not getting you...";
                            break;
                    }
                }
                else
                {
                    StockRateString = "Sorry, I am not getting you...";
                }
                // activity.CreateReply(StockRateString);
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply(StockRateString);
                await connector.Conversations.ReplyToActivityAsync(reply);
               // await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                await this.HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private async Task<Activity> HandleSystemMessage(Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                    break;
                case ActivityTypes.ConversationUpdate:
                    // Greet the user the first time the bot is added to a conversation.
                    if (activity.MembersAdded.Any(m => m.Id == activity.Recipient.Id))
                    {
                        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                        var response = activity.CreateReply();
                        response.Text = "Hi! I am your StockRate Bot. </br> I can understand your questions to know stock price for different items. </br>Try writing me questions like: </br> where does msft stand </br> how about google </br>what is tcs doing, etc. </br> You can refer the yahoo finance site for valid stock symbols. ";

                        await connector.Conversations.ReplyToActivityAsync(response);
                    }

                    break;
                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    break;
                case ActivityTypes.Typing:
                    // Handle knowing that the user is typing
                    break;
                case ActivityTypes.Ping:
                    break;
            }

            return null;
        }
        //private Activity HandleSystemMessage(Activity message)
        //{
        //    if (message.Type == ActivityTypes.DeleteUserData)
        //    {
        //        // Implement user deletion here
        //        // If we handle user deletion, return a real message
        //    }
        //    else if (message.Type == ActivityTypes.ConversationUpdate)
        //    {
        //        // Handle conversation state changes, like members being added and removed
        //        // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
        //        // Not available in all channels
        //    }
        //    else if (message.Type == ActivityTypes.ContactRelationUpdate)
        //    {
        //        // Handle add/remove from contact lists
        //        // Activity.From + Activity.Action represent what happened
        //    }
        //    else if (message.Type == ActivityTypes.Typing)
        //    {
        //        // Handle knowing tha the user is typing
        //    }
        //    else if (message.Type == ActivityTypes.Ping)
        //    {
        //    }

        //    return null;
        //}
    }
}