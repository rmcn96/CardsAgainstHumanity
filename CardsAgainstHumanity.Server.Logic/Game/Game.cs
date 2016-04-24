﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardsAgainstHumanity.Server.Data;
using CardsAgainstHumanity.Shared.Extensions;

namespace CardsAgainstHumanity.Server.Logic.Game
{
    public class Game
    {
        private readonly ICardService _cardService;

        public Game(ICardService cardService)
        {
            _cardService = cardService;
            GameId = Guid.NewGuid();
        }
        public Guid GameId { get; }

        public IList<Guid> AvailableWhiteCards { get; private set; } 
        public IList<Guid> AvailableBlackCards { get; private set; }
        public bool IsRunning { get; private set; }
        public List<Guid> Players { get; private set; } 

        public List<Round> Rounds { get; private set; }  

        public async Task Setup()
        {
            AvailableWhiteCards = (await _cardService.GetAllWhiteCards()).Select(k => k.Key).Shuffle().ToList();
            AvailableBlackCards = (await _cardService.GetAllBlackCards()).Select(k => k.Key).Shuffle().ToList();
            Rounds = new List<Round>();
            Players = new List<Guid>();
            IsRunning = true;
        }

        public void AddPlayer(Guid playerId)
        {
            //eh, a player can be added again, won't do anything
            if (Players.All(k => k != playerId))
            {
                Players.Add(playerId);
            }
        }

        public void RemovePlayer(Guid playerId)
        {
            if (Players.Contains(playerId))
            {
                Players.Remove(playerId);
            }
        }

        public Round CreateRound()
        {
            var blackCardId = AvailableBlackCards.FirstOrDefault();
            AvailableBlackCards.Remove(blackCardId);
            var round = new Round(_cardService, blackCardId, Players);
            Rounds.Add(round);
            if (AvailableBlackCards.Count == 0)
            {
                IsRunning = false;
            }
            return round;
        }

        public IDictionary<Guid, int> GetScores()
        {
            return Rounds.GroupBy(k => k.Winner).ToDictionary(k => k.Key, v => v.Count());
        } 
    }
}
