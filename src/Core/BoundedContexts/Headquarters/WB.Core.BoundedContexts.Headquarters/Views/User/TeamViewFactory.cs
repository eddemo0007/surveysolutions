﻿using System;
using System.Data.Entity;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class TeamViewFactory : ITeamViewFactory 
    {
        readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        private readonly IUserRepository userRepository;

        public TeamViewFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader, 
            IUserRepository userRepository)
        {
            this.interviewSummaryReader = interviewSummaryReader;
            this.userRepository = userRepository;
        }

        public UsersView GetAssigneeSupervisorsAndDependentInterviewers(int pageSize, string searchBy)
        {
            var assigneeSupervisorsAndDependentInterviewers = new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByTeamLead(searchBy: searchBy, interviews: interviews)
                        .ToList()
                        .Count())
            };
            FillUserRoles(assigneeSupervisorsAndDependentInterviewers);
            return assigneeSupervisorsAndDependentInterviewers;
        }

        public UsersView GetAsigneeInterviewersBySupervisor(int pageSize, string searchBy, Guid supervisorId)
        {
            var asigneeInterviewersBySupervisor = new UsersView()
            {
                Users = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .Take(pageSize)
                        .ToList()),

                TotalCountByQuery = this.interviewSummaryReader.Query(interviews =>
                    ApplyFilterByResponsible(searchBy, supervisorId, interviews)
                        .ToList().Count)
            };
            FillUserRoles(asigneeInterviewersBySupervisor);

            return asigneeInterviewersBySupervisor;
        }

        private void FillUserRoles(UsersView asigneeInterviewersBySupervisor)
        {
            var userIds = asigneeInterviewersBySupervisor.Users.Select(x => x.UserId).ToList();

            var allUsers = this.userRepository.Users.Where(x => userIds.Contains(x.Id)).Include(x => x.Roles).ToList();

            foreach (var user in asigneeInterviewersBySupervisor.Users)
            {
                user.IconClass = allUsers.FirstOrDefault(x => x.Id == user.UserId).Roles.FirstOrDefault().Role.ToString()
                    .ToLower();
            }
        }

        private static IQueryable<UsersViewItem> ApplyFilterByResponsible(string searchBy, Guid supervisorId, IQueryable<InterviewSummary> interviews)
        {
            interviews = interviews.Where(interview => !interview.IsDeleted && interview.TeamLeadId == supervisorId);

            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                interviews = interviews.Where(x => x.ResponsibleName.ToLower().Contains(searchBy.ToLower()));
            }

            var responsiblesFromInterviews = interviews.GroupBy(x => new { x.ResponsibleId, x.ResponsibleName })
                                                       .Where(x => x.Count() > 0)
                                                       .Select(x => new UsersViewItem { UserId = x.Key.ResponsibleId, UserName = x.Key.ResponsibleName })
                                                       .OrderBy(x => x.UserName);

            return responsiblesFromInterviews;
        }

        private static IQueryable<UsersViewItem> ApplyFilterByTeamLead(string searchBy, IQueryable<InterviewSummary> interviews)
        {
            interviews = interviews.Where(interview => !interview.IsDeleted);
            
            if (!string.IsNullOrWhiteSpace(searchBy))
            {
                interviews = interviews.Where(x => x.TeamLeadName.ToLower().Contains(searchBy.ToLower()));
            }

            var responsiblesFromInterviews = interviews.GroupBy(x => new {x.TeamLeadId, x.TeamLeadName})
                .Where(x => x.Count() > 0)
                .Select(x => new UsersViewItem
                {
                    UserId = x.Key.TeamLeadId,
                    UserName = x.Key.TeamLeadName
                })
                .OrderBy(x => x.UserName);

            return responsiblesFromInterviews;
        }
    }
}