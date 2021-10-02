// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;

namespace Test.ConsoleClient 
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:7067");
            var client = new Greeter.GreeterClient(channel);
            var client2 = new People.PeopleClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
            var reply2 = await client2.GetPeopleAsync(new Empty());
            var people = reply2.People.ToList();

            Console.WriteLine("Greeting: " + reply.Message);

            Console.WriteLine("=====================================");

            foreach(var person in people)
            {
                Console.WriteLine(person.Id);
                Console.WriteLine(person.FirstName);
                Console.WriteLine(person.LastName);
                Console.WriteLine("-------------------------------------");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}