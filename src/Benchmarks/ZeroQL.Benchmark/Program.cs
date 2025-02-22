﻿using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GraphQL.TestServer;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake.TestServerClient;
using ZeroQL;

var serverContext = new ZeroQL.TestServer.Program.ServerContext();

_ = ZeroQL.TestServer.Program.StartServer(serverContext);
await ZeroQL.TestServer.Program.VerifyServiceIsRunning(serverContext);

var benchmark = new RawVSZeroQL();
var raw = await benchmark.Raw();
var strawberry = await benchmark.StrawberryShake();
var zeroQLLambda = await benchmark.ZeroQLLambda();
var zeroQLRequest = await benchmark.ZeroQLRequest();

if (!(raw == strawberry && strawberry == zeroQLLambda && zeroQLLambda == zeroQLRequest))
{
    Console.WriteLine("Raw, StrawberryShake and ZeroQL are not equal");
    return;
}

BenchmarkRunner.Run<RawVSZeroQL>();

ZeroQL.TestServer.Program.StopServer(serverContext);

[MemoryDiagnoser]
public class RawVSZeroQL
{
    private readonly JsonSerializerOptions options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter()
        },
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient httpClient;
    private readonly TestServerClient zeroQLClient;
    private readonly StrawberryShakeTestServerClient strawberryShake;
    private readonly Upload upload;

    public RawVSZeroQL()
    {
        httpClient = new ImmortalHttpClientForStrawberryShake();
        httpClient.BaseAddress = new Uri("http://localhost:10000/graphql");

        zeroQLClient = new TestServerClient(httpClient);
        strawberryShake = StrawberryShakeTestServerClientCreator.Create(httpClient);
        upload = new Upload("image.png", new MemoryStream(new byte[42]));
    }

    [Benchmark]
    public async Task<string> Raw()
    {
        var rawQuery = @"{ ""query"": ""query { me { firstName }}"" }";
        var response = await httpClient.PostAsync("", new StringContent(rawQuery, Encoding.UTF8, "application/json"));
        var responseJson = await response.Content.ReadAsStreamAsync();
        var qlResponse = JsonSerializer.Deserialize<JsonObject>(responseJson, options);

        return qlResponse["data"]["me"]["firstName"].GetValue<string>();
    }

    [Benchmark]
    public async Task<string> StrawberryShake()
    {
        var firstname = await strawberryShake.Me.ExecuteAsync();
        return firstname.Data.Me.FirstName;
    }

    [Benchmark]
    public async Task<string> ZeroQLLambda()
    {
        var firstname = await zeroQLClient.Query(static q => q.Me(o => o.FirstName));

        return firstname.Data!;
    }

    [Benchmark]
    public async Task<string> ZeroQLRequest()
    {
        var firstname = await zeroQLClient.Execute(new GetMeQuery());

        return firstname.Data!;
    }
    
    [Benchmark]
    public async Task<int> ZeroQLLambdaUpload()
    {
        var variables = new { Id = 1, File = upload };
        var size = await zeroQLClient.Mutation(variables, static (i, m) => m.AddUserProfileImage(i.Id, i.File));

        return size.Data;
    }

    [Benchmark]
    public async Task<int> ZeroQLRequestUpload()
    {
        var size = await zeroQLClient.Execute(new AddAvatar(1, upload));

        return size.Data;
    }
}

public record GetMeQuery : GraphQL<Query, string>
{
    public override string Execute(Query query)
        => query.Me(o => o.FirstName);
}

public record AddAvatar(int Id, Upload File) : GraphQL<Mutation, int>
{
    public override int Execute(Mutation mutation)
        => mutation.AddUserProfileImage(Id, File);
}

public static class StrawberryShakeTestServerClientCreator
{
    public static StrawberryShakeTestServerClient Create(HttpClient httpClient)
    {
        var services = new ServiceCollection();
        services.AddStrawberryShakeTestServerClient();
        services.AddSingleton<IHttpClientFactory>(new FakeFactory(httpClient));

        var provider = services.BuildServiceProvider();
        var client = provider.GetService<StrawberryShakeTestServerClient>()!;

        return client;
    }

    private class FakeFactory : IHttpClientFactory
    {
        private readonly HttpClient client;

        public FakeFactory(HttpClient client)
        {
            this.client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return client;
        }
    }
}

public class ImmortalHttpClientForStrawberryShake : HttpClient
{
    protected override void Dispose(bool disposing)
    {
    }
}