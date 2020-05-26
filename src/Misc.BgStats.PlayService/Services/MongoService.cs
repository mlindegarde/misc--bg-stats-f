﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Misc.BgStats.PlayService.Model;
using MongoDB.Driver;
using Serilog;

namespace Misc.BgStats.PlayService.Services
{
    public class MongoService
    {
        #region Constants
        private const string Database = "board-game-stats";
        private const string BoardGameCollection = "board-games";
        private const string BoardGameStatusCollection = "board-game-status";
        private const string PlayCollection = "plays";
        #endregion

        #region Member Variables
        private readonly MongoClient _client;
        private readonly ILogger _logger;
        #endregion

        #region Constructor
        public MongoService(ILogger logger)
        {
            _client = new MongoClient();
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<List<BoardGame>> GetBoardListAsync(CancellationToken cancellationToken)
        {
            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<BoardGame> collection = database.GetCollection<BoardGame>(BoardGameCollection);

            return await collection.Find(x => true).ToListAsync(cancellationToken);
        }

        public async Task<long> GetPlayCountAsync(int id, CancellationToken cancellationToken)
        {
            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<Play> collection = database.GetCollection<Play>(PlayCollection);

            return await collection.CountDocumentsAsync(x => x.ObjectId == id, new CountOptions(), cancellationToken);
        }

        public async Task InsertPlaysAsync(List<Play> plays, CancellationToken cancellationToken)
        {
            if (plays?.Any() != true)
            {
                _logger.Warning("Attempted to log an empty play list");
                return;
            }

            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<Play> collection = database.GetCollection<Play>(PlayCollection);

            _logger.Verbose("Inserting {Count} plays for ObjectId {ObjectID}", plays.Count, plays[0].ObjectId);
            //await collection.InsertManyAsync(plays, null, cancellationToken);

            foreach (Play play in plays)
            {
                await collection.ReplaceOneAsync(
                    x => x.Id == play.Id, play,
                    new ReplaceOptions { IsUpsert = true },
                    cancellationToken);
            }
        }

        public async Task UpsertPlaysAsync(List<Play> plays, CancellationToken cancellationToken)
        {
            if (plays?.Any() != true)
            {
                _logger.Warning("Attempted to log an empty play list");
                return;
            }

            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<Play> collection = database.GetCollection<Play>(PlayCollection);

            _logger.Verbose("Upserting {Count} plays for ObjectId {ObjectID}", plays.Count, plays[0].ObjectId);

            foreach (Play play in plays)
            {
                await collection.ReplaceOneAsync(
                    x => x.Id == play.Id, play, 
                    new ReplaceOptions {IsUpsert = true}, 
                    cancellationToken);
            }
        }

        public async Task DeletePlaysFor(int id, CancellationToken cancellationToken)
        {
            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<Play> collection = database.GetCollection<Play>(PlayCollection);

            _logger.Verbose("Removing all plays for {ObjectId}", id);
            await collection.DeleteManyAsync(x => x.ObjectId == id, cancellationToken);
        }

        public async Task<BoardGameStatus> GetBoardGameStatusAsync(int id, CancellationToken cancellationToken)
        {
            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<BoardGameStatus> collection = database.GetCollection<BoardGameStatus>(BoardGameStatusCollection);

            return await collection.Find(x => x.ObjectId == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpsertBoardGameStatusAsync(BoardGameStatus status, CancellationToken cancellationToken)
        {
            if (status == null)
            {
                _logger.Warning("Attempted to upsert a null status");
                return;
            }

            IMongoDatabase database = _client.GetDatabase(Database);
            IMongoCollection<BoardGameStatus> collection = database.GetCollection<BoardGameStatus>(BoardGameStatusCollection);

            _logger.Verbose("Upserting status for {ObjectId}", status.ObjectId);
            await collection.ReplaceOneAsync(
                x => x.ObjectId == status.ObjectId, 
                status, 
                new ReplaceOptions { IsUpsert = true },
                cancellationToken);
        }
        #endregion
    }
}