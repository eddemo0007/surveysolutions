﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    [CamelCase]
    [RoutePrefix("api/Assignments")]
    public class AssignmetsApiController : ApiController
    {
        private readonly IAssignmentViewFactory assignmentViewFactory;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IPlainStorageAccessor<Assignment> assignmentsStorage;

        public AssignmetsApiController(IAssignmentViewFactory assignmentViewFactory,
            IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<Assignment> assignmentsStorage)
        {
            this.assignmentViewFactory = assignmentViewFactory;
            this.authorizedUser = authorizedUser;
            this.assignmentsStorage = assignmentsStorage;
        }
        
        [Route("")]
        [HttpGet]
        public IHttpActionResult Get([FromUri]AssignmentsDataTableRequest request)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            if (!string.IsNullOrEmpty(request.QuestionnaireId))
            {
                QuestionnaireIdentity.TryParse(request.QuestionnaireId, out questionnaireIdentity);
            }

            var input = new AssignmentsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Order = request.GetSortOrder(),
                SearchBy = request.Search.Value,
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId,
                QuestionnaireVersion = questionnaireIdentity?.Version,
                ResponsibleId = request.ResponsibleId,
                ShowArchive = request.ShowArchive
            };

            if (this.authorizedUser.IsSupervisor)
            {
                input.SupervisorId = this.authorizedUser.Id;
            }

            var result = this.assignmentViewFactory.Load(input);
            var response = new AssignmetsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = result.TotalCount,
                RecordsFiltered = result.TotalCount,
                Data = result.Items
            };
            return this.Ok(response);
        }

        
        [Route("")]
        [HttpDelete]
        public IHttpActionResult Delete([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();
            if (!this.authorizedUser.IsAdministrator || !this.authorizedUser.IsHeadquarter)
                return this.StatusCode(HttpStatusCode.Forbidden);

            foreach (var id in ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(id);
                assignment.Archive();
            }

            return this.Ok();
        }

        [Route("Unarchive")]
        [HttpPost]
        public IHttpActionResult Unarchive([FromBody]int[] ids)
        {
            if (ids == null) return this.BadRequest();
            if (!this.authorizedUser.IsAdministrator || !this.authorizedUser.IsHeadquarter)
                return this.StatusCode(HttpStatusCode.Forbidden);

            foreach (var id in ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(id);
                assignment.Unarchive();
            }

            return this.Ok();
        }

        [HttpPost]
        [Route("Assign")]
        public IHttpActionResult Assign([FromBody] AssignRequest request)
        {
            if (request?.Ids == null) return this.BadRequest();
            foreach (var idToAssign in request.Ids)
            {
                Assignment assignment = this.assignmentsStorage.GetById(idToAssign);
                assignment.Reassign(request.ResponsibleId);
            }

            return this.Ok();
        }

        [HttpPatch]
        [Route("{id:int}/SetCapacity")]
        public IHttpActionResult SetCapacity(int id, [FromBody] UpdateAssignmentRequest request)
        {
            if (!this.authorizedUser.IsAdministrator || !this.authorizedUser.IsHeadquarter)
                return this.StatusCode(HttpStatusCode.Forbidden);

            var assignment = this.assignmentsStorage.GetById(id);
            assignment.UpdateCapacity(request.Capacity);
            return this.Ok();
        }

        public class UpdateAssignmentRequest
        {
            public int? Capacity { get; set; }
        }

        public class AssignRequest
        {
            public Guid ResponsibleId { get; set; }

            public int[] Ids { get; set; }
        }

        public class AssignmetsDataTableResponse : DataTableResponse<AssignmentRow>
        {
        }

        public class AssignmentsDataTableRequest : DataTableRequest
        {
            public string QuestionnaireId { get; set; }
            public Guid? ResponsibleId { get; set; }

            public bool ShowArchive { get; set; }
        }
    }
}