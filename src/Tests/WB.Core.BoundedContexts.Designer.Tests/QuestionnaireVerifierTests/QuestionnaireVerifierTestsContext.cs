using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using System.Linq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVerifierTests
{
    [Subject(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier(IExpressionProcessor expressionProcessor = null)
        {
            return new QuestionnaireVerifier(expressionProcessor ?? new Mock<IExpressionProcessor>().Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToList(),
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }
    }
}