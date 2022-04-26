using Blish_HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch
{
    internal class ApiHelper
    {
        private static readonly Logger Logger = Logger.GetLogger<ItemSearchModule>();
        private const int ATTEMPT_INTERVAL = 1000;

        public static async Task<TResult> Fetch<TResult>(Func<Task<TResult>> fn)
            where TResult : class
        {
            TResult result = null;
            int attemptsRemaining;
            for (attemptsRemaining = 2; attemptsRemaining >= 0; attemptsRemaining--)
            {
                try
                {
                    result = await fn.Invoke();
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Failed fetching data from API.");
                    if (attemptsRemaining > 0)
                    {
                        await Task.Delay(ATTEMPT_INTERVAL);
                    }
                }
            }

            if (attemptsRemaining < 0)
            {
                Logger.Error($"Failed fetching data from API. No more retries");
                result = null;
            }

            return result;
        }
    }
}
