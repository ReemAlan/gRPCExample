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
        response.People.Add(new PeopleResponse.Types.Person { Id = 1, FirstName = "first name 1", LastName = "last name 1" });
        response.People.Add(new PeopleResponse.Types.Person { Id = 2, FirstName = "first name 2", LastName = "last name 2" });
        response.People.Add(new PeopleResponse.Types.Person { Id = 3, FirstName = "first name 3", LastName = "last name 3" });

        return Task.FromResult(response);
    }
}
