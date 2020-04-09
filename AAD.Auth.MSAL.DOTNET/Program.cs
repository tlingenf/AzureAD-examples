/*##########################################################################################################
# Disclaimer
# This sample code, scripts, and other resources are not supported under any Microsoft standard support 
# program or service and are meant for illustrative purposes only.
#
# The sample code, scripts, and resources are provided AS IS without warranty of any kind. Microsoft 
# further disclaims all implied warranties including, without limitation, any implied warranties of 
# merchantability or of fitness for a particular purpose. The entire risk arising out of the use or 
# performance of this material and documentation remains with you. In no event shall Microsoft, its 
# authors, or anyone else involved in the creation, production, or delivery of the sample be liable 
# for any damages whatsoever (including, without limitation, damages for loss of business profits, 
# business interruption, loss of business information, or other pecuniary loss) arising out of the 
# use of or inability to use the samples or documentation, even if Microsoft has been advised of 
# the possibility of such damages.
##########################################################################################################*/
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AAD.Auth.MSAL.DOTNET
{
    class Program
    {
        public static IConfigurationRoot configuration;

        static async Task Main(string[] args)
        {
            // Build configuration
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.json.user", true)
                .Build();

            var defaultForegroundColor = Console.ForegroundColor;

            var example = new GrantTypeExamples(configuration);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Select an authentication flow.");
            Console.WriteLine("1) - App Only Client Credentials Flow");
            Console.WriteLine("2) - Resource Owner Password Credentials");
            Console.WriteLine("3) - Device Code Flow");
            Console.WriteLine("4) - Interactive Login");
            Console.WriteLine("5) - Refresh Token");

            var selection = Console.ReadKey();

            Console.WriteLine();

            string accessToken = null;

            try
            {
                switch (selection.KeyChar)
                {
                    case '1':
                        accessToken = await example.ClientCredentialsAsync();
                        break;

                    case '2':
                        accessToken = await example.ResourceOwnerPasswordAsync();
                        break;

                    case '3':
                        accessToken = await example.DeviceCodeAsync();
                        break;

                    case '4':
                        accessToken = await example.InteractiveAsync();
                        break;

                    case '5':
                        accessToken = await example.LoginWithRefreshTokenAsync();
                        break;
                }

                Console.WriteLine("Your access token is:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(accessToken);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }

            Console.ForegroundColor = defaultForegroundColor;
        }
    }
}
