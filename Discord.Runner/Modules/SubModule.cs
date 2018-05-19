using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Backend;
using Discord.Backend.Enum;
using Discord.Backend.Helpers;
using Discord.Commands;

namespace Discord.Runner.Modules
{
    public class SubModule : ModuleBase
    {
        private readonly SubscribeManager _subscribeManager;

        public SubModule(SubscribeManager subscribeManager)
        {
            _subscribeManager = subscribeManager;
        }

        [Command("sub"), Summary("(sub <event nr>), Subscribes you to a DomainName event")]
        public async Task Sub(int eventNr)
        {
            var eventIndex = (Event) eventNr;
            if (!Enum.IsDefined(typeof(Event), eventIndex))
            {
                await ReplyAsync($"{eventIndex} is not found among the event ids, use `!events` to see all event");
                return;
            }

            _subscribeManager.Subscribe(Context.User.Id, eventIndex);
            await ReplyAsync($"You are now subscribed to {eventIndex.ToString()}");
        }

        [Command("UnSub"), Summary("(sub <event nr>), Subscribes you to a DomainName event")]
        public async Task UnSub(int eventNr)
        {
            var eventIndex = (Event) eventNr;
            if (!Enum.IsDefined(typeof(Event), eventIndex))
            {
                await ReplyAsync($"{eventIndex} is not found among the event ids, use `!events` to see all event");
                return;
            }

            _subscribeManager.UnSubscribe(Context.User.Id, eventIndex);
            await ReplyAsync($"You are now UnSubscribed to {eventIndex.ToString()}");
        }

        [Command("suball"), Summary("(suball, Subscribes you to a ALL DomainName event")]
        public async Task SubAll()
        {
            var allEvents = EnumUtil.GetValues<Event>().Select(e => (int) e);
            _subscribeManager.Subscribe(Context.User.Id, allEvents);
            await ReplyAsync($"You are now subscribed to ALL events");
        }

        [Command("unsuball"), Summary("(unsuball, UnSubscribes you to a ALL DomainName event")]
        public async Task UnSubAll()
        {
            var allEvents = EnumUtil.GetValues<Event>().Select(e => (int) e);
            _subscribeManager.UnSubscribe(Context.User.Id, allEvents);
            await ReplyAsync("You are now UNsubscribed to ALL events");
        }

        [Command("events"), Summary("List all events")]
        public async Task ListEvents()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are all events"
            };

            var values = EnumUtil.GetValues<Event>().Select(x => new EnumUtil.Item(x)).ToList();

            builder.AddField(x =>
            {
                x.Name = "Events";
                x.Value = string.Join("\n", values);
                x.IsInline = false;
            });

            await ReplyAsync("", embed: builder.Build());
        }
    }
}