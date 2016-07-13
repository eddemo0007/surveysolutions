using System;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Tester
{
    [ApiBasicAuth]
    [RoutePrefix("translation")]
    public class TranslationController : ApiController
    {
        private readonly IPlainStorageAccessor<TranslationInstance> translations;

        public TranslationController(IPlainStorageAccessor<TranslationInstance> translations)
        {
            this.translations = translations;
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public TranslationDto[] Get(Guid id)
            => this.translations.Query(_ => _.Where(x => x.QuestionnaireId == id).ToList()).Cast<TranslationDto>().ToArray();
    }
}