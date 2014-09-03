﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ChartStatisticsFactoryTests
{
    internal class when_creating_statistics_should_filter_date_range : ChartStatisticsFactoryTestsContext
    {
        Establish context = () =>
        {
            var stats = Mock.Of<IQueryableReadSideRepositoryReader<StatisticsLineGroupedByDateAndTemplate>>();

            var questionnaireId = Guid.NewGuid();
            var baseDate = new DateTime(2014, 8, 22);
            var questionnaireVersion = 1;

            var data = new List<StatisticsLineGroupedByDateAndTemplate>
            {
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(-5),
                    DateTicks = baseDate.AddDays(-2).Ticks,
                    ApprovedByHeadquartersCount = 0,
                    ApprovedBySupervisorCount = 0,
                    CompletedCount = 0,
                    InterviewerAssignedCount = 0,
                    RejectedByHeadquartersCount = 0,
                    RejectedBySupervisorCount = 0,
                    SupervisorAssignedCount = 0
                },
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(-4),
                    DateTicks = baseDate.AddDays(-2).Ticks,
                    ApprovedByHeadquartersCount = 1,
                    ApprovedBySupervisorCount = 1,
                    CompletedCount = 1,
                    InterviewerAssignedCount = 1,
                    RejectedByHeadquartersCount = 1,
                    RejectedBySupervisorCount = 1,
                    SupervisorAssignedCount = 1
                },
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(-2),
                    DateTicks = baseDate.Date.Ticks,
                    ApprovedByHeadquartersCount = 3,
                    ApprovedBySupervisorCount = 3,
                    CompletedCount = 3,
                    InterviewerAssignedCount = 3,
                    RejectedByHeadquartersCount = 3,
                    RejectedBySupervisorCount = 3,
                    SupervisorAssignedCount = 3
                },
                new StatisticsLineGroupedByDateAndTemplate
                {
                    QuestionnaireId = questionnaireId,
                    QuestionnaireVersion = questionnaireVersion,
                    Date = baseDate.AddDays(-1),
                    DateTicks = baseDate.Date.Ticks,
                    ApprovedByHeadquartersCount = 4,
                    ApprovedBySupervisorCount = 4,
                    CompletedCount = 4,
                    InterviewerAssignedCount = 4,
                    RejectedByHeadquartersCount = 4,
                    RejectedBySupervisorCount = 4,
                    SupervisorAssignedCount = 4
                }
            }.AsQueryable();

            chartStatisticsFactory = CreateChartStatisticsFactory(data);

            input = new ChartStatisticsInputModel
            {
                CurrentDate = baseDate,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                From = baseDate.AddDays(-4),
                To = baseDate.AddDays(-2)
            };
        };

        Because of = () => view = chartStatisticsFactory.Load(input);

        It should_have_days_count_three_muliply_two_records = () => view.Ticks.Length.ShouldEqual(3 * 2);

        It should_have_supervisorAssignedData_correct = () => view.Stats[0].ShouldEqual(new[] { 1, 1, 3 });

        private static ChartStatisticsFactory chartStatisticsFactory;
        private static ChartStatisticsInputModel input;
        private static ChartStatisticsView view;
    }
}