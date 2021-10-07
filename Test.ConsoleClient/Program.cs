// See https://aka.ms/new-console-template for more information
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf.Collections;
using Grpc.Core;

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

            // Server streaming
            Console.WriteLine("=====================================");
            Console.WriteLine("Server Streaming Call");
            Console.WriteLine("=====================================");
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var streamingServerCall = client2.GetPeopleServerStream(new Empty(), cancellationToken: cts.Token);
            try
            {
                await foreach (var person in streamingServerCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    Console.WriteLine($"{person.Id} | {person.FirstName} | {person.LastName}");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {               
                Console.WriteLine("Request cancelled.");
            }
            

            Console.WriteLine("=====================================");
            Console.WriteLine("Client Streaming Call");
            Console.WriteLine("=====================================");
            using var clientStreamingCall = client2.GetPeopleClientStream();
            try 
            {
                string? input;
                Console.WriteLine("Please the next person's id number or press space to quit.");
                while (true)
                {
                    input = Console.ReadLine();
                    if (String.IsNullOrEmpty(input)) break;
                    await clientStreamingCall.RequestStream.WriteAsync(new StreamerClientRequest { Start = Int32.Parse(input) });
                }
                await clientStreamingCall.RequestStream.CompleteAsync();
                List<Person> clientStreamPeople = (await clientStreamingCall.ResponseAsync).People.ToList();
                foreach(var person in clientStreamPeople)
                {
                    Console.WriteLine($"{person.Id} | {person.FirstName} | {person.LastName}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.WriteLine("=====================================");
            Console.WriteLine("Bidirectional Streaming Call");
            Console.WriteLine("=====================================");
            // Bi-directional streaming
            var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(7));
            using var bidirectionalStreamingCall = client2.GetPeopleStreamers(cancellationToken: cts2.Token);
            Console.WriteLine("Opened communication with server");
            try
            {
                var readTask = Task.Run(async () =>
                {
                    await foreach (var person in bidirectionalStreamingCall.ResponseStream.ReadAllAsync())
                    {
                        Console.WriteLine(person);
                    }
                });

                var writeTask = Task.Run(async () => 
                {
                    int count = 100;
                    while (!cts2.Token.IsCancellationRequested)
                    {
                        await bidirectionalStreamingCall.RequestStream.WriteAsync(new StreamerClientRequest { Start = count++ });
                        await Task.Delay(500);
                    }
                });
                
                await Task.WhenAll(writeTask, readTask);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Bidirectional request cancelled.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}