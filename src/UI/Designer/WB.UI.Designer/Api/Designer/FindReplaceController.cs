﻿using System;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Filters;

namespace WB.UI.Designer.Api
{
    [CamelCase]
    public class FindReplaceController : ApiController
    {
        private readonly IFindReplaceService replaceService;

        public FindReplaceController(IFindReplaceService replaceService)
        {
            this.replaceService = replaceService;
        }

        [HttpGet]
        public HttpResponseMessage FindAll(Guid id, string searchFor)
        {
            return Request.CreateResponse(this.replaceService.FindAll(id, searchFor));
        }
    }
}