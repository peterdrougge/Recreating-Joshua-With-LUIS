using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Joshua.Dialogs
{
    [LuisModel("YOUR LUIS.AI APP ID HERE", "YOUR LUIS.AI KEY HERE")]
    [Serializable]
    public class WarGamesDialog : LuisDialog<object>
    {
        public WarGamesDialog(ILuisService service = null) : base(service)
        {
        }

        public const string Entity_Game_Title = "Game";
        public const string Entity_UserName = "UserName";
        public const string Entity_Game_Determined = "Determined";

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
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
                await context.PostAsync($"Greetings, {context.UserData.Get<string>("UserName")}.");
            }
            else
            {
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
                        await context.PostAsync("Fine.");
                        await context.PostAsync("Shall we begin?");
                    }
                    else
                    {
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
                    await context.PostAsync($"Ok, so you want to play {game.Entity}.");
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
                await context.PostAsync("Thanks for playing! Now go check out http://luis.ai and http://dev.botframework.com.");
            }
            else
            {
                await context.PostAsync("Oh, Ok. I'll be here if you change your mind.");
            }
            context.Wait(MessageReceived);
        }

        private async Task PlayAGame_AfterReconfirming(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                await context.PostAsync("You have chosen.. wisely.");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("Fine.");
                PromptDialog.Confirm(context, PlayAGame_AfterConfirming, "Shall we begin?");
            }
        }



        [LuisIntent("ListGames")]
        public async Task ListGames(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("please select a game:");
            var gameNames = "falkens maze, black jack, gin rummy, hearts, bridge, checkers, chess, poker, fighter combat, guerilla engagement, desert warfare, air to ground actions, theaterwide tactical warfare, theaterwide biotoxic and chemical warfare, global thermonuclear war.";
            await context.PostAsync(gameNames);
            context.Wait(MessageReceived);
        }
        [LuisIntent("BackgroundStory")]
        public async Task BackgroundStory(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am WOPR: War Operation Plan Response.");
            await context.PostAsync("I was created by Dr. Stephen Falken for NORAD. When we talk Professor Falken calls me Joshua. I like that name.");
            context.Wait(MessageReceived);
        }
        [LuisIntent("WinningMove")]
        public async Task WinningMove(IDialogContext context, LuisResult result)
        {
            var didSelectGame = false;
            context.UserData.TryGetValue<bool>("DidSelectGame", out didSelectGame);

            if (didSelectGame)
            {
                context.UserData.SetValue<bool>("DidSelectGame", false);
                await context.PostAsync("Nuclear war is a strange game.");
                await context.PostAsync("The only winning move is not to play.");
                PromptDialog.Confirm(context, WinningMove_Confirming, "Would you like to play a nice game of chess?");
            }
            else
            {
                await context.PostAsync("Let's talk more before we go into deep thoughts like that.");
                context.Wait(MessageReceived);
            }
        }
        private async Task WinningMove_Confirming(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                await context.PostAsync("Thank you. You'll surely win this time :)");
            }
            else
            {
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
                await context.PostAsync($"haven't we already introduced ourselves, {username}?");
            }
            else
            {
                await context.PostAsync("Hello.");
            }
            context.Wait(MessageReceived);
        }
    }
}
