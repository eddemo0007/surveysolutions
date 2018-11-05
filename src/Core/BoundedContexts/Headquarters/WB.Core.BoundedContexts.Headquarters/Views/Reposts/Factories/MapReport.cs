﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.CoordinateReferenceSystem;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Clustering;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    internal class MapReport : IMapReport
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor;
        private readonly IAuthorizedUser authorizedUser;

        public MapReport(IInterviewFactory interviewFactory, IQuestionnaireStorage questionnaireStorage,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor, IAuthorizedUser authorizedUser)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnairesAccessor = questionnairesAccessor;
            this.authorizedUser = authorizedUser;
        }

        public List<string> GetGpsQuestionsByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
            => this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity)
                .Find<GpsCoordinateQuestion>().Select(question => question.StataExportCaption).ToList();

        public MapReportView Load(MapReportInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(input.QuestionnaireIdentity, null);
            var gpsQuestionId = questionnaire.GetQuestionIdByVariable(input.Variable);

            if (!gpsQuestionId.HasValue) throw new ArgumentNullException(nameof(gpsQuestionId));

            var gpsAnswers = this.interviewFactory.GetGpsAnswers(
                input.QuestionnaireIdentity,
                gpsQuestionId.Value, null, new GeoBounds(input.South, input.West, input.North, input.East), 
                this.authorizedUser.IsSupervisor ? this.authorizedUser.Id : (Guid?)null);

            var superCluster = new SuperCluster();

            GeoBounds bounds = GeoBounds.Inverse;

            superCluster.Load(gpsAnswers.Select(g =>
            {
                bounds.AdjustMinMax(g.Latitude, g.Longitude);

                return new SuperCluster.GeoPoint
                {
                    Position = new[] { g.Longitude, g.Latitude },
                    Props = new Dictionary<string, object>
                    {
                        ["interviewId"] = g.InterviewId.ToString()
                    }
                };
            }));

            if (input.Zoom == -1)
            {
                // https://stackoverflow.com/a/6055653/41483
                const int GLOBE_WIDTH = 256; // a constant in Google's map projection

                var angle = bounds.East - bounds.West;
                if (angle < 0)
                {
                    angle += 360;
                }

                input.Zoom = (int)Math.Round(Math.Log(input.MapWidth * 360 / angle / GLOBE_WIDTH) / Math.Log(2)) - 1;
            }

            var result = superCluster.GetClusters(new GeoBounds(input.South, input.West, input.North, input.East), input.Zoom);

            var collection = new FeatureCollection();

            collection.Features.AddRange(result.Select(p =>
            {
                var props = p.UserData.Props ?? new Dictionary<string, object>();
                if (p.UserData.NumPoints.HasValue)
                {
                    props["count"] = p.UserData.NumPoints;
                    props["expand"] = superCluster.GetClusterExpansionZoom(p.UserData.Index);
                }

                return new Feature(
                    new Point(new Position(p.Latitude, p.Longitude)),
                    props, id: p.UserData.Index.ToString("X"));
            }));

            return new MapReportView
            {
                InitialBounds = bounds,
                FeatureCollection = collection,
                TotalPoint = gpsAnswers.Length
            };
        }

        public List<QuestionnaireBrowseItem> GetQuestionnaireIdentitiesWithPoints() =>
            this.questionnairesAccessor.Query(_ => _.Where(x => !x.IsDeleted).ToList());
    }
}
