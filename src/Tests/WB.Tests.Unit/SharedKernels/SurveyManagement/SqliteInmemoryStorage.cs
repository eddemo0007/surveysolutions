using NSubstitute;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class SqliteInmemoryStorage<TEntity> : SqlitePlainStorage<TEntity> where TEntity : class , IPlainStorageEntity, new()
    {
        public SqliteInmemoryStorage() : base(new SQLiteConnectionWithLock(":memory:"), Substitute.For<ILogger>())
        {
        }
    }
}