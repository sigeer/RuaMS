// See https://aka.ms/new-console-template for more information
using Application.Benchmark;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<XmlWzMapperBenchmark>();

//BenchmarkSwitcher
//    .FromAssembly(typeof(Program).Assembly)
//    .Run(args);