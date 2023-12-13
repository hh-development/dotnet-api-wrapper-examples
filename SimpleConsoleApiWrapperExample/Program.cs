using HHDev.DataManagement.ApiClientWrapper;
using HHDev.DataManagement.Api.Models;
using HHDev.DataManagement.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using HHDev.Core.Helpers;

namespace SimpleConsoleApiWrapperExample
{
    internal class Program
    {
        private const string API_KEY = "API KEY HERE";

        static async void Main(string[] args)
        {
            var authManager = new AuthenticationManager(eAuthenticationMode.ApiKey, API_KEY);
            var apiClient = new ApiClient(authManager);

            var accountId = "ACCOUNT ID HERE";
            var runSheetId = "RUNSHEET ID HERE";

            var getRunResult = await apiClient.GetRunSheetById(accountId, runSheetId, new ApiGetOptions()
            {
                ParametersToInclude = new List<string>()
                {
                    "Laps.LapTime",
                }
            });

            if (getRunResult.Success == false)
            {
                Console.WriteLine("Failed to get runsheet");
                return;
            }

            var lapTimes = new List<double>();

            foreach (var lap in getRunResult.ReturnValue.Laps)
            {
                var lapTime = (double?)lap.Parameters["LapTime"];
                if (lapTime.HasValue)
                {
                    lapTimes.Add(lapTime.Value);
                }
            }

            var averageLapTime = lapTimes.Average();

            var updateResult = await apiClient.UpdateRunSheet(accountId, runSheetId, new UpdateModel()
            {
                ParameterUpdates = new List<ParameterUpdateModel>()
                {
                    new ParameterUpdateModel("AvergeLapTimeCalculated", averageLapTime.ToStringInvariant())
                }
            });

            if (updateResult)
            {
                Console.WriteLine($"Successfully updated AvergeLapTimeCalculated to {averageLapTime}");
            }
            else
            {
                Console.WriteLine("Failed to update lap");
            }
        }
    }
}
