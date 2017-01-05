﻿using System;
using System.Linq;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteSupervisor;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;
using WB.Core.BoundedContexts.Headquarters.Views.Supervisor;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor, Observer")]
    public class UsersApiController : BaseApiController
    {
        private readonly IInterviewersViewFactory interviewersFactory;
        private readonly ISupervisorsViewFactory supervisorsFactory;
        private readonly IUserListViewFactory usersFactory;
        public readonly IDeleteSupervisorService deleteSupervisorService;

        public UsersApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IInterviewersViewFactory interviewersFactory,
            ISupervisorsViewFactory supervisorsFactory,
            IUserListViewFactory usersFactory, 
            IDeleteSupervisorService deleteSupervisorService)
            : base(commandService, provider, logger)
        {
            this.interviewersFactory = interviewersFactory;
            this.supervisorsFactory = supervisorsFactory;
            this.usersFactory = usersFactory;
            this.deleteSupervisorService = deleteSupervisorService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewersView Interviewers(InterviewersListViewModel filter)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

            var input = new InterviewersInputModel
            {
                Page = filter.PageIndex,
                PageSize = filter.PageSize,
                ViewerId = viewerId,
                SupervisorName = filter.SupervisorName,
                Orders = filter.SortOrder,
                SearchBy = filter.SearchBy,
                Archived = filter.Archived,
                ConnectedToDevice = filter.ConnectedToDevice
            };

            return this.interviewersFactory.Load(input);
        }

        [HttpPost]
        [CamelCase]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public DataTableResponse<InterviewerListItem> AllInterviewers([FromBody] DataTableRequest request)
        {
            // Headquarter and Admin can view interviewers by any supervisor
            // Supervisor can view only their interviewers
            Guid viewerId = this.GlobalInfo.GetCurrentUser().Id;

            var input = new InterviewersInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                ViewerId = viewerId,
                Orders = request.GetSortOrderRequestItems(),
                SearchBy = request.Search.Value,

                SupervisorName = string.Empty,
                Archived = false,
                ConnectedToDevice = null
            };

            var interviewers = this.interviewersFactory.Load(input);

            return new DataTableResponse<InterviewerListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = interviewers.TotalCount,
                RecordsFiltered = interviewers.TotalCount,
                Data = interviewers.Items.ToList().Select(x => new InterviewerListItem
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    CreationDate =  x.CreationDate,

                    SupervisorName = x.SupervisorName,
                    Email = x.Email,
                    DeviceId = x.DeviceId
                })
            };

        }

        public class InterviewerListItem
        {
            public virtual Guid UserId { get; set; }
            public virtual string UserName { get; set; }
            public virtual string CreationDate { get; set; }
            public virtual string SupervisorName { get; set; }
            public virtual string Email { get; set; }
            public virtual string DeviceId { get; set; }
        }



        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView Supervisors(UsersListViewModel data)
        {
            var input = new SupervisorsInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = false
            };

            return this.supervisorsFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public SupervisorsView ArchivedSupervisors(UsersListViewModel data)
        {
            var input = new SupervisorsInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = true
            };

            return this.supervisorsFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Observer")]
        public UserListView Headquarters(UsersListViewModel data)
        {
            return this.GetUsers(data, UserRoles.Headquarter);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UserListView Observers(UsersListViewModel data)
        {
            return this.GetUsers(data, UserRoles.Observer);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public UserListView ApiUsers(UsersListViewModel data)
        {
            return this.GetUsers(data, UserRoles.ApiUser);
        }

        private UserListView GetUsers(UsersListViewModel data, UserRoles role, bool archived = false)
        {
            var input = new UserListViewInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Role = role,
                Orders = data.SortOrder,
                SearchBy = data.SearchBy,
                Archived = archived
            };

            return this.usersFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonCommandResponse DeleteSupervisor(DeleteSupervisorCommandRequest request)
        {
            var response = new JsonCommandResponse();
            try
            {
                this.deleteSupervisorService.DeleteSupervisor(request.SupervisorId);
                response.IsSuccess = true;
            }
            catch (Exception e)
            {
                this.Logger.Error(e.Message, e);

                response.IsSuccess = false;
                response.DomainException = e.Message;
            }

            return response;
        }
    }
    public class DeleteSupervisorCommandRequest 
    {
        public Guid SupervisorId { get; set; }
    }
}