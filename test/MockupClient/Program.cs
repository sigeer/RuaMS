// See https://aka.ms/new-console-template for more information
using MockupClient;

Console.WriteLine("Hello, World!");

var client1 = new ClientBootstrap();
await client1.Initialize();
await Task.Delay(Timeout.Infinite);