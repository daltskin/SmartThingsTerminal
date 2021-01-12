using System.Collections.Generic;
using CommandLine;
using dotenv.net;

namespace SmartThingsTerminal
{    
    public class Options
    {
        [Option('t', "accesstoken", Required = true, HelpText = "OAuth Personal access token - generate from: https://account.smartthings.com/tokens")]
        public string AccessToken { get; set; }

        [Option('s', "screen", Required = false, HelpText = "Jump into a specific screen: [devices/installedapps/locations/rules/scenes/schedules/subscriptions]")]
        public string ApiName { get; set; }
    }

    public class Startup
    {
        public Options Options
        {
            get;
            set;
        }
        public bool Configure(string[] args)
        {
            // Precedence of reading config
            // 1. Command Line Arguments
            // 2. .env file in file tree
            // 3. System Environment Variable
            var parserResult = CommandLine.Parser.Default
                .ParseArguments<Options>(args)
                .WithNotParsed(ReadEnvironmentVariables)
                .WithParsed(UseCommandLineArguments);

            return Options != null;
        }

        private void ReadEnvironmentVariables(IEnumerable<Error> errors)
        {
            DotEnv.Config(false, "local.env");
            var accessToken = System.Environment.GetEnvironmentVariable("STT_ACCESSTOKEN");
            var screen = System.Environment.GetEnvironmentVariable("STT_SCREEN");

            if (!string.IsNullOrEmpty(accessToken))
            {
                Options = new Options
                {
                    AccessToken = accessToken,
                    ApiName = screen                    
                };
            }
        }

        private void UseCommandLineArguments(Options options)
        {
            this.Options = options;
        }
    }
}