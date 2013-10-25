using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    using AtomicVerifier = Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>>;

    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private readonly IExpressionProcessor expressionProcessor;

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeLinkedQuestionSource = new[]
        { QuestionType.DateTime, QuestionType.Numeric, QuestionType.Text };

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeSubstitutionReferences = new[]
        {
            QuestionType.DateTime, QuestionType.Numeric, QuestionType.SingleOption, QuestionType.Text, QuestionType.AutoPropagate
        };

        private readonly IEnumerable<AtomicVerifier> atomicVerifiers;

        public QuestionnaireVerifier(IExpressionProcessor expressionProcessor)
        {
            this.expressionProcessor = expressionProcessor;

            this.atomicVerifiers = new[]
            {
                Verifier(NoQuestionsExist, "WB0001", VerificationMessages.WB0001_NoQuestions),
                Verifier<IQuestion>(this.HasInvalidSyntaxOfCustomValidationExpression, "WB0002", VerificationMessages.WB0002_CustomValidationExpressionHasIncorrectSyntax),
                Verifier<IComposite>(this.HasInvalidSyntaxOfCustomEnablementCondition, "WB0003", VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax),

                this.ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups,
                this.ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt,
                this.ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt,

                this.ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperPropagationLevel,

                this.ErrorsByLinkedQuestions,

                this.ErrorsByQuestionsWithSubstitutions,
            };
        }

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();

            return
                from verifier in this.atomicVerifiers
                let errors = verifier.Invoke(questionnaire)
                from error in errors
                select error;
        }

        private static AtomicVerifier Verifier(Func<QuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { new QuestionnaireVerificationError(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationError>();
        }

        private static AtomicVerifier Verifier<TEntity>(Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(QuestionnaireDocument questionnaire)
        {
            var autoPropagateQuestionsWithEmptyTriggers = questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0).ToArray();

            return this.CreateQuestionnaireVerificationErrorsForQuestions("WB0008",
                VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                autoPropagateQuestionsWithEmptyTriggers);
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithNoPropagatingQuestionsPointingToThem = questionnaire.Find<IGroup>(group
                => this.IsGroupPropagatable(@group)
                    && !this.GetPropagatingQuestionsPointingToPropagatedGroup(@group.PublicKey, questionnaire).Any());

            return
                this.CreateQuestionnaireVerificationErrorsForGroups("WB0009",
                    VerificationMessages.WB0009_PropagatedGroupHaveNoPropagatingQuestionsPointingToThem,
                    propagatedGroupsWithNoPropagatingQuestionsPointingToThem);
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroups = questionnaire.Find<IGroup>(this.IsGroupPropagatable);
            foreach (var propagatedGroup in propagatedGroups)
            {
                var propagatingQuestionsPointingToPropagatedGroup =
                    this.GetPropagatingQuestionsPointingToPropagatedGroup(propagatedGroup.PublicKey, questionnaire);

                if (propagatingQuestionsPointingToPropagatedGroup.Count() < 2)
                    continue;

                yield return
                    PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem(propagatedGroup,
                        propagatingQuestionsPointingToPropagatedGroup);
            }
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByLinkedQuestions(QuestionnaireDocument questionnaire)
        {
            var linkedQuestions = questionnaire.Find<IQuestion>(
                question => question.LinkedToQuestionId.HasValue);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var sourceQuestion = questionnaire.Find<IQuestion>(linkedQuestion.LinkedToQuestionId.Value);

                if (sourceQuestion == null)
                {
                    yield return this.QuestionReferencedByLinkedQuestionDoesNotExistError(linkedQuestion);
                    continue;
                }

                bool isSourceQuestionValidType = QuestionTypesValidToBeLinkedQuestionSource.Contains(sourceQuestion.QuestionType);
                if (!isSourceQuestionValidType)
                {
                    yield return this.LinkedQuestionReferenceQuestionOfNotSupportedTypeError(linkedQuestion, sourceQuestion);
                    continue;
                }

                var isSourceQuestionInsidePropagatedGroup = this.GetAllParentGroupsForQuestion(sourceQuestion, questionnaire).Any(this.IsGroupPropagatable);
                if (!isSourceQuestionInsidePropagatedGroup)
                {
                    yield return this.LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(linkedQuestion, sourceQuestion);
                }
            }
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithSubstitutions(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IQuestion> questionsWithSubstitutions =
                questionnaire.Find<IQuestion>(question => StringUtil.GetAllSubstitutionVariableNames(question.QuestionText).Length > 0);
            
            var errorByAllQuestionsWithSubstitutions = new List<QuestionnaireVerificationError>();

            foreach (var questionWithSubstitution in questionsWithSubstitutions)
            {
                if (questionWithSubstitution.Featured)
                {
                    errorByAllQuestionsWithSubstitutions.Add(QuestionHaveIncorrectSubstitutionCantBeFeatured(questionWithSubstitution));
                    continue;
                }

                var substitutionReferences = StringUtil.GetAllSubstitutionVariableNames(questionWithSubstitution.QuestionText);

                List<Guid> vectorOfAutopropagatedQuestionsForQuestionWithSubstitution =
                    GetAllAutopropagationQuestionsAsVector(questionWithSubstitution, questionnaire);

                this.VerifyEnumerableAndAccumulateErrorsToList(substitutionReferences,
                       (identifier) =>
                           this.GetVerificationErrorBySubstitutionReferenceOrNull(questionWithSubstitution, identifier,
                               vectorOfAutopropagatedQuestionsForQuestionWithSubstitution, questionnaire), errorByAllQuestionsWithSubstitutions);
            }

            return errorByAllQuestionsWithSubstitutions;
        }

        private IEnumerable<QuestionnaireVerificationError>
            ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperPropagationLevel(QuestionnaireDocument questionnaire)
        {
            var questionsWithValidationExpression = questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.ValidationExpression));

            var errorByAllQuestionsWithCustomValidation = new List<QuestionnaireVerificationError>();

            foreach (var questionWithValidationExpression in questionsWithValidationExpression)
            {
                IEnumerable<string> identifiersUsedInExpression =
                    expressionProcessor.GetIdentifiersUsedInExpression(questionWithValidationExpression.ValidationExpression);

                List<Guid> vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation =
                    GetAllAutopropagationQuestionsAsVector(questionWithValidationExpression, questionnaire);

                this.VerifyEnumerableAndAccumulateErrorsToList(identifiersUsedInExpression,
                    (identifier) =>
                        this.GetVerificationErrorByCustomValidationReferenceOrNull(questionWithValidationExpression, identifier,
                            vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, questionnaire), errorByAllQuestionsWithCustomValidation);
            }

            return errorByAllQuestionsWithCustomValidation;
        }

        private bool HasInvalidSyntaxOfCustomValidationExpression(IQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.ValidationExpression))
                return false;

            return !this.expressionProcessor.IsSyntaxValid(question.ValidationExpression);
        }

        private static QuestionnaireVerificationReference CreateReference(IComposite entity)
        {
            return new QuestionnaireVerificationReference(
                entity is IGroup ? QuestionnaireVerificationReferenceType.Group : QuestionnaireVerificationReferenceType.Question,
                entity.PublicKey);
        }

        private bool HasInvalidSyntaxOfCustomEnablementCondition(IComposite entity)
        {
            string customEnablementCondition = GetCustomEnablementCondition(entity);

            if (string.IsNullOrWhiteSpace(customEnablementCondition))
                return false;

            return !this.expressionProcessor.IsSyntaxValid(customEnablementCondition);
        }

        private static string GetCustomEnablementCondition(IComposite entity)
        {
            if (entity is IGroup)
                return ((IGroup)entity).ConditionExpression;
            else if (entity is IQuestion)
                return ((IQuestion)entity).ConditionExpression;
            else
                return null;
        }

        private QuestionnaireVerificationError GetVerificationErrorByCustomValidationReferenceOrNull(
            IQuestion questionWithValidationExpression, string identifier,
            List<Guid> vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, QuestionnaireDocument questionnaire)
        {
            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }

            Guid parsedId;
            if (!Guid.TryParse(identifier, out parsedId))
            {
                return ParameterUsedInValidationExpressionIsntRecognized(questionWithValidationExpression);
            }

            var questionsReferencedInValidation = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId);
            if (questionsReferencedInValidation == null)
            {
                return QuestionReferencedByQuestionCustomValidationExpressionDoesNotExist(questionWithValidationExpression);
            }

            if (this.QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(questionsReferencedInValidation,
                vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, questionnaire))
            {
                return QuestionHaveCustomValidationReferencingQuestionWithDeeperPropagationLevel(
                    questionWithValidationExpression, questionsReferencedInValidation);
            }

            return null;
        }

        private QuestionnaireVerificationError GetVerificationErrorBySubstitutionReferenceOrNull(IQuestion questionWithSubstitution,
            string substitutionReference, List<Guid> vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions,
            QuestionnaireDocument questionnaire)
        {
            if (substitutionReference == questionWithSubstitution.StataExportCaption)
            {
                return this.QuestionWithSubstitutionsCantHaveSelfReferences(questionWithSubstitution);
            }

            var questionSourceOfSubstitution =
                questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == substitutionReference);

            if (questionSourceOfSubstitution == null)
            {
                return this.QuestionReferencedByQuestionWithSubstitutionsDoesNotExist(questionWithSubstitution);
            }

            if (!QuestionTypesValidToBeSubstitutionReferences.Contains(questionSourceOfSubstitution.QuestionType))
            {
                return
                    this.QuestionsSubstitutionReferenceOfNotSupportedType(questionWithSubstitution, questionSourceOfSubstitution);
            }

            if (this.QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(questionSourceOfSubstitution,
                vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions, questionnaire))
            {
                return
                    this.QuestionWithSubstitutionsCantHaveReferencesWithDeeperPropagationLevel(questionWithSubstitution,
                        questionSourceOfSubstitution);
            }

            return null;
        }

        private QuestionnaireVerificationError ParameterUsedInValidationExpressionIsntRecognized(IQuestion questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0021",
                VerificationMessages.WB0021_ParameterUsedInValidationExpressionIsntRecognized,
                this.CreateVerificationReferenceForQuestion(questionWithValidationExpression));
        }

        private QuestionnaireVerificationError QuestionReferencedByQuestionCustomValidationExpressionDoesNotExist(IQuestion questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0020",
                VerificationMessages.WB0020_QuestionReferencedByQuestionCustomValidationExpressionDoesNotExist,
                this.CreateVerificationReferenceForQuestion(questionWithValidationExpression));
        }

        private QuestionnaireVerificationError QuestionHaveCustomValidationReferencingQuestionWithDeeperPropagationLevel(
            IQuestion questionWithValidationExpression, IQuestion questionsReferencedInValidation)
        {
            var references = new[]
            {
                this.CreateVerificationReferenceForQuestion(questionWithValidationExpression),
                this.CreateVerificationReferenceForQuestion(questionsReferencedInValidation)
            };

            return
                new QuestionnaireVerificationError("WB0014",
                    VerificationMessages.WB0014_QuestionHaveCustomValidationReferencingQuestionWithDeeperPropagationLevel,
                    references);

        }

        private QuestionnaireVerificationError QuestionHaveIncorrectSubstitutionCantBeFeatured(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0015",
                VerificationMessages.WB0015_QuestionHaveIncorrectSubstitutionCantBeFeatured,
                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                    questionsWithSubstitution.PublicKey));
        }

        private QuestionnaireVerificationError PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem(IGroup propagatedGroup, IEnumerable<IQuestion> propagatingQuestionsPointingToPropagatedGroup)
        {
            var references = new List<QuestionnaireVerificationReference> { this.CreateVerificationReferenceForGroup(propagatedGroup) };

            references.AddRange(propagatingQuestionsPointingToPropagatedGroup.Select(this.CreateVerificationReferenceForQuestion));

            return new QuestionnaireVerificationError("WB0010",
                VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem, references.ToArray());
        }

        private QuestionnaireVerificationError QuestionWithSubstitutionsCantHaveReferencesWithDeeperPropagationLevel(IQuestion questionsWithSubstitution,IQuestion questionSourceOfSubstitution)
        {
            var references = new[]
                        {
                            this.CreateVerificationReferenceForQuestion(questionsWithSubstitution),
                            this.CreateVerificationReferenceForQuestion(questionSourceOfSubstitution)
                        };

             return new QuestionnaireVerificationError("WB0019",
               VerificationMessages.WB0019_QuestionWithSubstitutionsCantHaveReferencesWithDeeperPropagationLevel, references);
        }

        private QuestionnaireVerificationError QuestionsSubstitutionReferenceOfNotSupportedType(IQuestion questionsWithSubstitution,IQuestion questionSourceOfSubstitution)
        {
            var references = new[]
                        {
                            this.CreateVerificationReferenceForQuestion(questionsWithSubstitution),
                            this.CreateVerificationReferenceForQuestion(questionSourceOfSubstitution)
                        };

             return new QuestionnaireVerificationError("WB0018",
                VerificationMessages.WB0018_QuestionsSubstitutionReferenceOfNotSupportedType, references);
        }

        private QuestionnaireVerificationError QuestionReferencedByQuestionWithSubstitutionsDoesNotExist(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0017",
                VerificationMessages.WB0017_QuestionReferencedByQuestionWithSubstitutionsDoesNotExist,
                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                    questionsWithSubstitution.PublicKey));
        }

        private QuestionnaireVerificationError QuestionWithSubstitutionsCantHaveSelfReferences(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0016",
                VerificationMessages.WB0016_QuestionWithSubstitutionsCantHaveSelfReferences,
                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                    questionsWithSubstitution.PublicKey));
        }

        private QuestionnaireVerificationError QuestionReferencedByLinkedQuestionDoesNotExistError(IQuestion linkedQuestion)
        {
            return new QuestionnaireVerificationError("WB0011",
                       VerificationMessages.WB0011_QuestionReferencedByLinkedQuestionDoesNotExist,
                       this.CreateVerificationReferenceForQuestion(linkedQuestion));
        }

        private QuestionnaireVerificationError LinkedQuestionReferenceQuestionOfNotSupportedTypeError(IQuestion linkedQuestion,IQuestion sourceQuestion)
        {
            var references = new[]
            {
                this.CreateVerificationReferenceForQuestion(linkedQuestion),
                this.CreateVerificationReferenceForQuestion(sourceQuestion)
            };

            return new QuestionnaireVerificationError("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferenceQuestionOfNotSupportedType, references);
        }

        private QuestionnaireVerificationError LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(IQuestion linkedQuestion,IQuestion sourceQuestion)
        {
            var references = new[]
            {
                this.CreateVerificationReferenceForQuestion(linkedQuestion),
                this.CreateVerificationReferenceForQuestion(sourceQuestion)
            };
            return new QuestionnaireVerificationError("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferenceQuestionNotUnderPropagatedGroup, references);
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForQuestions(string code, string message,params IQuestion[] questions)
        {
            QuestionnaireVerificationReference[] references =
                questions
                    .Select(CreateVerificationReferenceForQuestion)
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForGroups(string code, string message,params IGroup[] groups)
        {
            QuestionnaireVerificationReference[] references =
                groups
                    .Select(CreateVerificationReferenceForGroup)
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
        }

        private IEnumerable<QuestionnaireVerificationError> CreateQuestionnaireVerificationErrorsForQuestions(string code, string message,params  IQuestion[] questions)
        {
            return questions.Select(q => CreateQuestionnaireVerificationErrorForQuestions(code, message, q));
        }

        private IEnumerable<QuestionnaireVerificationError> CreateQuestionnaireVerificationErrorsForGroups(string code, string message,IEnumerable<IGroup> questions)
        {
            return questions.Select(q => CreateQuestionnaireVerificationErrorForGroups(code, message, q));
        }

        private QuestionnaireVerificationReference CreateVerificationReferenceForGroup(IGroup group)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, group.PublicKey);
        }

        private QuestionnaireVerificationReference CreateVerificationReferenceForQuestion(IQuestion question)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, question.PublicKey);
        }

        private void VerifyEnumerableAndAccumulateErrorsToList<T>(IEnumerable<T> enumerableOfTelemetryToVerification,
            Func<T, QuestionnaireVerificationError> getByEnumerableItemErrorOrNull, List<QuestionnaireVerificationError> errorList)
        {
            errorList.AddRange(enumerableOfTelemetryToVerification.Select(getByEnumerableItemErrorOrNull).Where(errorOrNull => errorOrNull != null));
        }

        private static bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private bool IsGroupPropagatable(IGroup group)
        {
            return group.Propagated == Propagate.AutoPropagated;
        }

        private IEnumerable<IQuestion> GetPropagatingQuestionsPointingToPropagatedGroup(Guid groupId, QuestionnaireDocument document)
        {
            return document.Find<IAutoPropagateQuestion>(question => question.Triggers.Contains(groupId));
        }

        private IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, QuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private List<Guid> GetAllAutopropagationQuestionsAsVector(IQuestion question, QuestionnaireDocument questionnaire)
        {
            List<Guid> propagationQuestions = GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), questionnaire)
                .Where(IsGroupPropagatable)
                .Select(g => GetPropagatingQuestionsPointingToPropagatedGroup(g.PublicKey, questionnaire).FirstOrDefault().PublicKey)
                .ToList();

            return propagationQuestions;
        }

        private IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }

        private bool QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(IQuestion question,
            List<Guid> vectorOfAutopropagatedQuestions, QuestionnaireDocument questionnaire)
        {
            List<Guid> autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution =
                this.GetAllAutopropagationQuestionsAsVector(question, questionnaire);
         
            return autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution.Count > 0
                &&
                autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution.Except(
                    vectorOfAutopropagatedQuestions).Any();
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }
    }
}