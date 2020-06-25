using System;
using GitHubStatTracker.Core.Services;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(GitHubStatTracker.Function.Startup))]

namespace GitHubStatTracker.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<TrackRepos, TrackRepos>();
            builder.Services.AddSingleton<TableStorageService, TableStorageService>();
            builder.Services.AddSingleton<GitHubService, GitHubService>();

            builder.Services.AddOptions<OAuthOptions>()
                    .Configure<IConfiguration>((settings, configuration) =>
                    {
                        configuration.GetSection("GitHub").Bind(settings);
                    });
        }
    }
}
