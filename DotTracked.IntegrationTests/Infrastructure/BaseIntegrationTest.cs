using DotTracked.Data;
using DotTracked.IntegrationTests.Infrastructure.Auth;
using DotTracked.Shared.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotTracked.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    protected string CurrentUserId =>
        factory.Services.GetRequiredService<IOptions<TestAuthHandlerOptions>>().Value.DefaultUserId;

    protected async Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Set<TEntity>().Add(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    protected async Task<TEntity?> FindAsync<TEntity>(params object?[]? keyValues) where TEntity : class
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().FindAsync(keyValues);
    }

    protected HttpClient CreateAppUserClient()
    {
        return CreateAuthorizedClient(Roles.AppUser);
    }

    protected HttpClient CreateAdminClient()
    {
        return CreateAuthorizedClient(Roles.Admin);
    }

    private HttpClient CreateAuthorizedClient(string role)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme,
                        options => { options.DefaultRole = role; });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }
}