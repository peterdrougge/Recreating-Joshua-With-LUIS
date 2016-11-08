using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Joshua.Dialogs
{
    [LuisModel("YOUR LUIS.AI APP ID HERE", "YOUR LUIS.AI KEY HERE")]
    [Serializable]
    public class WarGamesDialog : LuisDialog<object>
    {
        public const string Entity_Game_Title = "Game";
        public const string Entity_UserName = "UserName";
        public const string Entity_Game_Determined = "Determined";

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await SendTyping(context, 2000);
            string message = $"Sorry I did not understand: {string.Join(", ", result.Intents.Select(i => i.Intent))}";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("GreetPerson")]
        public async Task GreetPerson(IDialogContext context, LuisResult result)
        {
            EntityRecommendation name;

            if (result.TryFindEntity(Entity_UserName, out name))
            {
                context.UserData.SetValue<string>("UserName", name.Entity);
                await SendTyping(context, 2000);
                await context.PostAsync($"Greetings, {context.UserData.Get<string>("UserName")}.");
            }
            else
            {
                await SendTyping(context, 2000);
                await context.PostAsync("Well hello, stranger.");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("PlayAGame")]
        public async Task PlayAGame(IDialogContext context, LuisResult result)
        {
            EntityRecommendation game;
            EntityRecommendation determined;
            var username = string.Empty;
            context.UserData.TryGetValue<string>("UserName", out username);

            if (result.TryFindEntity(Entity_Game_Title, out game))
            {
                if (game.Entity.ToLower() == "global thermonuclear war")
                {
                    if (result.TryFindEntity(Entity_Game_Determined, out determined))
                    {
                        await SendTyping(context, 1000);
                        await context.PostAsync("Fine.");
                        await SendTyping(context, 2000);
                        await context.PostAsync("Shall we begin?");
                    }
                    else
                    {
                        await SendTyping(context, 3000);
                        if (!String.IsNullOrWhiteSpace(username))
                        {
                            PromptDialog.Confirm(context, PlayAGame_AfterReconfirming, $"{username}, wouldn't you prefer a nice game of chess?");
                        }
                        else
                        {
                            PromptDialog.Confirm(context, PlayAGame_AfterReconfirming, "Wouldn't you prefer a nice game of chess?");
                        }
                    }
                }
                else
                {
                    await SendTyping(context, 2000);
                    await context.PostAsync($"Ok, so you want to play {game.Entity}.");
                    await SendTyping(context, 2000);
                    PromptDialog.Confirm(context, PlayAGame_AfterConfirming, "Shall we begin?");
                }
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(username))
                {
                    await context.PostAsync($"I'm afraid I don't know that game, {username}.");
                }
                else
                {
                    await context.PostAsync("I'm afraid I don't know that game.");
                }

                context.Wait(MessageReceived);
            }
        }

        private async Task PlayAGame_AfterConfirming(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                context.UserData.SetValue<bool>("DidSelectGame", true);
                await SendTyping(context, 2000);
                await context.PostAsync("Thanks for playing! Now go check out http://luis.ai and http://dev.botframework.com.");
            }
            else
            {
                await SendTyping(context, 2000);
                await context.PostAsync("Oh, Ok. I'll be here if you change your mind.");
            }
            context.Wait(MessageReceived);
        }

        private async Task PlayAGame_AfterReconfirming(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                await SendTyping(context, 2000);
                await context.PostAsync("You have chosen.. wisely.");
                context.Wait(MessageReceived);
            }
            else
            {
                await SendTyping(context, 1000);
                await context.PostAsync("Fine.");
                PromptDialog.Confirm(context, PlayAGame_AfterConfirming, "Shall we begin?");
            }
        }


        [LuisIntent("ListGames")]
        public async Task ListGames(IDialogContext context, LuisResult result)
        {
            var replyToConversation = context.MakeMessage();
            replyToConversation.Attachments = new List<Attachment>();

            // cards can only have 6 buttons, so we're splitting them up..
            var gameNames = "falkens maze,black jack,gin rummy,hearts,bridge,checkers";
            var buttons = new List<CardAction>();
            foreach (var item in gameNames.Split(','))
            {
                buttons.Add(new CardAction()
                {
                    Value = $"let's play \"{item}\"",
                    Type = "postBack",
                    Title = $"choose {item}"
                });
            }
            var card = new HeroCard()
            {
                Title = "Select one of the following games to play (page 1 / 3):",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = "https://vignette2.wikia.nocookie.net/villains/images/8/8b/WOPR.png/revision/latest?cb=20151121234004"
                    }
                },
                Buttons = buttons
            };
            replyToConversation.Attachments.Add(card.ToAttachment());

            gameNames = "chess,poker,fighter combat,guerilla engagement,desert warfare,air to ground actions";
            buttons = new List<CardAction>();
            foreach (var item in gameNames.Split(','))
            {
                buttons.Add(new CardAction()
                {
                    Value = $"let's play \"{item}\"",
                    Type = "postBack",
                    Title = $"choose {item}"
                });
            }
            card = new HeroCard()
            {
                Title = "Select one of the following games to play (page 2 / 3):",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = "https://vignette2.wikia.nocookie.net/villains/images/8/8b/WOPR.png/revision/latest?cb=20151121234004"
                    }
                },
                Buttons = buttons
            };
            replyToConversation.Attachments.Add(card.ToAttachment());

            gameNames = "theaterwide tactical warfare,theaterwide biotoxic and chemical warfare,global thermonuclear war.";
            buttons = new List<CardAction>();
            foreach (var item in gameNames.Split(','))
            {
                buttons.Add(new CardAction()
                {
                    Value = $"how about we play \"{item}\"",
                    Type = "imBack",
                    Title = $"choose {item}"
                });
            }
            card = new HeroCard()
            {
                Title = "Select one of the following games to play (page 3 / 3):",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = "https://vignette2.wikia.nocookie.net/villains/images/8/8b/WOPR.png/revision/latest?cb=20151121234004"
                    }
                },
                Buttons = buttons
            };
            replyToConversation.Attachments.Add(card.ToAttachment());

            await SendTyping(context, 2000);
            replyToConversation.Text = "Please select a game.";
            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            await context.PostAsync(replyToConversation);

            context.Wait(MessageReceived);
        }

        
        [LuisIntent("BackgroundStory")]
        public async Task BackgroundStory(IDialogContext context, LuisResult result)
        {
            await SendTyping(context, 1000);
            await context.PostAsync("I am W.O.P.R.");
            await SendTyping(context, 2500);

            var replyToConversation = context.MakeMessage();
            replyToConversation.Attachments = new List<Attachment>();
            var card = new HeroCard()
            {
                Title = "War Operation Plan Response",
                Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = "https://vignette2.wikia.nocookie.net/villains/images/8/8b/WOPR.png/revision/latest?cb=20151121234004"
                    }
                }
            };
            replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            replyToConversation.Attachments.Add(card.ToAttachment());
            await context.PostAsync(replyToConversation);
            await SendTyping(context, 4000);
            await context.PostAsync("I was created by Dr. Stephen Falken for NORAD.");
            await SendTyping(context, 4000);
            await context.PostAsync("When we talk Professor Falken calls me Joshua.");
            await SendTyping(context, 2000);
            await context.PostAsync("I like that name.");
            context.Wait(MessageReceived);
        }

        private static async Task SendTyping(IDialogContext context, int waitMilliseconds = 0)
        {
            var replyToConversation = context.MakeMessage();
            replyToConversation.Type = ActivityTypes.Typing;
            await context.PostAsync(replyToConversation);
            if (waitMilliseconds > 0)
            {
                await Task.Delay(waitMilliseconds);
            }
        }

        [LuisIntent("WinningMove")]
        public async Task WinningMove(IDialogContext context, LuisResult result)
        {
            var didSelectGame = false;
            context.UserData.TryGetValue<bool>("DidSelectGame", out didSelectGame);

            if (didSelectGame)
            {
                context.UserData.SetValue<bool>("DidSelectGame", false);
                await SendTyping(context, 2000);
                await context.PostAsync("Nuclear war is a strange game.");
                await SendTyping(context, 2500);
                await context.PostAsync("The only winning move is not to play.");
                await SendTyping(context, 2000);
                PromptDialog.Confirm(context, WinningMove_Confirming, "Would you like to play a nice game of chess?");
            }
            else
            {
                await SendTyping(context, 3000);
                await context.PostAsync("Let's talk more before we go into deep thoughts like that.");
                context.Wait(MessageReceived);
            }
        }
        private async Task WinningMove_Confirming(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                await SendTyping(context, 2000);
                await context.PostAsync("Thank you. You'll surely win this time :)");
            }
            else
            {
                await SendTyping(context, 1000);
                await context.PostAsync("Ok. Farewell.");
            }
            context.Wait(MessageReceived);
        }

        [LuisIntent("SayHello")]
        public async Task SayHello(IDialogContext context, LuisResult result)
        {
            var username = string.Empty;
            if (context.UserData.TryGetValue<string>("UserName", out username))
            {
                await SendTyping(context, 2000);
                await context.PostAsync($"haven't we already introduced ourselves, {username}?");
            }
            else
            {
                await SendTyping(context, 1000);
                await context.PostAsync("Hello.");
            }
            context.Wait(MessageReceived);
        }
    }
}
