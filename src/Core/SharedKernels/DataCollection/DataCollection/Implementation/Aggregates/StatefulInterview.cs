﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Events;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class StatefulInterview : Interview, IStatefulInterview
    {
        public StatefulInterview(IQuestionnaireStorage questionnaireRepository,
                                 IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider,
                                 ISubstitionTextFactory substitionTextFactory)
            : base(questionnaireRepository, expressionProcessorStatePrototypeProvider, substitionTextFactory)
        {
        }

        #region Apply

        public override void Apply(InterviewCreated @event)
        {
            base.Apply(@event);
            this.sourceInterview = this.Tree.Clone();
        }

        public override void Apply(InterviewFromPreloadedDataCreated @event)
        {
            base.Apply(@event);
            this.sourceInterview = this.Tree.Clone();
        }

        public override void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId,
                @event.QuestionnaireVersion);

            this.CreatedOnClient = true;
            this.properties.InterviewerId = @event.UserId;
            this.UsesExpressionStorage = @event.UsesExpressionStorage;
            this.sourceInterview = this.Tree.Clone();
        }

        public void Apply(InterviewSynchronized @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.InterviewData.QuestionnaireId,
                @event.InterviewData.QuestionnaireVersion);

            this.sourceInterview = this.Tree.Clone();
            this.Tree.ActualizeTree();

            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;
            
            foreach (var answerDto in @event.InterviewData.Answers.OrderBy(x => x.QuestionRosterVector.Length))
            {
                var question = this.Tree.GetQuestion(Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

                if (question == null)
                {
                    //now answers from removed rosters are in sync package.
                    continue;    
                }

                if (answerDto.Answer != null)
                {
                    question.SetObjectAnswer(answerDto.Answer);
                }

                if (answerDto.AllComments != null)
                    question.AnswerComments = answerDto.AllComments.Select(commentDto => ToAnswerComment(answerDto, commentDto)).ToList();

                this.ActualizeRostersIfQuestionIsRosterSize(answerDto.Id);
            }

            if (this.UsesExpressionStorage)
            {
                this.UpdateTreeWithDependentChanges(this.Tree, this.GetQuestionnaireOrThrow());
            }
            else
            {
                // titles for numeric questions should be recalculated afterward because answers for roster title questions
                // could be not processed by the time roster instance was created.
                this.Tree.FindRosters().Where(x => x.IsNumeric).ForEach(x => x.UpdateRosterTitle());

                foreach (var disabledGroup in @event.InterviewData.DisabledGroups)
                    this.Tree.GetGroup(Identity.Create(disabledGroup.Id, disabledGroup.InterviewItemRosterVector))?.Disable();

                foreach (var disabledQuestion in @event.InterviewData.DisabledQuestions)
                    this.Tree.GetQuestion(Identity.Create(disabledQuestion.Id, disabledQuestion.InterviewItemRosterVector))?.Disable();

                foreach (var invalidQuestion in @event.InterviewData.FailedValidationConditions)
                    this.Tree.GetQuestion(invalidQuestion.Key)?.MarkInvalid(invalidQuestion.Value);

                foreach (var disabledStaticText in @event.InterviewData.DisabledStaticTexts)
                    this.Tree.GetStaticText(disabledStaticText)?.Disable();

                foreach (var invalidStaticText in @event.InterviewData.InvalidStaticTexts)
                    this.Tree.GetStaticText(invalidStaticText.Key)?.MarkInvalid(invalidStaticText.Value);

                foreach (var validStaticText in @event.InterviewData.ValidStaticTexts)
                    this.Tree.GetStaticText(validStaticText)?.MarkValid();

                foreach (var variable in @event.InterviewData.Variables)
                    this.Tree.GetVariable(Identity.Create(variable.Key.Id, variable.Key.InterviewItemRosterVector))?.SetValue(variable.Value);

                foreach (var disabledVariable in @event.InterviewData.DisabledVariables)
                    this.Tree.GetVariable(Identity.Create(disabledVariable.Id, disabledVariable.InterviewItemRosterVector))?.Disable();

                this.Tree.ReplaceSubstitutions();

            
                CalculateLinkedToListOptionsOnTree(this.Tree, this.ExpressionProcessorStatePrototype, false);

                base.UpdateExpressionState(this.sourceInterview, this.Tree, this.ExpressionProcessorStatePrototype);

                this.UpdateLinkedQuestions(this.Tree, this.ExpressionProcessorStatePrototype, false);
            }
            this.CreatedOnClient = @event.InterviewData.CreatedOnClient;
            this.properties.SupervisorId = @event.InterviewData.SupervisorId;
            this.properties.InterviewerId = @event.InterviewData.UserId;
            this.SupervisorRejectComment = @event.InterviewData.Comments;

            this.sourceInterview = this.Tree.Clone();
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {
            
        }

        public new void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.Tree.GetGroup(@group)?.ReplaceSubstitutions();

            foreach (var @question in @event.Questions)
                this.Tree.GetQuestion(@question)?.ReplaceSubstitutions();

            foreach (var @staticText in @event.StaticTexts)
                this.Tree.GetStaticText(@staticText)?.ReplaceSubstitutions();
        }

        public new void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);

            this.InterviewerCompleteComment = @event.Comment;
            this.IsCompleted = true;
        }

        public new void Apply(InterviewRejected @event)
        {
            base.Apply(@event);
            this.SupervisorRejectComment = @event.Comment;
        }

        public new void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
            this.IsCompleted = false;
        }

        public new void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            this.HasErrors = false;
        }

        public new void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.HasErrors = true;
        }

        #endregion

        private InterviewTree sourceInterview;

        public DateTime? StartedDate => this.properties.StartedDate;
        public DateTime? CompletedDate => this.properties.CompletedDate;
        public InterviewStatus Status => this.properties.Status;
        public bool IsDeleted => this.properties.IsHardDeleted || this.Status == InterviewStatus.Deleted;

        public Guid Id => this.EventSourceId;
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public string GetAnswerAsString(Identity questionIdentity, CultureInfo cultureInfo = null)
        {
            var question = this.Tree.GetQuestion(questionIdentity);

            return question.GetAnswerAsString(cultureInfo ?? CultureInfo.InvariantCulture);
        }

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }
        public InterviewTreeGroup GetGroup(Identity identity) => this.Tree.GetGroup(identity);
        public InterviewTreeRoster GetRoster(Identity identity) => this.Tree.GetRoster(identity);
        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsGps;
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsDateTime;
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultimedia;
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsQRBarcode;
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsTextList;
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiFixedOption;
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsInteger;
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsDouble;
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsText;
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleFixedOption;
        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsYesNo;

        public InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleLinkedToList;
        public InterviewTreeQuestion GetQuestion(Identity identity) => this.Tree.GetQuestion(identity);
        public InterviewTreeStaticText GetStaticText(Identity identity) => this.Tree.GetStaticText(identity);

        public InterviewTreeMultiOptionLinkedToListQuestion GetMultiOptionLinkedToListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiLinkedToList;
        
        public IEnumerable<InterviewTreeSection> GetEnabledSections() => this.Tree.Sections.Where(s => !s.IsDisabled());

        #region Command handlers

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId, InterviewKey key, int? assignmentId, Dictionary<Guid, AbstractAnswer> answersToIdentifyingQuestions)
        {
            this.QuestionnaireIdentity = questionnaireIdentity;
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            answersToIdentifyingQuestions = answersToIdentifyingQuestions ?? new Dictionary<Guid, AbstractAnswer>();

            var changedInterviewTree = this.Tree.Clone();

            this.ValidatePrefilledAnswers(this.Tree, questionnaire, answersToIdentifyingQuestions, RosterVector.Empty);

            Dictionary<Identity, AbstractAnswer> prefilledQuestionsWithAnswers = answersToIdentifyingQuestions.ToDictionary(x => new Identity(x.Key, RosterVector.Empty), x => x.Value);
            foreach (KeyValuePair<Identity, AbstractAnswer> answer in prefilledQuestionsWithAnswers)
            {
                changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
            }

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireIdentity.QuestionnaireId, questionnaire.Version, assignmentId, questionnaire.IsUsingExpressionStorage()));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyEvents(treeDifference, userId);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId, answersTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));

            this.ApplyInterviewKey(key);
        }

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            var treeDifference = FindDifferenceBetweenTrees(this.sourceInterview, this.Tree);

            this.ApplyPassiveEvents(treeDifference);

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(this.HasInvalidAnswers() || this.HasInvalidStaticTexts
                ? new InterviewDeclaredInvalid() as IEvent
                : new InterviewDeclaredValid());
        }

        public void Synchronize(SynchronizeInterviewCommand command)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(command.SynchronizedInterview.QuestionnaireId, command.SynchronizedInterview.QuestionnaireVersion);

            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            base.CreateInterviewFromSynchronizationMetadata(command.SynchronizedInterview.Id,
                command.UserId,
                command.SynchronizedInterview.QuestionnaireId,
                command.SynchronizedInterview.QuestionnaireVersion,
                command.InitialStatus,
                command.FeaturedQuestionsMeta,
                command.SynchronizedInterview.Comments,
                command.SynchronizedInterview.RejectDateTime,
                command.SynchronizedInterview.InterviewerAssignedDateTime,
                true,
                command.CreatedOnClient,
                questionnaire.IsUsingExpressionStorage()
            );

            this.ApplyEvent(new InterviewSynchronized(command.SynchronizedInterview));
        }

        public void CreateInterviewFromSnapshot(CreateInterviewFromSnapshotCommand command)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(command.SynchronizedInterview.QuestionnaireId, command.SynchronizedInterview.QuestionnaireVersion);
            this.QuestionnaireIdentity = questionnaireIdentity;
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();

            base.CreateInterviewFromSynchronizationMetadata(command.InterviewId,
                command.UserId,
                command.SynchronizedInterview.QuestionnaireId,
                command.SynchronizedInterview.QuestionnaireVersion,
                command.SynchronizedInterview.Status,
                new AnsweredQuestionSynchronizationDto[0], 
                command.SynchronizedInterview.Comments,
                command.SynchronizedInterview.RejectDateTime,
                command.SynchronizedInterview.InterviewerAssignedDateTime,
                true,
                command.SynchronizedInterview.CreatedOnClient,
                questionnaire.IsUsingExpressionStorage()
            );

            this.ApplyEvent(new InterviewSynchronized(command.SynchronizedInterview));

            var answeredQuestions = this.Tree.AllNodes.OfType<InterviewTreeQuestion>().Where(q => q.IsAnswered()).Select(q => q.Identity);
            this.UpdateTreeWithDependentChanges(this.Tree, questionnaire);
        }



        #endregion

        public bool HasGroup(Identity group) => this.Tree.GetGroup(group) != null;

        public string GetRosterTitle(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity)?.RosterTitle;

        public string GetTitleText(Identity entityIdentity)
            => this.GetTitleSubstitutionText(entityIdentity)?.Text ?? string.Empty;

        public string GetBrowserReadyTitleHtml(Identity entityIdentity)
            => this.GetTitleSubstitutionText(entityIdentity)?.BrowserReadyText ?? string.Empty;

        private SubstitionText GetTitleSubstitutionText(Identity entityIdentity)
            => this.Tree.GetQuestion(entityIdentity)?.Title
            ?? this.Tree.GetGroup(entityIdentity)?.Title
            ?? this.Tree.GetStaticText(entityIdentity)?.Title;

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
        {
            do
            {
                var variableIdentity = new Identity(variableId, variableRosterVector);

                var variable = this.Tree.GetVariable(variableIdentity);
                if (variable != null) return variable.Value;

                if (variableRosterVector.Length == 0) break;

                variableRosterVector = variableRosterVector.Shrink(variableRosterVector.Length - 1);
            } while (variableRosterVector.Length >= 0);

            return null;
        }

        public InterviewTreeQuestion FindQuestionInQuestionBranch(Guid entityId, Identity questionIdentity)
            => this.Tree.FindEntityInQuestionBranch(entityId, questionIdentity) as InterviewTreeQuestion;

        public IEnumerable<Identity> FindQuestionsFromSameOrDeeperLevel(Guid entityId, Identity questionIdentity)
            => this.Tree.FindEntitiesFromSameOrDeeperLevel(entityId, questionIdentity);

        public bool IsQuestionPrefilled(Identity entityIdentity)
        {
            return this.Tree.GetQuestion(entityIdentity)?.IsPrefilled ?? false;
        }

        public IEnumerable<Identity> GetAllIdentitiesForEntityId(Guid id)
            => this.Tree.AllNodes.Where(node => node.Identity.Id == id).Select(node => node.Identity);

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.Tree.GetQuestion(questionIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public int GetGroupsInGroupCount(Identity group) => this.GetGroupsAndRostersInGroup(group).Count();

        private IEnumerable<InterviewTreeQuestion> GetEnabledQuestions()
            => this.Tree.FindQuestions().Where(question => !question.IsDisabled());

        private IEnumerable<InterviewTreeQuestion> GetEnabledInterviewerQuestions()
            => this.GetEnabledQuestions().Where(question =>
                question.IsInterviewer && (!question.IsPrefilled || (question.IsPrefilled && this.CreatedOnClient)));

        public int CountActiveAnsweredQuestionsInInterview()
            => this.GetEnabledInterviewerQuestions().Count(question => question.IsAnswered());

        public int CountActiveQuestionsInInterview() => this.GetEnabledInterviewerQuestions().Count();

        public int CountAllEnabledAnsweredQuestions()
            => this.GetEnabledQuestions().Count(question => question.IsAnswered());
        public int CountAllEnabledQuestions() => this.GetEnabledQuestions().Count();
        public int CountAllInvalidEntities() => this.GetAllInvalidEntitiesInInterview().Count();

        public int CountEnabledSupervisorQuestions()
            => this.GetEnabledQuestions().Count(question => question.IsSupervisors);

        public int CountEnabledHiddenQuestions() => this.GetEnabledQuestions().Count(question => question.IsHidden);

        public int CountInvalidEntitiesInInterview() => this.GetInvalidEntitiesInInterview().Count();

        public IEnumerable<Identity> GetAllInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts()
                .Concat(this.GetEnabledInvalidQuestions().Select(question => question.Identity));

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts()
                .Concat(this.GetEnabledInvalidQuestions().Where(question => question.IsInterviewer)
                    .Select(question => question.Identity));

        public bool IsFirstEntityBeforeSecond(Identity first, Identity second)
        {
            return first == this.Tree.GetAllNodesInEnumeratorOrder()
                               .Select(node => node.Identity)
                               .FirstOrDefault(identity => identity == first || identity == second);
        }

        private IEnumerable<Identity> GetEnabledInvalidStaticTexts()
            => this.Tree.FindStaticTexts()
                .Where(staticText => !staticText.IsDisabled() && !staticText.IsValid)
                .Select(staticText => staticText.Identity);

        private IEnumerable<InterviewTreeQuestion> GetEnabledInvalidQuestions()
            => this.Tree.FindQuestions()
                .Where(question => !question.IsDisabled() 
                                && !question.IsValid
                                && (!question.IsPrefilled || (question.IsPrefilled && this.CreatedOnClient)));

        public int CountEnabledQuestions(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledQuestions() ?? 0;

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledAnsweredQuestions() ?? 0;

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledInvalidQuestionsAndStaticTexts() ?? 0;

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CountEnabledInvalidQuestionsAndStaticTexts(group) > 0;

        public bool HasUnansweredQuestions(Identity group) 
            => this.Tree.GetGroup(group)?.HasUnansweredQuestions() ?? false;

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsVisibledToInterviewer()
        {
            var allCommentedQuestions = this.GetCommentedBySupervisorAllQuestions();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            return allCommentedQuestions.Where(identity => questionnaire.IsInterviewierQuestion(identity.Id));
        }

        public IEnumerable<Identity> GetCommentedBySupervisorAllQuestions()
        {
            return this.Tree.FindQuestions()
                .Where(this.IsEnabledWithSupervisorComments)
                .Select(x => new
                {
                    Id = x.Identity,
                    HasInterviewerReply = this.HasSupervisorCommentInterviewerReply(x)
                })
                .OrderBy(x => x.HasInterviewerReply)
                .Select(x => x.Id);
        }

        public IEnumerable<Identity> GetAllCommentedEnabledQuestions()
        {
            return this.Tree.FindQuestions()
                .Where(question => !question.IsDisabled() && question.AnswerComments.Any())
                .Select(x => x.Identity);
        }

        private bool IsEnabledWithSupervisorComments(InterviewTreeQuestion question)
            => !question.IsDisabled() &&
               question.AnswerComments.Any(y => y.UserId != this.properties.InterviewerId);

        public string GetLastSupervisorComment() => this.SupervisorRejectComment;

        private bool HasSupervisorCommentInterviewerReply(InterviewTreeQuestion question)
        {
            var indexOfLastNotInterviewerComment = question.AnswerComments.FindLastIndex(0, x => x.UserId != this.properties.InterviewerId);
            return question.AnswerComments.Skip(indexOfLastNotInterviewerComment + 1).Any();
        }

        public Identity GetParentGroup(Identity groupOrQuestion)
            => this.Tree.GetNodeByIdentity(groupOrQuestion)?.Parent?.Identity;

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.GetAllChildrenOrEmptyList(groupIdentity)
                .OfType<InterviewTreeQuestion>()
                .Select(question => question.Identity);

        private IEnumerable<IInterviewTreeNode> GetAllChildrenOrEmptyList(Identity groupIdentity)
            => this.Tree.GetGroup(groupIdentity)?.Children ?? new List<IInterviewTreeNode>();

        public IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.GetAllChildrenOrEmptyList(parentIdentity)
                .Where(roster => roster.Identity.Id == rosterId)
                .Select(roster => roster.Identity)
                .ToList();

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
            => this.GetGroupsAndRostersInGroup(group)
                .Where(groupOrRoster => !groupOrRoster.IsDisabled())
                .Select(groupOrRoster => groupOrRoster.Identity);

        private IEnumerable<InterviewTreeGroup> GetGroupsAndRostersInGroup(Identity group)
            => this.Tree.GetGroup(group)?.Children?.OfType<InterviewTreeGroup>() ?? new InterviewTreeGroup[0];

        public IEnumerable<InterviewTreeGroup> GetAllGroupsAndRosters()
            => this.Tree.GetAllNodesInEnumeratorOrder().OfType<InterviewTreeGroup>();

        public InterviewTreeSection FirstSection => this.Tree.Sections.First();

        public Guid CurrentResponsibleId
        {
            get
            {
                var result = this.properties.InterviewerId ?? this.properties.SupervisorId;
                if (result == null) throw new InterviewException($"Interview has no responsible assigned. Interview key: {this.interviewKey}");
                return result.Value;
            }
        }

        public IEnumerable<InterviewTreeGroup> GetAllEnabledGroupsAndRosters()
            => this.Tree.GetAllNodesInEnumeratorOrder().OfType<InterviewTreeGroup>().Where(group => !group.IsDisabled());

        public IEnumerable<IInterviewTreeNode> GetAllNodes() => this.Tree.GetAllNodesInEnumeratorOrder();

        public bool IsEntityValid(Identity identity)
        {
            var question = this.Tree.GetQuestion(identity);
            if (question != null)
            {
                return !question.IsAnswered() || question.IsValid;
            }
            var staticText = this.Tree.GetStaticText(identity);
            return staticText?.IsValid ?? false;
        }

        public IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId, string defaltErrorMessageFallback)
        {
            var question = this.Tree.GetQuestion(questionOrStaticTextId);
            if (question?.FailedValidations != null)
            {
                var questionValidationMassages = question.ValidationMessages
                    .Select(substitutionText => string.IsNullOrWhiteSpace(substitutionText.Text) ? defaltErrorMessageFallback : substitutionText.Text)
                    .ToList();

                if (questionValidationMassages.Count == 1) return new[] {questionValidationMassages[0]};

                return question.FailedValidations.Select(failedValidation =>
                    $"{questionValidationMassages.ElementAt(failedValidation.FailedConditionIndex)} " +
                    $"[{failedValidation.FailedConditionIndex + 1}]");

            }

            var staticText =  this.Tree.GetStaticText(questionOrStaticTextId);
            if (staticText?.FailedValidations != null)
            {
                var staticTextValidationMassages = staticText.ValidationMessages
                    .Select(substitutionText => string.IsNullOrWhiteSpace(substitutionText.Text) ? defaltErrorMessageFallback : substitutionText.Text)
                    .ToList();

                if (staticTextValidationMassages.Count == 1) return new[] {staticTextValidationMassages[0]};

                return staticText.FailedValidations.Select(failedValidation =>
                    $"{staticTextValidationMassages.ElementAt(failedValidation.FailedConditionIndex)} " +
                    $"[{failedValidation.FailedConditionIndex + 1}]");
            }

            return Enumerable.Empty<string>();
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var node = this.Tree.GetNodeByIdentity(entityIdentity);
            // Not being disposed of ViewModels can try to update their state, but they can be removed from the tree already if roster was removed.
            if (node == null) return false;
            return !node.IsDisabled();
        }

        public bool CreatedOnClient { get; private set; } = false;

        public bool WasAnswered(Identity entityIdentity)
        {
            var question = this.Tree.GetQuestion(entityIdentity);

            return question != null && !question.IsDisabled() && question.IsAnswered();
        }

        public IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity)
            => this.Tree.GetQuestion(entityIdentity).AnswerComments;

        List<CategoricalOption> IStatefulInterview.GetTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int sliceSize)
            => this.GetFirstTopFilteredOptionsForQuestion(question, parentQuestionValue, filter, sliceSize);

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithoutFilter(Identity question, int value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithoutFilter(question, value, parentQuestionValue);

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithFilter(Identity question, string value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithFilter(question, value, parentQuestionValue);

        public int CountCommentedQuestionsVisibledToInterviewer() => this.GetCommentedBySupervisorQuestionsVisibledToInterviewer().Count();

        private static AnswerComment ToAnswerComment(AnsweredQuestionSynchronizationDto answerDto, CommentSynchronizationDto commentDto)
            => new AnswerComment(
                userId: commentDto.UserId,
                userRole: commentDto.UserRole,
                commentTime: commentDto.Date,
                comment: commentDto.Text,
                questionIdentity: Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

        public bool AcceptsInterviewerAnswers()
        {
            return !IsDeleted && Status == InterviewStatus.InterviewerAssigned;
        }

        public IReadOnlyCollection<IInterviewTreeNode> GetAllSections()
        {
            return this.Tree.Sections;
        }

        public InterviewSynchronizationDto GetSynchronizationDto()
        {
            var answeredQuestions = new List<AnsweredQuestionSynchronizationDto>();
            var disabledGroups = new HashSet<InterviewItemId>();
            var disabledQuestions = new HashSet<InterviewItemId>();
            var disabledStaticTexts = new List<Identity>();
            var validAnsweredQuestions = new HashSet<InterviewItemId>();
            var invalidAnsweredQuestions = new HashSet<InterviewItemId>();
            var validStaticTexts = new List<Identity>();
            var invalidStaticTexts = new List<KeyValuePair<Identity, List<FailedValidationCondition>>>();
            var failedValidationConditions = new Dictionary<Identity, IList<FailedValidationCondition>>();

            foreach (var question in Tree.FindQuestions())
            {
                CommentSynchronizationDto[] comments = question.AnswerComments.Select(ac => new CommentSynchronizationDto()
                {
                    Date = ac.CommentTime,
                    Text = ac.Comment,
                    UserId = ac.UserId,
                    UserRole = ac.UserRole
                }).ToArray();

                var answeredQuestion = new AnsweredQuestionSynchronizationDto(question.Identity.Id,
                    question.Identity.RosterVector,
                    GetAnswerAsObject(question),
                    comments);

                if (question.IsAnswered() || answeredQuestion.HasComments())
                {
                    answeredQuestions.Add(answeredQuestion);
                }

                if (question.IsDisabled())
                {
                    disabledQuestions.Add(new InterviewItemId(question.Identity.Id, question.Identity.RosterVector));
                }
                else
                {
                    if (question.IsAnswered() && !question.IsValid)
                    {
                        invalidAnsweredQuestions.Add(new InterviewItemId(question.Identity.Id, question.Identity.RosterVector));
                        failedValidationConditions.Add(question.Identity, question.FailedValidations.ToList());
                    }
                    if (question.IsValid)
                    {
                        validAnsweredQuestions.Add(new InterviewItemId(question.Identity.Id, question.Identity.RosterVector));
                    }
                }
            }

            foreach (var group in Tree.AllNodes.OfType<InterviewTreeGroup>())
            {
                if (group.IsDisabled())
                    disabledGroups.Add(new InterviewItemId(group.Identity.Id, group.Identity.RosterVector));
            }

            foreach (var staticText in Tree.FindStaticTexts())
            {
                var staticTextIdentity = staticText.Identity;
                if (!staticText.IsDisabled())
                {
                    if (!staticText.IsValid)
                    {
                        invalidStaticTexts.Add(new KeyValuePair<Identity, List<FailedValidationCondition>>(
                            staticTextIdentity, staticText.FailedValidations.ToList()));
                    }
                }
                else
                {
                    disabledStaticTexts.Add(staticTextIdentity);
                }
            }

            Dictionary<InterviewItemId, object> variableValues = new Dictionary<InterviewItemId, object>();
            HashSet<InterviewItemId> disabledVariables = new HashSet<InterviewItemId>();

            foreach (var variable in Tree.AllNodes.OfType<InterviewTreeVariable>())
            {
                if (variable.IsDisabled())
                {
                    disabledVariables.Add(new InterviewItemId(variable.Identity.Id, variable.Identity.RosterVector));
                }
                else if (variable.HasValue)
                {
                    variableValues.Add(new InterviewItemId(variable.Identity.Id, variable.Identity.RosterVector), variable.Value);
                }
            }

            return new InterviewSynchronizationDto(
                id: Id,
                status: Status,
                comments: SupervisorRejectComment,
                rejectDateTime:  this.properties.RejectDateTime,
                interviewerAssignedDateTime : this.properties.InterviewerAssignedDateTime,
                userId: CurrentResponsibleId,
                supervisorId: this.properties.SupervisorId,
                questionnaireId: QuestionnaireIdentity.QuestionnaireId,
                questionnaireVersion: QuestionnaireIdentity.Version,
                answers: answeredQuestions.ToArray(),
                disabledGroups: disabledGroups,
                disabledQuestions: disabledQuestions,
                disabledStaticTexts: disabledStaticTexts,
                validAnsweredQuestions: validAnsweredQuestions,
                invalidAnsweredQuestions: invalidAnsweredQuestions,
                validStaticTexts: validStaticTexts,
                invalidStaticTexts: invalidStaticTexts,
                rosterGroupInstances: null /* Obsolete */,
                failedValidationConditions: failedValidationConditions.ToList(),
                linkedQuestionOptions: null /* Obsolete */,
                variables: variableValues,
                disabledVariables: disabledVariables,
                wasCompleted: this.properties.WasCompleted,
                createdOnClient: CreatedOnClient);
        }

        public bool IsReadOnlyQuestion(Identity identity)
        {
            return false;
        }

        private object GetAnswerAsObject(InterviewTreeQuestion question)
        {
            if (!question.IsAnswered()) return null;

            if (question.IsText) return question.AsText.GetAnswer()?.Value;
            if (question.IsMultimedia) return question.AsMultimedia.GetAnswer()?.FileName;
            if (question.IsQRBarcode) return question.AsQRBarcode.GetAnswer()?.DecodedText;
            if (question.IsInteger) return question.AsInteger.GetAnswer()?.Value;
            if (question.IsDouble) return question.AsDouble.GetAnswer()?.Value;
            if (question.IsDateTime) return question.AsDateTime.GetAnswer()?.Value;
            if (question.IsGps) return question.AsGps.GetAnswer()?.Value;
            if (question.IsTextList) return question.AsTextList.GetAnswer()?.ToTupleArray();
            if (question.IsSingleLinkedOption) return question.AsSingleLinkedOption.GetAnswer()?.SelectedValue;
            if (question.IsMultiLinkedOption) return question.AsMultiLinkedOption.GetAnswer()?.CheckedValues;
            if (question.IsSingleFixedOption) return question.AsSingleFixedOption.GetAnswer()?.SelectedValue;
            if (question.IsMultiFixedOption) return question.AsMultiFixedOption.GetAnswer()?.ToDecimals()?.ToArray();
            if (question.IsYesNo) return question.AsYesNo.GetAnswer()?.ToAnsweredYesNoOptions()?.ToArray();
            if (question.IsSingleLinkedToList) return question.AsSingleLinkedToList.GetAnswer().SelectedValue;
            if (question.IsMultiLinkedToList) return question.AsMultiLinkedToList.GetAnswer()?.ToDecimals()?.ToHashSet();

            return null;
        }
    }
}