#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesService : IWorkspacesService
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;
        private readonly ILoggerProvider loggerProvider;
        private readonly IPlainStorageAccessor<Workspace> workspaces;
        private readonly IPlainStorageAccessor<WorkspacesUsers> workspaceUsers;
        private readonly IUserRepository users;
        private readonly ILogger<WorkspacesService> logger;

        public WorkspacesService(UnitOfWorkConnectionSettings connectionSettings,
            ILoggerProvider loggerProvider, 
            IPlainStorageAccessor<Workspace> workspaces,
            IPlainStorageAccessor<WorkspacesUsers> workspaceUsers,
            IUserRepository users,
            ILogger<WorkspacesService> logger)
        {
            this.connectionSettings = connectionSettings;
            this.loggerProvider = loggerProvider;
            this.workspaces = workspaces;
            this.workspaceUsers = workspaceUsers;
            this.users = users;
            this.logger = logger;
        }

        public Task Generate(string name, DbUpgradeSettings upgradeSettings)
        {
            string schemaName = "ws_" + name;
            DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                schemaName);
            this.logger.LogInformation("Adding workspace {workspace}", name);
            return Task.Run(() => DbMigrationsRunner.MigrateToLatest(connectionSettings.ConnectionString,
                schemaName,
                upgradeSettings, loggerProvider));
        }

        public bool IsWorkspaceDefined(string? workspace)
        {
            return workspaces.Query(_ => _.Any(x => x.Name == workspace));
        }

        public IEnumerable<WorkspaceContext> GetWorkspaces()
        {
            return workspaces.Query(_ => _
                .Select(workspace => workspace.AsContext())
                .ToList());
        }

        public IEnumerable<WorkspaceContext> GetWorkspacesForUser(Guid userId)
        {
            var user = this.users.FindById(userId);

            if (user.IsInRole(UserRoles.Administrator))
            {
                return this.GetWorkspaces();
            }
            
            var userWorkspaces = workspaces.Query(_ =>
                _.Where(x => x.Users.Any(u => u.User.Id == userId))
                    .Select(workspace => workspace.AsContext())
                    .ToList()
            );

            return userWorkspaces;
        }

        public void AddUserToWorkspace(HqUser user, string workspace)
        {
            Workspace workspaceEntity = workspaces.GetById(workspace) ?? throw new ArgumentNullException("Workspace not found");

            var workspaceUser = new WorkspacesUsers(workspaceEntity, user);
            
            this.workspaceUsers.Store(workspaceUser, workspaceUser.Id);
            
            this.logger.LogInformation("Added {user} to {workspace}", user, workspace);
        }
    }
}
