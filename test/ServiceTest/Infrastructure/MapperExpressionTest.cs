using Application.Core.Login;
using Application.EF;
using Application.EF.Entities;
using Application.Module.Duey.Master;
using Application.Module.Duey.Master.Models;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.QueryableExtensions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace ServiceTest.Infrastructure
{
    internal class MapperExpressionTest
    {
        IDbContextFactory<DBContext> _dbContextFactory;

        static Expression<Func<DueyPackageModel, bool>> expression = x => x.Mesos > 0;
        public MapperExpressionTest()
        {
            var services = new ServiceCollection();

            services.AddLoginServerDI("Sqlite", "Data Source=benchmark.db");
            services.AddDueyMaster();

            var provider = services.BuildServiceProvider();

            _dbContextFactory = provider.GetRequiredService<IDbContextFactory<DBContext>>();

            using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Database.Migrate();
        }

        [Test]
        public void MapsterMapExpressionTest()
        {
            using var dbContext = _dbContextFactory.CreateDbContext();
            var queryable = dbContext.Dueypackages.AsNoTracking().ProjectToType<DueyPackageModel>().Where(expression);
            Console.WriteLine(queryable.ToQueryString());
            var dataFromDB = queryable.ToList();
            //可以考虑用Mapster替换AutoMapper
            Assert.Pass();
        }

        [Test]
        public void OldAutoMapperMapExpressionTest()
        {
            var mapper = new AutoMapper.MapperConfiguration(opt =>
            {
                opt.CreateMap<DueyPackageEntity, DueyPackageModel>()
                    .ForMember(dest => dest.Id, src => src.MapFrom(x => x.PackageId));
            }).CreateMapper();
            using var dbContext = _dbContextFactory.CreateDbContext();
            var entityExpression = mapper.MapExpression<Expression<Func<DueyPackageEntity, bool>>>(expression);
            var queryable = dbContext.Dueypackages.Where(entityExpression);
            Console.WriteLine(queryable.ToQueryString());
            var dataFromDB = queryable.ToList();
            //可以考虑用Mapster替换AutoMapper
            Assert.Pass();
        }

        [Test]
        public void AutoMapperMapExpressionTest()
        {
            var mapper = new AutoMapper.MapperConfiguration(opt =>
            {
                opt.CreateMap<DueyPackageEntity, DueyPackageModel>()
                    .ForMember(dest => dest.Id, src => src.MapFrom(x => x.PackageId));
            }).CreateMapper();
            using var dbContext = _dbContextFactory.CreateDbContext();
            // var entityExpression = mapper.MapExpression<Expression<Func<FredstorageEntity, bool>>>(expression);
            var queryable = dbContext.Dueypackages.ProjectTo<DueyPackageModel>(mapper.ConfigurationProvider).Where(expression);
            Console.WriteLine(queryable.ToQueryString());
            var dataFromDB = queryable.ToList();
            //可以考虑用Mapster替换AutoMapper
            Assert.Pass();
        }
    }
}
