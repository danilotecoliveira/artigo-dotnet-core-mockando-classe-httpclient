using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpClientMock
{
public class ExternalService
{
    private readonly HttpClient _httpClient;
    private const string _baseUrl = "https://codigomaromba.com/";
    private readonly ILogger<ExternalService> _logger;

    public ExternalService(
        HttpClient httpClient, 
        ILogger<ExternalService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{_baseUrl}endpoint-path").ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("A chamada retornou uma resposta inesperada.");

                return Enumerable.Empty<string>();
            }

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<IEnumerable<string>>(content);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao realizar a chamada.");

            throw new ArgumentOutOfRangeException(ex.Message, new ApplicationException());
        }
    }
}
}
