﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        private readonly IQuestionnaire questionnaire;

        public InterviewTree(Guid interviewId, IQuestionnaire questionnaire, IEnumerable<InterviewTreeSection> sections)
        {
            this.InterviewId = interviewId.FormatGuid();
            this.questionnaire = questionnaire;

            this.Sections = sections.ToList();

            foreach (var section in this.Sections)
            {
                ((IInternalInterviewTreeNode)section).SetTree(this);
            }
        }

        public string InterviewId { get; }
        public IReadOnlyCollection<InterviewTreeSection> Sections { get; private set; }
        
        public InterviewTreeQuestion GetQuestion(Identity questionIdentity)
            => this
                .GetNodes<InterviewTreeQuestion>()
                .SingleOrDefault(node => node.Identity == questionIdentity);


        internal InterviewTreeGroup GetGroup(Identity identity) 
            => this
            .GetNodes<InterviewTreeGroup>()
            .SingleOrDefault(node => node.Identity == identity);

        internal InterviewTreeStaticText GetStaticText(Identity identity) 
            => this
            .GetNodes<InterviewTreeStaticText>()
            .Single(node => node.Identity == identity);

        public InterviewTreeVariable GetVariable(Identity identity)
            => this
            .GetNodes<InterviewTreeVariable>()
            .Single(node => node.Identity == identity);

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this
                .GetNodes<InterviewTreeQuestion>()
                .Where(node => node.Identity.Id == questionId)
                .ToReadOnlyCollection();

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions()
            => this
                .GetNodes<InterviewTreeQuestion>()
                .ToReadOnlyCollection();

        public IReadOnlyCollection<InterviewTreeRoster> FindRosters()
            => this
                .GetNodes<InterviewTreeRoster>()
                .ToReadOnlyCollection();

        public IEnumerable<IInterviewTreeNode> FindEntity(Guid nodeId)
        {
            return this.GetNodes().Where(x => x.Identity.Id == nodeId);
        }

        private IEnumerable<TNode> GetNodes<TNode>() => this.GetNodes().OfType<TNode>();

        private IEnumerable<IInterviewTreeNode> GetNodes()
            => this.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(node => node.Children);
     
        public void ActualizeTree()
        {
            var itemsQueue = new Queue<IInterviewTreeNode>(Sections);

            while (itemsQueue.Count > 0)
            {
                var currentItem = itemsQueue.Dequeue();
                if (!(currentItem is InterviewTreeGroup))
                    continue;
                var currentGroup = currentItem as InterviewTreeGroup;
                currentGroup.ActualizeChildren();

                foreach (var childItem in currentGroup.Children)
                {
                    itemsQueue.Enqueue(childItem);
                }
            }
        }

        public void RemoveNode(Identity identity)
        {
            foreach (var node in this.GetNodes().Where(x => x.Identity.Equals(identity)))
                ((InterviewTreeGroup)node.Parent)?.RemoveChild(node.Identity);
        }

        public IReadOnlyCollection<InterviewTreeNodeDiff> Compare(InterviewTree changedTree)
        {
            var sourceNodes = this.GetNodes().ToList();
            var changedNodes = changedTree.GetNodes().ToList();

            var leftOuterJoin = from source in sourceNodes
                                join changed in changedNodes
                                    on source.Identity equals changed.Identity
                                    into temp
                                from changed in temp.DefaultIfEmpty()
                                select InterviewTreeNodeDiff.Create(source, changed);

            var rightOuterJoin = from changed in changedNodes
                                 join source in sourceNodes
                                     on changed.Identity equals source.Identity
                                     into temp
                                 from source in temp.DefaultIfEmpty()
                                 select InterviewTreeNodeDiff.Create(source, changed);

            var fullOuterJoin = leftOuterJoin.Concat(rightOuterJoin);

            return fullOuterJoin
                .DistinctBy(x => new {sourceIdentity = x.SourceNode?.Identity, changedIdentity = x.ChangedNode?.Identity})
                .Where(diff =>
                    diff.IsNodeAdded ||
                    diff.IsNodeRemoved ||
                    diff.IsNodeDisabled ||
                    diff.IsNodeEnabled ||
                    IsRosterTitleChanged(diff as InterviewTreeRosterDiff) ||
                    IsAnswerByQuestionChanged(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionValid(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionInalid(diff as InterviewTreeQuestionDiff) ||
                    IsStaticTextValid(diff as InterviewTreeStaticTextDiff) ||
                    IsStaticTextInalid(diff as InterviewTreeStaticTextDiff) ||
                    IsVariableChanged(diff as InterviewTreeVariableDiff) ||
                    IsOptionsSetChanged(diff as InterviewTreeQuestionDiff))
                .ToReadOnlyCollection();
        }

        private bool IsOptionsSetChanged(InterviewTreeQuestionDiff diffByQuestion)
        {
            if (diffByQuestion == null || diffByQuestion.IsNodeRemoved) return false;

            return diffByQuestion.IsOptionsChanged;
        }

        private static bool IsVariableChanged(InterviewTreeVariableDiff diffByVariable)
            => diffByVariable != null && diffByVariable.IsValueChanged;

        private static bool IsQuestionValid(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsValid;

        private static bool IsQuestionInalid(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsInvalid;

        private static bool IsStaticTextValid(InterviewTreeStaticTextDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsValid;

        private static bool IsStaticTextInalid(InterviewTreeStaticTextDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsInvalid;

        private static bool IsAnswerByQuestionChanged(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsAnswerChanged;

        private static bool IsRosterTitleChanged(InterviewTreeRosterDiff diffByRoster)
            => diffByRoster != null && diffByRoster.IsRosterTitleChanged;

        public override string ToString()
            => $"Tree ({this.InterviewId})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Sections.Select(section => section.ToString().PrefixEachLine("  ")));

        public InterviewTree Clone()
        {
            var clonedInterviewTree = (InterviewTree)this.MemberwiseClone();
            clonedInterviewTree.Sections = this.Sections.Select(s =>
            {
                var interviewTreeSection = (InterviewTreeSection)s.Clone();
                ((IInternalInterviewTreeNode) interviewTreeSection).SetTree(clonedInterviewTree);
                return interviewTreeSection;
            }).ToReadOnlyCollection();

            return clonedInterviewTree;
        }

        public IInterviewTreeNode CreateNode(QuestionnaireReferenceType type, Identity identity)
        {
            switch (type)
            {
                case QuestionnaireReferenceType.SubSection: return CreateSubSection(identity);
                case QuestionnaireReferenceType.StaticText: return CreateStaticText(identity);
                case QuestionnaireReferenceType.Variable: return CreateVariable(identity);
                case QuestionnaireReferenceType.Question: return CreateQuestion(identity);
                case QuestionnaireReferenceType.Roster: throw new ArgumentException("Use roster manager to create rosters");
            }
            return null;
        }

        public InterviewTreeQuestion CreateQuestion(Identity questionIdentity)
        {
            return CreateQuestion(this.questionnaire, questionIdentity);
        }

        public static InterviewTreeQuestion CreateQuestion(IQuestionnaire questionnaire, Identity questionIdentity)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionIdentity.Id);
            string title = questionnaire.GetQuestionTitle(questionIdentity.Id);
            string variableName = questionnaire.GetQuestionVariableName(questionIdentity.Id);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionIdentity.Id);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionIdentity.Id);
            bool isLinkedQuestion = questionnaire.IsQuestionLinked(questionIdentity.Id) || questionnaire.IsQuestionLinkedToRoster(questionIdentity.Id);
            var linkedSourceEntityId = isLinkedQuestion ? (questionnaire.IsQuestionLinked(questionIdentity.Id) ? questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id) : questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id)) : (Guid?) null;

            Guid? commonParentRosterIdForLinkedQuestion = isLinkedQuestion ? questionnaire.GetCommontParentForLinkedQuestionAndItSource(questionIdentity.Id) : null;
            Identity commonParentIdentity = null;
            if (isLinkedQuestion && commonParentRosterIdForLinkedQuestion.HasValue)
            {
                var level = questionnaire.GetRosterLevelForEntity(commonParentRosterIdForLinkedQuestion.Value);
                var commonParentRosterVector = questionIdentity.RosterVector.Take(level).ToArray();
                commonParentIdentity = new Identity(commonParentRosterIdForLinkedQuestion.Value, commonParentRosterVector);
            }

            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionIdentity.Id);
            return new InterviewTreeQuestion(questionIdentity, questionType: questionType, isDisabled: false, title: title, variableName: variableName, answer: null, linkedOptions: null, cascadingParentQuestionId: cascadingParentQuestionId, isYesNo: isYesNoQuestion, isDecimal: isDecimalQuestion, linkedSourceId: linkedSourceEntityId, commonParentRosterIdForLinkedQuestion: commonParentIdentity);
        }

        public static InterviewTreeVariable CreateVariable(Identity variableIdentity)
        {
            return new InterviewTreeVariable(variableIdentity);
        }

        public InterviewTreeSubSection CreateSubSection(Identity subSectionIdentity)
        {
            return CreateSubSection(this.questionnaire, subSectionIdentity);
        }

        public static InterviewTreeSubSection CreateSubSection(IQuestionnaire questionnaire, Identity subSectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(subSectionIdentity.Id);

            return new InterviewTreeSubSection(subSectionIdentity, childrenReferences);
        }

        public static InterviewTreeSection CreateSection(IQuestionnaire questionnaire, Identity sectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(sectionIdentity.Id);
            return new InterviewTreeSection(sectionIdentity, Enumerable.Empty<IInterviewTreeNode>(), childrenReferences);
        }

        public static InterviewTreeStaticText CreateStaticText(Identity staticTextIdentity)
        {
            return new InterviewTreeStaticText(staticTextIdentity);
        }

        public InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index)
        {
            return GetRosterManager(rosterIdentity.Id).CreateRoster(parentIdentity, rosterIdentity, index);
        }

        public RosterManager GetRosterManager(Guid rosterId)
        {
            if (questionnaire.IsFixedRoster(rosterId))
            {
                return new FixedRosterManager(this, this.questionnaire, rosterId);
            }

            Guid sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            var questionaType = questionnaire.GetQuestionType(sourceQuestionId);
            if (questionaType == QuestionType.MultyOption)
            {
                if (this.questionnaire.IsQuestionYesNo(sourceQuestionId))
                {
                    return new YesNoRosterManager(this, this.questionnaire, rosterId);
                }
                return new MultiRosterManager(this, this.questionnaire, rosterId);
            }

            if (questionaType == QuestionType.Numeric)
            {
                return new NumericRosterManager(this, this.questionnaire, rosterId);
            }

            if (questionaType == QuestionType.TextList)
            {
                return new ListRosterManager(this, this.questionnaire, rosterId);
            }

            throw new ArgumentException("Unknown roster type");
        }
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }
        IInterviewTreeNode Parent { get; }
        IReadOnlyCollection<IInterviewTreeNode> Children { get; }

        bool IsDisabled();

        void Disable();
        void Enable();

        IInterviewTreeNode Clone();
    }

    public interface IInternalInterviewTreeNode
    {
        void SetTree(InterviewTree tree);
        void SetParent(IInterviewTreeNode parent);
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;

        protected InterviewTreeLeafNode(Identity identity, bool isDisabled)
        {
            this.Identity = identity;
            this.isDisabled = isDisabled;
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        IReadOnlyCollection<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree) => this.Tree = tree;
        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent) => this.Parent = parent;

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);

        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public virtual IInterviewTreeNode Clone()
        {
            return (IInterviewTreeNode) this.MemberwiseClone();
        }
    }

    public enum QuestionnaireReferenceType
    {
        SubSection = 1,
        Roster = 2,
        StaticText = 10,
        Variable = 20,
        Question = 30,
    }

    public class QuestionnaireItemReference
    {
        public QuestionnaireItemReference(QuestionnaireReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
        }

        public Guid Id { get; set; }

        public QuestionnaireReferenceType Type { get; set; }
    }

    public class RosterNodeDescriptor
    {
        public Identity Identity { get; set; }
        public string Title { get; set; }

        public RosterType Type { get; set; }

        public InterviewTreeQuestion SizeQuestion { get; set; }
        public Identity RosterTitleQuestionIdentity { get; set; }
    }
}