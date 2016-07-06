﻿using System;
using Main.Core.Entities.Composite;

namespace WB.Core.SharedKernels.Questionnaire.Translator
{
    public interface IQuestionnaireTranslation
    {
        string GetTitle(Guid entityId);
        string GetInstruction(Guid questionId);
    }
}