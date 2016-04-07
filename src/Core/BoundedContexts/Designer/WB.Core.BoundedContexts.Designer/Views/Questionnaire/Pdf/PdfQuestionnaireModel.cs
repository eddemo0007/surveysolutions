﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfSettings
    {
        public PdfSettings(int instructionsExcerptLength, int expressionExcerptLength, int optionsExcerptCount, int minAmountOfDigitsInCodes, int attachmentSize)
        {
            InstructionsExcerptLength = instructionsExcerptLength;
            ExpressionExcerptLength = expressionExcerptLength;
            OptionsExcerptCount = optionsExcerptCount;
            MinAmountOfDigitsInCodes = minAmountOfDigitsInCodes;
            AttachmentSize = attachmentSize;
        }

        public int InstructionsExcerptLength { get; }
        public int ExpressionExcerptLength { get; }
        public int OptionsExcerptCount { get; }
        public int MinAmountOfDigitsInCodes { get; }
        public int AttachmentSize { get; set; }
    }

    public class PdfQuestionnaireModel
    {
        public class ModificationStatisticsByUser
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public DateTime? Date { get; set; }

            public DateTime? On => this.Date;
            public string By => this.Name;
        }

        public class GroupStatistics
        {
            public int GroupsCount { get; set; } = 0;
            public int RostersCount { get; set; } = 0;
            public int QuestionsCount { get; set; } = 0;
            public int StaticTextsCount { get; set; } = 0;
        }

        public class QuestionnaireStatistics : GroupStatistics
        {
            public int SectionsCount { get; set; } = 0;
            public int QuestionsWithConditionsCount { get; set; } = 0;
        }

        public class EntityWithLongCondition
        {
            public  int Index { get; set; }
            public Guid Id { get; set; }
            public string VariableName { get; set; }
            public string Title { get; set; }
            public string EnablementCondition { get; set; }
        }

        public class EntityWithLongValidation
        {
            public int Index { get; set; }
            public Guid Id { get; set; }
            public string VariableName { get; set; }
            public string Title { get; set; }
            public List<ValidationCondition> ValidationConditions { get; set; }
        }

        private readonly QuestionnaireDocument questionnaire;
        public PdfSettings Settings { get; }

        public ModificationStatisticsByUser Created { get; set; }
        public ModificationStatisticsByUser LastModified { get; set; }
        public ModificationStatisticsByUser Requested { get; set; }

        public QuestionnaireStatistics Statistics { get; set; } = new QuestionnaireStatistics();
        private readonly List<IComposite> allItems;

        public PdfQuestionnaireModel(QuestionnaireDocument questionnaire, PdfSettings settings)
        {
            this.questionnaire = questionnaire;
            this.Settings = settings;
            this.questionnaire.ConnectChildrenWithParent();
            this.allItems = this.questionnaire.Children.SelectMany<IComposite, IComposite>(x => x.TreeToEnumerable<IComposite>(g => g.Children)).ToList();

            ItemsWithLongConditions = CollectEntitiesWithLongConditions();
            ItemsWithLongValidations = CollectItemsWithLongValidations();
            QuestionsWithLongInstructions = Find<IQuestion>(x => x.Instructions?.Length > Settings.InstructionsExcerptLength).ToList();
            QuestionsWithLongOptionsList = Find<IQuestion>(x => x.Answers?.Count > Settings.OptionsExcerptCount).ToList();

            FillStatistics(this.allItems, this.Statistics);
            this.Statistics.SectionsCount = questionnaire.Children.Count;
            this.Statistics.GroupsCount -= Statistics.SectionsCount;
            this.Statistics.QuestionsWithConditionsCount = Find<IQuestion>(x => !string.IsNullOrWhiteSpace(x.ConditionExpression) || x.ValidationConditions.Any()).Count();
        }

        private List<EntityWithLongValidation> CollectItemsWithLongValidations()
        {
            var questions = this.Find<IQuestion>(x => x.ValidationConditions.Count > 0 && x.ValidationConditions.Any(condition => condition.Expression?.Length > this.Settings.ExpressionExcerptLength))
                .Select(x => new EntityWithLongValidation
                {
                    Id = x.PublicKey,
                    Title = x.QuestionText,
                    VariableName = x.StataExportCaption,
                    ValidationConditions = x.ValidationConditions.ToList()
                });
            var staticTexts = this.Find<IStaticText>(x => x.ValidationConditions?.Count > 0 && x.ValidationConditions.Any(condition => condition.Expression?.Length > this.Settings.ExpressionExcerptLength))
                .Select(x => new EntityWithLongValidation
                {
                    Id = x.PublicKey,
                    Title = x.Text,
                    ValidationConditions = x.ValidationConditions.ToList()
                });
            var entitiesWithLongValidations = questions.Union(staticTexts).ToList();

            int index = 1;
            entitiesWithLongValidations.ForEach(x => x.Index = index++);

            return entitiesWithLongValidations;
        }

        private List<EntityWithLongCondition> CollectEntitiesWithLongConditions()
        {
            var questions = this.Find<IQuestion>(x => x.ConditionExpression?.Length > this.Settings.ExpressionExcerptLength)
                .Select(x => new EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.QuestionText,
                    VariableName = x.StataExportCaption,
                    EnablementCondition = x.ConditionExpression.Trim()
                });
            var groupsAndRosters = this.Find<IGroup>(x => x.ConditionExpression?.Length > this.Settings.ExpressionExcerptLength)
                .Select(x => new EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.Title,
                    EnablementCondition = x.ConditionExpression.Trim()
                });
            var staticTexts = this.Find<IStaticText>(x => x.ConditionExpression?.Length > this.Settings.ExpressionExcerptLength)
                .Select(x => new EntityWithLongCondition
                {
                    Id = x.PublicKey,
                    Title = x.Text,
                    EnablementCondition = x.ConditionExpression.Trim()
                });

            var entitiesWithLongConditions = questions.Union(groupsAndRosters).Union(staticTexts).ToList();

            int index = 1;
            entitiesWithLongConditions.ForEach(x => x.Index = index++);

            return entitiesWithLongConditions;
        }

        public List<IQuestion> QuestionsWithLongOptionsList { get; private set; }

        public List<IQuestion> QuestionsWithLongInstructions { get; private set; }

        public List<EntityWithLongValidation> ItemsWithLongValidations { get; private set; }

        public List<EntityWithLongCondition> ItemsWithLongConditions { get; private set; }

        public string Title => this.questionnaire.Title;
        public IEnumerable<Guid> SectionIds => this.questionnaire.Children.Select(x => x.PublicKey).ToList();
        public IEnumerable<ModificationStatisticsByUser> SharedPersons { get; set; }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
           => this.Find(condition).FirstOrDefault();

        public IEnumerable<T> Find<T>() where T : class
           => this.allItems.Where(x => x is T).Cast<T>();

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
            => this.Find<T>().Where(condition);

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return this.allItems.FirstOrDefault(x => x is T && x.PublicKey == publicKey) as T;
        }

        public string GetGroupTitle(Guid groupId) => this.Find<Group>(groupId).Title;

        public IEnumerable<IComposite> GetChildren(Guid groupId)
        {
            return this.Find<Group>(groupId).Children;
        }

        public bool IsQuestion(IComposite item)
        {
            var question = item as IQuestion;
            return question != null;
        }

        public bool IsGroup(IComposite item)
        {
            var group = item as IGroup;
            return group != null;
        }

        public bool IsRoster(IComposite item)
        {
            return this.IsGroup(item) && (item as IGroup).IsRoster;
        }

        public bool IsStaticText(IComposite item)
        {
            var text = item as IStaticText;
            return text != null;
        }

        public string GetBreadcrumbsForGroup(Guid groupId)
        {
            var parents = this.GetAllParentGroupsStartingFromBottom(this.Find<Group>(groupId), this.questionnaire).ToList();
            parents.Reverse();
            return string.Join(" / ", parents.Select(x => x.Title));
        }

        private IEnumerable<IGroup> GetAllParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var startGroupId = group.PublicKey;
            while (group != document)
            {
                if (group.PublicKey != startGroupId)
                {
                    yield return group;
                }
                group = (IGroup)group.GetParent();
            }
        }

        public GroupStatistics GetGroupStatistics(Guid groupId)
        {
            var statistics = new GroupStatistics();
            var childItems = this.Find<Group>(groupId).TreeToEnumerable<IComposite>(g => g.Children).Where(x => x.PublicKey != groupId);
            return this.FillStatistics(childItems, statistics);
        }

        private GroupStatistics FillStatistics(IEnumerable<IComposite> items, GroupStatistics statistics)
        {
            foreach (var item in items)
            {
                if (this.IsQuestion(item))
                    statistics.QuestionsCount++;
                else if (this.IsRoster(item))
                    statistics.RostersCount++;
                else if (this.IsGroup(item))
                    statistics.GroupsCount++;
                else
                    statistics.StaticTextsCount++;
            }
            return statistics;
        }

        public bool QuestionHasInstructions(IQuestion question) => !string.IsNullOrWhiteSpace(question.Instructions);

        public bool QuestionHasEnablementCondition(IQuestion question) => !string.IsNullOrWhiteSpace(question.ConditionExpression);

        public bool GroupHasEnablementCondition(IGroup group) => !string.IsNullOrWhiteSpace(@group.ConditionExpression);

        public string GetRosterSourceQuestionVariable(Guid rosterId)
        {
            var roster = this.Find<Group>(rosterId);
            return roster.RosterSizeQuestionId != null
                ? this.Find<IQuestion>(roster.RosterSizeQuestionId.Value)?.StataExportCaption
                : string.Empty;
        }

        public string GetStringifiedTypeOfRosterSizeQuestion(Guid? rosterSizeQuestionId)
        {
            if (!rosterSizeQuestionId.HasValue)
                return string.Empty;

            var question = this.Find<IQuestion>(rosterSizeQuestionId.Value);
            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return "multi-select";
                case QuestionType.Numeric:
                    return "numeric";
                case QuestionType.TextList:
                    return "list";
                default:
                    return string.Empty;
            }
        }

        public string GetFormattedFixedRosterValue(IGroup roster, decimal value) =>
            this.FormatAsIntegerWithLeadingZeros(value, roster.FixedRosterTitles.Select(x => (double)x.Value));

        public string GetFormattedOptionValue(List<Answer> options, string optionValueAsString)
        {
            var values = options.Select(x => double.Parse(x.AnswerValue));
            var optionValue = decimal.Parse(optionValueAsString);
            return FormatAsIntegerWithLeadingZeros(optionValue, values);
        }

        public string GetFormattedQuestionType(IQuestion question)
        {
            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                {
                    var singleQuestion = (question as SingleQuestion);
                    var isLinked = (singleQuestion?.LinkedToQuestionId.HasValue ?? false) ||
                                    (singleQuestion?.LinkedToRosterId.HasValue ?? false);
                    var isCascading = singleQuestion?.CascadeFromQuestionId.HasValue ?? false;
                    var isCombobox = singleQuestion?.IsFilteredCombobox ?? false;
                    var questionOptions = (new[]
                    {
                        isLinked ? "linked" : "",
                        isCascading? "cascading" : "",
                        isCombobox? "Combo box" : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));

                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return "single-select" + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
                }
                case QuestionType.MultyOption:
                {
                    var multyOptionsQuestion = (question as MultyOptionsQuestion);
                    var areAnswersOrdered = multyOptionsQuestion?.AreAnswersOrdered ?? false;
                    var isYesNoView = multyOptionsQuestion?.YesNoView ?? false;
                    var isLinked = (multyOptionsQuestion?.LinkedToQuestionId.HasValue ?? false) ||
                                    (multyOptionsQuestion?.LinkedToRosterId.HasValue ?? false);

                    var questionOptions = (new[]
                    {
                        areAnswersOrdered ? "ordered" : "",
                        isYesNoView ? "yes/no" : "",
                        isLinked ? "linked" : ""
                    }).Where(x => !string.IsNullOrWhiteSpace(x));
                    var stringifiedQuestionOptions = string.Join(", ", questionOptions);
                    return "multi-select" + (string.IsNullOrWhiteSpace(stringifiedQuestionOptions) ? "" : ": " + stringifiedQuestionOptions);
                }
                case QuestionType.Numeric:
                    var isInteger = (question as NumericQuestion)?.IsInteger ?? false;
                    return "numeric: " + (isInteger ? "integer" : "decimal");
                case QuestionType.DateTime:
                    return "date: MM/DD/YYYY";
                case QuestionType.GpsCoordinates:
                    return "GPS";
                case QuestionType.Text:
                    return "text";
                case QuestionType.TextList:
                    return "list";
                case QuestionType.QRBarcode:
                    return "barcode";
                case QuestionType.Multimedia:
                    return "picture";
                default:
                    return string.Empty;
            }
        }

        private string FormatAsIntegerWithLeadingZeros(decimal value, IEnumerable<double> values)
        {
            var maxValue = values.Select(x => Math.Floor(Math.Log10(Math.Abs(x)) + 1)).Max();
            maxValue = Math.Max(Settings.MinAmountOfDigitsInCodes, maxValue);
            return Convert.ToInt64(value).ToString($"D{maxValue}");
        }

        public string GetQuestionInstructionExcerpt(IQuestion question) =>
            question.Instructions.Substring(0, Math.Min(this.Settings.InstructionsExcerptLength, question.Instructions.Length));

        public bool InstructionIsTooLong(IQuestion question) =>
            question.Instructions?.Length > this.Settings.InstructionsExcerptLength;

        public int GetEntityIndexInAppendix(Guid questionId, string appendix)
        {
            switch (appendix)
            {
                case "E":
                    return this.ItemsWithLongConditions.Single(x => x.Id == questionId).Index;
                case "V":
                    return this.ItemsWithLongValidations.Single(x => x.Id == questionId).Index;
                case "I":
                {
                    var question = Find<IQuestion>(questionId);
                    return  QuestionsWithLongInstructions.IndexOf(question) + 1;
                }
                case "O":
                {
                    var question = Find<IQuestion>(questionId);
                    return QuestionsWithLongOptionsList.IndexOf(question) + 1;
                }
            }
            return -1;
        }

        public bool ExpressionIsTooLong(string expression) => expression?.Length > this.Settings.ExpressionExcerptLength;

        public string GetExpressionExcerpt(string expression) => expression?.Substring(0, Math.Min(this.Settings.ExpressionExcerptLength, expression.Length)) ?? string.Empty;
        public string GetInstructionsId(Guid id) => $"instructions-{id.FormatGuid()}";

        public string GetConditionId(Guid id) => $"condition-{id.FormatGuid()}";

        public string GetQuestionId(Guid id) => $"question-{id.FormatGuid()}";

        public string GetGroupId(Guid id) => $"group-{id.FormatGuid()}";

        public string GetValidationsId(Guid id) => $"validations-{id.FormatGuid()}";

        public string GetOptionsId(Guid id) => $"options-{id.FormatGuid()}";

        public bool IsYesNoMultiQuestion(IQuestion question) => (question as MultyOptionsQuestion)?.YesNoView ?? false;

        public bool StaticTextHasEnablementCondition(IStaticText text) => !string.IsNullOrWhiteSpace(text.ConditionExpression);

        public int GetValidationsCount(IList<ValidationCondition> validationConditions) => validationConditions?.Count ?? 0;

        public Guid GetAttachmentId(IStaticText staticText)
        {
            return this.questionnaire.Attachments.First(x => x.Name == staticText.AttachmentName).AttachmentId;
        }

        public bool StaticTextHasAttachedImage(IStaticText staticText)
        {
            return !string.IsNullOrWhiteSpace(staticText.AttachmentName) &&
                   this.questionnaire.Attachments.Any(x => x.Name == staticText.AttachmentName);
        }
    }
}