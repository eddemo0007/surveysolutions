﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenFilesStoreRepositoryAccessorTests
{
    internal class when_clear_method_called : RavenFilesStoreRepositoryAccessorTestContext
    {
        Establish context = () =>
        {
            ravenFilesStoreRepositoryAccessor = CreateRavenFilesStoreRepositoryAccessor();
            ravenFilesStoreRepositoryAccessor.Store(storedView, id);
        };

        Because of = () => { ravenFilesStoreRepositoryAccessor.Clear(); };

        It should_return_null_for_stored_view = () =>
           ravenFilesStoreRepositoryAccessor.GetById(id).ShouldBeNull();

        private Cleanup stuff = () =>
        {
            if (ravenFilesStoreRepositoryAccessor != null)
                ravenFilesStoreRepositoryAccessor.Dispose();
        };

        protected static RavenFilesStoreRepositoryAccessor<TestableView> ravenFilesStoreRepositoryAccessor =
            CreateRavenFilesStoreRepositoryAccessor();

        private static TestableView storedView = new TestableView() { RandomNumber = 2 };
        private static string id = "test";
    }
}
