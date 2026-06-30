using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Typical.Core.Services;

public static class MessengerServiceExtensions
{
    public static IServiceCollection AddMessenger(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger, StrongReferenceMessenger>();
        return services;
    }
}
