// See https://aka.ms/new-console-template for more information
using Application.Benchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;


var config = ManualConfig.Create(DefaultConfig.Instance)
    .WithBuildTimeout(TimeSpan.FromMinutes(5));

BenchmarkRunner.Run<PacketReadStringBenchmark>(config);

//BenchmarkSwitcher
//    .FromAssembly(typeof(Program).Assembly)
//    .Run(args);