﻿using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.ReusableCategories
{
    class ReusableCategoriesFillerIntoQuestionnaire : IReusableCategoriesFillerIntoQuestionnaire
    {
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;

        public ReusableCategoriesFillerIntoQuestionnaire(IReusableCategoriesStorage reusableCategoriesStorage)
        {
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        public QuestionnaireDocument FillCategoriesIntoQuestionnaireDocument(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument originalDocument)
        {
            var filledDocument = originalDocument.Clone();

            if (filledDocument.Categories.Any())
            {
                foreach (var question in filledDocument.Find<ICategoricalQuestion>())
                {
                    if (question.CategoriesId.HasValue)
                    {
                        var options = reusableCategoriesStorage.GetOptions(questionnaireIdentity, question.CategoriesId.Value);
                        question.Answers = options.Select(option => new Answer()
                        {
                            AnswerCode = option.Id,
                            AnswerText = option.Text,
                            ParentCode = option.ParentId,
                            ParentValue = option.ParentId?.ToString(),
                            AnswerValue = option.Id.ToString(),
                        }).ToList();
                    }
                }
            }

            return filledDocument;
        }
    }
}
