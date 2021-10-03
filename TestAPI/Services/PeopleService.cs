using Grpc.Core;
using TestAPI;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;

namespace TestAPI.Services;

public class PeopleService : People.PeopleBase
{
    private readonly ILogger<PeopleService> _logger;
    public PeopleService(ILogger<PeopleService> logger)
    {
        _logger = logger;
    }

    public override Task<PeopleResponse> GetPeople(Empty request, ServerCallContext context)
    {
        PeopleResponse response = new PeopleResponse();
        response.People.Add(new Person { Id = 1, FirstName = "first name 1", LastName = "last name 1" });
        response.People.Add(new Person { Id = 2, FirstName = "first name 2", LastName = "last name 2" });
        response.People.Add(new Person { Id = 3, FirstName = "first name 3", LastName = "last name 3" });

        return Task.FromResult(response);
    }

    public override async Task GetPeopleStream(Empty _, IServerStreamWriter<Person> responseStream, ServerCallContext context)
    {
        int i = 1;
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
            
            var person = new Person
            {
                Id = i,
                FirstName = $"first name {i}",
                LastName = $"last name {i}"
            };

            try
            {
                _logger.LogInformation("Sending Person response");
                await responseStream.WriteAsync(person);
                i++;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogInformation($"{e.Message}\n     Request has been cancelled by the client.");
            }
        }
    }

    public override Task<List<Person>> GetPeople(IAsyncStreamReader<StreamerClientRequest> requestStream, ServerCallContext context)
    {
        List<Person> response = new();
        while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
        {
            int start = requestStream.Current.Start;
            response.Add(new Person { Id = start, FirstName = $"Client stream first {start}", LastName = $"Client stream last {start}"});
        }
        
        return Task.FromResult(response);
    }

    public override async Task GetPeopleStreamers(IAsyncStreamReader<StreamerClientRequest> requestStream, 
        IServerStreamWriter<Person> responseStream, 
        ServerCallContext context)
    {
        _logger.LogInformation("About to start streaming from server..");

        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                int start = await requestStream.MoveNext() ? requestStream.Current.Start: 100;
                for (int i = start; i <= i+3; i++)
                {
                    await responseStream.WriteAsync(new Person { Id = i, FirstName = $"First {i}", LastName = $"Last {i}"});
                    _logger.LogInformation($"Sent {i} person/people");
                }
            }
            catch (InvalidOperationException e)
            {
                _logger.LogInformation($"{e.Message}\n\tRequest has been cancelled by the client.");
            }
        }
    }
}
