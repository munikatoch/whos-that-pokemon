using Serilog.Events;
using WhosThatPokemon.Interfaces.Logger;
using WhosThatPokemon.Interfaces.Service;

namespace WhosThatPokemon.Services.Common
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAppLogger _appLogger;

        public HttpHelper(IHttpClientFactory clientFactory, IAppLogger appLogger)
        {
            _clientFactory = clientFactory;
            _appLogger = appLogger;
        }

        public async Task<byte[]?> GetImageContent(string url, string type)
        {
            byte[]? content = Array.Empty<byte>();

            if (ValidateAndParseUrl(url, out Uri? uri))
            {
                try
                {
                    using HttpClient client = _clientFactory.CreateClient(type);
                    using HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.Content.Headers.ContentType != null)
                        {
                            string contentType = response.Content.Headers.ContentType.ToString();
                            if (contentType.StartsWith("image"))
                            {
                                content = await response.Content.ReadAsByteArrayAsync();
                            }
                        }
                        else
                        {
                            await _appLogger.FileLogAsync(response, LogEventLevel.Error).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await _appLogger.FileLogAsync(response, LogEventLevel.Error).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _appLogger.ExceptionLogAsync($"HttpHelper: GetImageContent {url}", ex).ConfigureAwait(false);
                }
            }
            return content;
        }

        private static bool ValidateAndParseUrl(string url, out Uri? uri)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out uri) && uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
