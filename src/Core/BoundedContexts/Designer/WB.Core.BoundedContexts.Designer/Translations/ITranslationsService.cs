using System;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    public interface ITranslationsService
    {
        ITranslation Get(Guid questionnaireId, Guid? translationId);
        TranslationFile GetAsExcelFile(Guid questionnaireId, Guid? translationId);
        void Store(Guid questionnaireId, Guid translationId, byte[] excelRepresentation);
        void CloneTranslation(Guid questionnaireId, Guid translationId, Guid newQuestionnaireId, Guid newTranslationId);
        void Delete(Guid questionnaireId, Guid translationId);
    }
}