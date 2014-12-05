using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    internal static class Setup
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static void StubToMockedServiceLocator<T>()
            where T : class
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<T>())
                .Returns(Mock.Of<T>());
        }

        public static void SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator()
        {
            var questionnaireDocumentUpgrader = Mock.Of<IQuestionnaireDocumentUpgrader>();

            Setup.InstanceToMockedServiceLocator(questionnaireDocumentUpgrader);
        }

        public static IQueryableReadSideRepositoryReader<TEntity> QueryableReadSideRepositoryReaderByQueryResultType<TEntity, TResult>(IEnumerable<TEntity> entities)
            where TEntity : class, IReadSideRepositoryEntity
        {
            var repositoryReader = Mock.Of<IQueryableReadSideRepositoryReader<TEntity>>();

            Mock.Get(repositoryReader)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<TEntity>, TResult>>()))
                .Returns<Func<IQueryable<TEntity>, TResult>>(query => query.Invoke(entities.AsQueryable()));

            Mock.Get(repositoryReader)
                .Setup(reader => reader.QueryAll(It.IsAny<Expression<Func<TEntity, bool>>>()))
                .Returns<Expression<Func<TEntity, bool>>>(condition =>
                {
                    Func<TEntity, bool> predicate = condition.Compile();
                    return entities.Where(predicate);
                });

            return repositoryReader;
        }
    }
}