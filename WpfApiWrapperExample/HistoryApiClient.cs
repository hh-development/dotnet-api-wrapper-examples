using HHDev.DataManagement.ApiClientWrapper;
using HHDev.DataManagement.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WpfApiWrapperExample.Models;

namespace WpfApiWrapperExample
{
    /// <summary>
    /// <see cref="ApiClientV2"/> extended with the History endpoints, which the wrapper does not expose yet.
    /// The calls go through the wrapper's own <c>SendAsync</c>, so authentication, timeouts, logging and
    /// error reporting (<see cref="ApiResult"/>) behave exactly like every other wrapper call.
    /// </summary>
    public class HistoryApiClient : ApiClientV2
    {
        public HistoryApiClient(AuthenticationManager authenticationManager) : base(authenticationManager)
        {
        }

        /// <summary>
        /// Gets the change history of a setup (GET /Setups/History/{id}), newest change first.
        /// </summary>
        public Task<ApiResult<IReadOnlyList<ApiHistoryEntryModel>>> GetSetupHistoryAsync(string accountId, string setupId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            return GetHistoryAsync("Setups", accountId, setupId, timeout, cancellationToken);
        }

        /// <summary>
        /// Gets the change history of any entity that exposes a History endpoint, e.g. "Setups", "RunSheets", "Events".
        /// </summary>
        /// <param name="controller">The API controller name as shown in Swagger, e.g. "Setups".</param>
        public Task<ApiResult<IReadOnlyList<ApiHistoryEntryModel>>> GetHistoryAsync(string controller, string accountId, string id, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(controller);
            ArgumentException.ThrowIfNullOrEmpty(id);

            var url = $"{controller}/History/{Uri.EscapeDataString(id)}";

            return SendAsync<IReadOnlyList<ApiHistoryEntryModel>>(accountId, HttpMethod.Get, url, timeout, cancellationToken);
        }
    }
}
