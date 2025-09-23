// See https://aka.ms/new-console-template for more information
using Application.Benchmark;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<PacketWriteFixStringBenchmark>();

//BenchmarkSwitcher
//    .FromAssembly(typeof(Program).Assembly)
//    .Run(args);