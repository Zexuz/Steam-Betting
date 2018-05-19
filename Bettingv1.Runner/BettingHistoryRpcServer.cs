using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bettingv1.Models;
using Grpc.Core;
using RpcCommunicationHistory;
using Shared.Shared;
using UserBet = RpcCommunicationHistory.UserBet;

namespace Bettingv1.Runner
{
    public class BettingHistoryRpcServer : Bettingv1HisotryService.Bettingv1HisotryServiceBase
    {
        private readonly MongoDbService _mongoDbService;

        public BettingHistoryRpcServer(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public override async Task<MatchResponse> GetGlobalHistory(GetGlobalHistoryRequest request, ServerCallContext context)
        {
            var res = await _mongoDbService.GetGlobalHistory(request);
            var matchHistory = RemapMatchHistories(res);

            return new MatchResponse
            {
                Data = {matchHistory},
                Pagaination = new PaginationResponse
                {
                    Offset = res.CurrentIndex,
                    Total = (int) res.Total,
                }
            };
        }

        public override async Task<MatchResponse> GetPersonalHistory(GetPersonalHistoryRequest request, ServerCallContext context)
        {
            var res = await _mongoDbService.GetPersonalHistory(request);
            var matchHistory = RemapMatchHistories(res);

            return new MatchResponse
            {
                Data = {matchHistory},
                Pagaination = new PaginationResponse
                {
                    Offset = res.CurrentIndex,
                    Total = (int) res.Total,
                }
            };
        }

        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse());
        }
        
        private static IEnumerable<MatchHistory> RemapMatchHistories(Result<MatchModel> res)
        {
            var matchHistory = res.Data.Select(m => new MatchHistory
            {
                RoundId = m.RoundId,
                Created = m.Created.ToBinary().ToString(),
                Value = (double) m.Value,
                Winner = new User
                {
                    ImageUrl = m.Winner.ImageUrl,
                    Name = m.Winner.Name,
                    SteamId = m.Winner.SteamId,
                },
                Items =
                {
                    m.Items.Select(i => new Item
                    {
                        ImageUrl = i.ImageUrl,
                        Name = i.Name,
                        Value = (double) i.Value
                    })
                },
                Bets =
                {
                    m.Bets.Select(b => new UserBet
                    {
                        Items =
                        {
                            m.Items.Select(i => new Item
                            {
                                ImageUrl = i.ImageUrl,
                                Name = i.Name,
                                Value = (double) i.Value
                            })
                        },
                        User = new User
                        {
                            ImageUrl = b.User.ImageUrl,
                            Name = b.User.Name,
                            SteamId = b.User.SteamId,
                        }
                    })
                }
            });
            return matchHistory;
        }
    }
}