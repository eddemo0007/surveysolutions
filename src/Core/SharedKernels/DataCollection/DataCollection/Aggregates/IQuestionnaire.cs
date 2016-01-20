﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Main.Core.Entities.SubEntities;

using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public interface IQuestionnaire
    {
        /// <summary>
        /// Gets the current version of the instance as it is known in the event store.
        /// </summary>
        long Version { get; }

        string Title { get; }

        Guid? ResponsibleId { get; }

        void InitializeQuestionnaireDocument();

        [Obsolete("This method is for import service only and should be removed at all.")]
        IQuestion GetQuestionByStataCaption(string stataCaption);

        bool HasQuestion(Guid questionId);

        bool HasGroup(Guid groupId);

        QuestionType GetQuestionType(Guid questionId);

        QuestionScope GetQuestionScope(Guid questionId);

        AnswerType GetAnswerType(Guid questionId);

        bool IsQuestionLinked(Guid questionId);

        string GetQuestionTitle(Guid questionId);

        string GetQuestionVariableName(Guid questionId);

        string GetGroupTitle(Guid groupId);

        string GetStaticText(Guid staticTextId);

        Guid? GetCascadingQuestionParentId(Guid questionId);

        IEnumerable<decimal> GetAnswerOptionsAsValues(Guid questionId);

        string GetAnswerOptionTitle(Guid questionId, decimal answerOptionValue);

        decimal GetCascadingParentValue(Guid questionId, decimal answerOptionValue);

        int? GetMaxSelectedAnswerOptions(Guid questionId);

        int GetMaxRosterRowCount();

        bool IsCustomValidationDefined(Guid questionId);

        bool IsQuestion(Guid entityId);

        bool IsInterviewierQuestion(Guid questionId);

        string GetCustomValidationExpression(Guid questionId);

        ReadOnlyCollection<Guid> GetPrefilledQuestions();

        IEnumerable<Guid> GetAllParentGroupsForQuestion(Guid questionId);

        ReadOnlyCollection<Guid> GetParentsStartingFromTop(Guid entityId);

        Guid? GetParentGroup(Guid groupOrQuestionId);

        string GetCustomEnablementConditionForQuestion(Guid questionId);

        string GetCustomEnablementConditionForGroup(Guid groupId);

        bool ShouldQuestionSpecifyRosterSize(Guid questionId);

        IEnumerable<Guid> GetRosterGroupsByRosterSizeQuestion(Guid questionId);

        int? GetListSizeForListQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedQuestion(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedEntity(Guid questionId);

        IEnumerable<Guid> GetRostersFromTopToSpecifiedGroup(Guid groupId);

        IEnumerable<Guid> GetFixedRosterGroups(Guid? parentRosterId = null);

        Guid[] GetRosterSizeSourcesForQuestion(Guid questionId);

        int GetRosterLevelForQuestion(Guid questionId);

        int GetRosterLevelForGroup(Guid groupId);

        int GetRosterLevelForEntity(Guid entityId);

        bool IsRosterGroup(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingQuestions(Guid groupId);

        ReadOnlyCollection<Guid> GetAllUnderlyingInterviewerQuestions(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildGroupsAndRosters(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildGroups(Guid groupId);

        IEnumerable<Guid> GetAllUnderlyingChildRosters(Guid groupId);

        Guid GetQuestionReferencedByLinkedQuestion(Guid linkedQuestionId);
        
        bool IsQuestionInteger(Guid questionId);

        bool IsQuestionYesNo(Guid questionId);

        int? GetCountOfDecimalPlacesAllowedByQuestion(Guid questionId);

        FixedRosterTitle[] GetFixedRosterTitles(Guid groupId);

        bool DoesQuestionSpecifyRosterTitle(Guid questionId);

        IEnumerable<Guid> GetRostersAffectedByRosterTitleQuestion(Guid questionId);

        bool IsRosterTitleQuestionAvailable(Guid rosterId);

        IEnumerable<Guid> GetNestedRostersOfGroupById(Guid rosterId);

        Guid? GetRosterSizeQuestion(Guid rosterId);

        IEnumerable<Guid> GetCascadingQuestionsThatDependUponQuestion(Guid questionId);

        IEnumerable<Guid> GetCascadingQuestionsThatDirectlyDependUponQuestion(Guid id);

        IEnumerable<Guid> GetAllChildCascadingQuestions();

        bool DoesCascadingQuestionHaveOptionsForParentValue(Guid questionId, decimal parentValue);

        IEnumerable<Guid> GetAllSections();

        /// <summary>
        /// Gets list of question ids that use question with provided <param name="questionId">questionId</param> as a substitution
        /// </summary>
        /// <param name="questionId">Substituted question id</param>
        /// <returns>List of questions that depend on provided question</returns>
        IEnumerable<Guid> GetSubstitutedQuestions(Guid questionId);

        /// <summary>
        /// Gets first level child questions of a group
        /// </summary>
        ReadOnlyCollection<Guid> GetChildQuestions(Guid groupId);

        /// <summary>
        /// Gets first level child entities of a group
        /// </summary>
        ReadOnlyCollection<Guid> GetChildEntityIds(Guid groupId);

        ReadOnlyCollection<Guid> GetChildInterviewerQuestions(Guid groupId);
    }
}