using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewFactory : IQuestionnaireListViewFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewFolder> publicFoldersStorage;

        public QuestionnaireListViewFactory(IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage,
            IPlainStorageAccessor<QuestionnaireListViewFolder> publicFoldersStorage)
        {
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.publicFoldersStorage = publicFoldersStorage;
        }

        public IReadOnlyCollection<QuestionnaireListViewItem> GetUserQuestionnaires(
            Guid userId, bool isAdmin, int pageIndex = 1, int pageSize = 128)
        {
            return questionnaireListViewItemStorage.Query(queryable
                => FilterByQuestionnaires(queryable, userId, isAdmin)
                    .OrderBy(x => x.Title)
                    .Skip((pageIndex - 1)*pageSize)
                    .Take(pageSize)
                    .ToReadOnlyCollection());
        }

        private static IQueryable<QuestionnaireListViewItem> FilterByQuestionnaires(
            IQueryable<QuestionnaireListViewItem> queryable, Guid userId, bool isAdmin)
        {
            var notDeletedQuestionnaires = queryable.Where(x => x.IsDeleted == false);

            return isAdmin
                ? notDeletedQuestionnaires
                : notDeletedQuestionnaires.Where(questionnaire =>
                    questionnaire.CreatedBy == userId ||
                    questionnaire.SharedPersons.Any(person => person.UserId == userId) ||
                    questionnaire.IsPublic);
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var count = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input).Count());

            var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

            var records = questionnaireListViewItemStorage.Query(_ =>
                FilterQuestionnaires(_, input).Select(x => new QuestionnaireListViewItem()
                    {
                        CreatedBy = x.CreatedBy,
                        CreationDate = x.CreationDate,
                        CreatorName = x.CreatorName,
                        IsDeleted = x.IsDeleted,
                        IsPublic = x.IsPublic,
                        LastEntryDate = x.LastEntryDate,
                        Owner = x.Owner,
                        PublicId = x.PublicId,
                        QuestionnaireId = x.QuestionnaireId,
                        Title = x.Title,
                    })
                    .OrderUsingSortExpression(sortOrder)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList());

            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: records,
                order: input.Order);
        }

        public QuestionnaireListView LoadFoldersAndQuestionnaires(QuestionnaireListInputModel input)
        {
            List<QuestionnaireListViewFolder> folders = Enumerable.Empty<QuestionnaireListViewFolder>().ToList();
            List<QuestionnaireListViewItem> questionnaires = Enumerable.Empty<QuestionnaireListViewItem>().ToList();
            int foldersCount = 0, questionnairesCount = 0;

            if (input.IsPublic)
            {
                foldersCount = publicFoldersStorage.Query(_ => FilterFolders(_, input).Count());
                folders = publicFoldersStorage.Query(_ => FilterFolders(_, input)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList());
            }

            questionnairesCount = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input).Count());
            var count = foldersCount + questionnairesCount;

            if (folders.Count < input.PageSize)
            {
                var questionnairesSkipCount = (input.Page - 1) * input.PageSize - foldersCount;
                questionnairesSkipCount = questionnairesSkipCount < 0 ? 0 : questionnairesSkipCount;
                var questionnairesTakeCount = input.PageSize - folders.Count;

                var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

                questionnaires = questionnaireListViewItemStorage.Query(_ =>
                    FilterQuestionnaires(_, input).Select(x => new QuestionnaireListViewItem()
                        {
                            CreatedBy = x.CreatedBy,
                            CreationDate = x.CreationDate,
                            CreatorName = x.CreatorName,
                            IsDeleted = x.IsDeleted,
                            IsPublic = x.IsPublic,
                            LastEntryDate = x.LastEntryDate,
                            Owner = x.Owner,
                            PublicId = x.PublicId,
                            QuestionnaireId = x.QuestionnaireId,
                            Title = x.Title,
                        })
                        .OrderUsingSortExpression(sortOrder)
                        .Skip(questionnairesSkipCount)
                        .Take(questionnairesTakeCount)
                        .ToList());
            }

            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: folders.Concat(questionnaires.Cast<IQuestionnaireListItem>()).ToList(), 
                order: input.Order);
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItem> _,
            QuestionnaireListInputModel input)
        {
            var result = _.Where(x => x.IsDeleted == false && x.FolderId == input.FolderId);
            if (!string.IsNullOrEmpty(input.SearchFor))
            {
                var filterLowerCase = input.SearchFor.Trim().ToLower();
                result =
                    result.Where(
                        x => x.Title.ToLower().Contains(filterLowerCase) || x.CreatorName.ToLower().Contains(filterLowerCase));
            }

            if (input.IsAdminMode)
            {
                if (!input.IsPublic)
                {
                    result =
                        result.Where(
                            x =>
                                x.CreatedBy == input.ViewerId ||
                                x.SharedPersons.Any(person => person.UserId == input.ViewerId));
                }
            }
            else
            {
                if (input.IsPublic)
                    result = result.Where(x => x.IsPublic);
                else
                    result =
                        result.Where(
                            x =>
                                x.CreatedBy == input.ViewerId ||
                                x.SharedPersons.Any(person => person.UserId == input.ViewerId));
            }
            return result;
        }

        private IQueryable<QuestionnaireListViewFolder> FilterFolders(IQueryable<QuestionnaireListViewFolder> _,
            QuestionnaireListInputModel input)
        {
            var result = _.Where(x => input.IsPublic == true); // support only public folders

            if (!string.IsNullOrEmpty(input.SearchFor))
            {
                var filterLowerCase = input.SearchFor.Trim().ToLower();
                result = result.Where(x => x.Title.ToLower().Contains(filterLowerCase));
            }
            else 
            {
                result = result.Where(x => input.FolderId == x.Parent);
            }

            return result;
        }
    }
}