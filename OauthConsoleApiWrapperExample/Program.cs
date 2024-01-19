using HHDev.DataManagement.ApiClientWrapper;
using HHDev.DataManagement.Api.Models;
using HHDev.DataManagement.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using HHDev.Core.Helpers;

namespace OauthConsoleApiWrapperExample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Running OAuth console HH DM API Wrapper example.");

            // The same AuthenticationSettings instance must be shared between OAuthAuthenticationManager and the AuthenticationManager given to the ApiClient instance.
            var sharedAuthSettings = new SimpleAuthenticationSettings
            {
                AuthenticationMode = eAuthenticationMode.OAuth
            };
            OAuthAuthenticationManager.AuthenticationSettings = sharedAuthSettings;
            var apiClient = new ApiClient(new AuthenticationManager(sharedAuthSettings));

            var getAccountsResult = await apiClient.GetAllAccounts(new ApiGetOptions());

            if (getAccountsResult.Success == false)
            {
                Console.WriteLine("Failed to get accounts");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Retrieved accounts:");
            Console.WriteLine(string.Join("\n", getAccountsResult.ReturnValue.Select(a => $"{a.Name}: {a.Id}")));
            Console.ReadKey();
        }
    }
}

