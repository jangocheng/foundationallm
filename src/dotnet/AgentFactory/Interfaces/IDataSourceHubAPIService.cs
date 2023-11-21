﻿using FoundationaLLM.AgentFactory.Core.Models.Messages;
namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service.
/// </summary>
public interface IDataSourceHubAPIService
{
    /// <summary>
    /// Gets the status of the DataSource Hub Service.
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Gets a list of DataSources from the DataSource Hub.
    /// </summary>
    /// <param name="sources">The data sources to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string sessionId);
}
