using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Wrappers.Interfaces;
using Microsoft.AspNetCore.Hosting;

using RpcCommunicationTicket;
using Exception = System.Exception;

namespace Betting.Backend.Services.Impl
{
    public class TicketService : ITicketService
    {
        private readonly IGmailService               _gmailService;
        private readonly ITicketServiceClientWrapper _ticketServiceClient;
        private readonly IDiscordService             _discordService;
        private readonly ITicketHubConnections       _ticketHubConnections;
        private readonly bool                        _isDev;

        public TicketService
        (
            IGrpcServiceFactory grpcServiceFactory,
            IGmailService gmailService,
            IDiscordService discordService,
            ITicketHubConnections ticketHubConnections,
            IHostingEnvironment env
        )
        {
            _isDev = env.EnvironmentName.Contains("Dev");
            _gmailService = gmailService;
            _ticketServiceClient = grpcServiceFactory.GetTicketSercviceClient();
            _discordService = discordService;
            _ticketHubConnections = ticketHubConnections;
        }

        public async Task<SingleTicketResponse> UserCreateTicket(UserCreateTicketRequest req)
        {
            var res = await _ticketServiceClient.UserCreateTicket(req);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Ticket) await SendUserCreatedTicketEmailToSupport(req, res.Ticket.TicketId);

            return res;
        }

        public async Task<SingleTicketResponse> UserRespondToTicket(UserRespondToTicketRequest req)
        {
            var res = await _ticketServiceClient.UserRespondToTicket(req);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Ticket) await SendUserResponedOnTicketEmailToSupport(req);

            return res;
        }

        public async Task<ListTicketsResponse> UserGetAllTickets(string steamId)
        {
            return await _ticketServiceClient.UserGetAllTickets(new Steamid
            {
                SteamId = steamId
            });
        }

        public async Task<UserCountUnreadTicketsResponse> UserCountUnreadTickets(string steamId)
        {
            return await _ticketServiceClient.UserCountUnreadTickets(new Steamid
            {
                SteamId = steamId
            });
        }

        public async Task<SingleTicketResponse> UserMarkTicketAsRead(string steamId, string ticketId)
        {
            return await _ticketServiceClient.UserMarkTicketAsRead(new UserMarkTicketAsReadRequest
            {
                SteamId = steamId,
                TicketId = ticketId
            });
        }

        public async Task<SingleTicketResponse> AdminCreateTicket(AdminCreateTicketRequest req)
        {
            var res = await _ticketServiceClient.AdminCreateTicket(req);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Ticket)
            {
                await _ticketHubConnections.TicketUpdate(res.Ticket);
                 SendDiscordadminCreatedTicketMessageToUser(res.Ticket);
            }

            return res;
        }

        public async Task<SingleTicketResponse> AdminRespondToTicket(AdminRespondToTicketRequest req)
        {
            var res = await _ticketServiceClient.AdminRespondToTicket(req);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Ticket)
            {
                await _ticketHubConnections.TicketUpdate(res.Ticket);
                 SendDiscordMessageToUser(res.Ticket);
            }

            return res;
        }

        public async Task<SingleTicketResponse> AdminChangeStatusOnTicket(AdminChangeStatusOnTicketRequest req)
        {
            var res = await _ticketServiceClient.AdminChangeStatusOnTicket(req);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Ticket)
            {
                await _ticketHubConnections.TicketUpdate(res.Ticket);
                 SendDiscordStatusChangedMessageToUser(res.Ticket);
            }

            return res;
        }


        public async Task<ListTicketsResponse> AdminGetTicketsOnQuery(AdminGetTicketsOnQueryRequest req)
        {
            return await _ticketServiceClient.AdminGetTicketsOnQuery(req);
        }


        private void SendDiscordMessageToUser(Ticket ticketToSteamId)
        {
            const string str = "A DomainName Support staff have answered yor ticket. Check it out here <LINK TO THAT TICKET!>!";
            SendDicordMessage(str, ticketToSteamId.SteamId);
        }

        private void SendDiscordStatusChangedMessageToUser(Ticket ticketToSteamId)
        {
            const string str = "A DomainName Support staff have changed status on one of yor ticket. Check it out here <LINK TO THAT TICKET!>!";
            SendDicordMessage(str, ticketToSteamId.SteamId);
        }

        private void SendDiscordadminCreatedTicketMessageToUser(Ticket ticketToSteamId)
        {
            const string str = "A DomainName Support staff have created a ticket for you. Check it out here <LINK TO THAT TICKET!>!";
            SendDicordMessage(str, ticketToSteamId.SteamId);
        }

        private void SendDicordMessage(string str, string steamId)
        {
            try
            {
                _discordService.SendPersonalMessageAsync(str, steamId);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async Task SendUserCreatedTicketEmailToSupport(UserCreateTicketRequest req, string ticketId)
        {
            var title = $"New ticket: {req.Title} - {req.SteamId} / {req.Message.Name} : id{ticketId}";
            var body = req.Message.MessageBody;
            await SendEmailToSupport(body, title);
        }

        private async Task SendUserResponedOnTicketEmailToSupport(UserRespondToTicketRequest req)
        {
            var title = $"User responed on : {req.SteamId} - {req.TicketId}";
            var body = req.Message.MessageBody;
            await SendEmailToSupport(body, title);
        }

        private async Task SendEmailToSupport(string body, string title)
        {
            if (_isDev) return;

            await _gmailService.SendEmail(new List<MailAddress>
                {
                    new MailAddress("isak454@hotmail.com"),
                    new MailAddress("robin.Edbom@gmail.com")
                }, body, title
            );
        }
    }
}