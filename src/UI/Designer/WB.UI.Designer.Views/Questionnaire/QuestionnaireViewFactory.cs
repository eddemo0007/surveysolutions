﻿using Main.Core.Documents;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>,
        IViewFactory<QuestionnaireViewInputModel, QuestionnaireStataMapView>
    {
        private readonly IDenormalizerStorage<QuestionnaireDocument> _questionnaireStorage;

        public QuestionnaireViewFactory(IDenormalizerStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this._questionnaireStorage = questionnaireStorage;
        }

        QuestionnaireView IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>.Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);

            return new QuestionnaireView(doc);
        }

        QuestionnaireStataMapView IViewFactory<QuestionnaireViewInputModel, QuestionnaireStataMapView>.Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);

            return new QuestionnaireStataMapView(doc);
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            return this._questionnaireStorage.GetByGuid(input.QuestionnaireId);
        }
    }
}