using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FastExpressionCompiler;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Benchmark
{
    /**
     * 
     * | Method             | Mean      | Error      | StdDev    | Rank | Gen0   | Allocated |
     * |------------------- |----------:|-----------:|----------:|-----:|-------:|----------:|
     * | UseMapsterAdapt    |  31.44 ns |  11.966 ns |  3.107 ns |    1 | 0.0306 |      48 B |
     * | UseMapsterMapper   |  44.12 ns |  17.455 ns |  4.533 ns |    2 | 0.0306 |      48 B |
     * | UseAutoMapper      |  98.45 ns |   7.791 ns |  1.206 ns |    3 | 0.0305 |      48 B |
     * | UseMapsterDIMapper | 234.95 ns | 147.506 ns | 38.307 ns |    4 | 0.2346 |     368 B |
     */
    [RankColumn]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(warmupCount: 5, iterationCount: 5)]
    [MemoryDiagnoser]
    public class MapperBenchrmark
    {
        AutoMapper.IMapper _autoMapper;
        MapsterMapper.IMapper _mapsterDIMapper;
        MapsterMapper.IMapper _mapsterMapper;
        TestSourceModel source;
        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection();

            TypeAdapterConfig.GlobalSettings.NewConfig<TestSourceModel, TestTargetModel>()
                .Map(dest => dest.Time, x => x.DateTimeOffsetValue.ToUnixTimeMilliseconds());
            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
            TypeAdapterConfig.GlobalSettings.Compile();

            services.AddSingleton(TypeAdapterConfig.GlobalSettings);
            // 使用Mapper性能正常
            services.AddSingleton<IMapper, ServiceMapper>();

            var provider = services.BuildServiceProvider();
            _mapsterMapper = new Mapper(TypeAdapterConfig.GlobalSettings);
            _mapsterDIMapper = provider.GetRequiredService<IMapper>();


            _autoMapper = new AutoMapper.MapperConfiguration(opt =>
            {
                opt.CreateMap<TestSourceModel, TestTargetModel>()
                    .ForMember(dest => dest.Time, src => src.MapFrom(x => x.DateTimeOffsetValue.ToUnixTimeMilliseconds()));
            }).CreateMapper();


            source = new TestSourceModel { DateTimeOffsetValue = DateTimeOffset.UtcNow };

        }
        [Benchmark()]
        public void UseAutoMapper()
        {
            _autoMapper.Map<TestTargetModel>(source);
        }

        [Benchmark()]
        public void UseMapsterAdapt()
        {
            source.Adapt<TestTargetModel>();
        }

        [Benchmark()]
        public void UseMapsterDIMapper()
        {
            _mapsterDIMapper.Map<TestTargetModel>(source);
        }

        [Benchmark()]
        public void UseMapsterMapper()
        {
            _mapsterMapper.Map<TestTargetModel>(source);
        }
    }

    public class TestSourceModel
    {
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }


    public class TestTargetModel
    {
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public string StringValue { get; set; }
        public long Time { get; set; }
    }

    public class TestService
    {
        public int GetIntValue()
        {
            return Random.Shared.Next(10);
        }
    }

}
