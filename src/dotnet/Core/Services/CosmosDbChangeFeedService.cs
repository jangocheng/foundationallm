﻿using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Chat;
using Microsoft.Azure.Cosmos.Fluent;
using System.Runtime;

namespace FoundationaLLM.Core.Services
{
    /// <inheritdoc/>
    public class CosmosDbChangeFeedService : ICosmosDbChangeFeedService
    {
        private readonly Database _database;
        private readonly Container _sessions;
        private readonly Container _leases;

        private ChangeFeedProcessor? _changeFeedProcessorProcessUserSessions;

        private readonly ILogger<CosmosDbChangeFeedService> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        private bool _changeFeedsInitialized = false;

        /// <summary>
        /// Gets a value indicating whether the change feeds have been initialized.
        /// </summary>
        public bool IsInitialized => _changeFeedsInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbChangeFeedService"/> class.
        /// </summary>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="CosmosDbChangeFeedService"/> type name.</param>
        /// <param name="cosmosDbService">Contains standard methods for managing data stored
        /// within the Azure Cosmos DB workspace.</param>
        /// <param name="settings">The <see cref="CosmosDbSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public CosmosDbChangeFeedService(ILogger<CosmosDbChangeFeedService> logger,
            ICosmosDbService cosmosDbService,
            IOptions<CosmosDbSettings> settings)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;

            CosmosSerializationOptions options = new()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };
            var client = new CosmosClientBuilder(settings.Value.Endpoint, settings.Value.Key)
                .WithSerializerOptions(options)
                .WithConnectionModeGateway()
                .Build();

            var database = client.GetDatabase(settings.Value.Database);

            _database = database ??
                        throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB database ({settings.Value.Database}).");
            _sessions = database?.GetContainer(CosmosDbContainers.Sessions) ??
                        throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.Sessions}).");
            _leases = database?.GetContainer(CosmosDbContainers.Leases) ??
                      throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.Leases}).");
        }

        /// <inheritdoc/>
        public async Task StartChangeFeedProcessorsAsync()
        {
            _logger.LogInformation("Starting Change Feed Processors...");
            try
            {
                _changeFeedProcessorProcessUserSessions = _sessions
                    .GetChangeFeedProcessorBuilder<Session>("ProcessUserSessions", ProcessUserSessionsChangeFeedHandler)
                    .WithInstanceName($"{Guid.NewGuid()}_ProcessUserSessions") // Prefix with a unique name to allow multiple instances to run at the same time.
                    .WithLeaseContainer(_leases)
                    .Build();

                await _changeFeedProcessorProcessUserSessions.StartAsync();

                _changeFeedsInitialized = true;
                _logger.LogInformation("Change Feed Processors started.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing change feed processors.");
            }
        }

        /// <inheritdoc/>
        public async Task StopChangeFeedProcessorAsync()
        {
            // Stop the ChangeFeedProcessor
            _logger.LogInformation("Stopping Change Feed Processors...");

            if (_changeFeedProcessorProcessUserSessions != null) await _changeFeedProcessorProcessUserSessions.StopAsync();

            _logger.LogInformation("Change Feed Processors stopped.");
        }

        private async Task ProcessUserSessionsChangeFeedHandler(
            ChangeFeedProcessorContext context,
            IReadOnlyCollection<Session> input,
            CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope("Cosmos DB Change Feed Processor: ProcessUserSessionsChangeFeedHandler");

            var sessions = input.Where(i => i.Type == nameof(Session)).ToArray();

            _logger.LogInformation("Cosmos DB Change Feed Processor: Processing {count} changes...", sessions.Count());

            await Parallel.ForEachAsync(sessions, cancellationToken, async (record, token) =>
            {
                try
                {
                    await _cosmosDbService.UpsertUserSessionAsync(record);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            });
        }
    }
}
