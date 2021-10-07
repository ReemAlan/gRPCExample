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
        PeopleResponse response = new();
        response.People.Add(new Person { Id = 1, FirstName = "first name 1", LastName = "last name 1" });
        response.People.Add(new Person { Id = 2, FirstName = "first name 2", LastName = "last name 2" });
        response.People.Add(new Person { Id = 3, FirstName = "first name 3", LastName = "last name 3" });

        return Task.FromResult(response);
    }

    public override async Task GetPeopleServerStream(Empty _, IServerStreamWriter<Person> responseStream, ServerCallContext context)
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
                _logger.LogInformation($"{e.Message}:\tRequest has been cancelled by the client.");
            }
            catch (Exception e)
            {
                _logger.LogInformation($"{e.Message}");
            }
        }
    }

    public override async Task<PeopleResponse> GetPeopleClientStream(IAsyncStreamReader<StreamerClientRequest> requestStream, ServerCallContext context)
    {
        PeopleResponse response = new();
        while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
        {
            int current = requestStream.Current.Start;
            response.People.Add(new Person { Id = current, FirstName = $"first {current}", LastName = $"last {current}" });
        }

        return response;
    }

    public override async Task GetPeopleStreamers(IAsyncStreamReader<StreamerClientRequest> requestStream, 
        IServerStreamWriter<Person> responseStream, 
        ServerCallContext context)
    {
        _logger.LogInformation("About to start streaming from server..");

        try
        {
            while (!context.CancellationToken.IsCancellationRequested && await requestStream.MoveNext())
            {
                int start = requestStream.Current.Start;
                await responseStream.WriteAsync(new Person { Id = start, FirstName = $"First {start}", LastName = $"Last {start}"});            
            }
        }
        catch (InvalidOperationException e)
        {
            _logger.LogInformation($"{e.Message}\tRequest has been cancelled by the client. 2");
        }
        catch (Exception e)
        {
            _logger.LogInformation($"{e.Message}");
        }
        
    }
}
