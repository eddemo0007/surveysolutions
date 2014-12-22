﻿namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class PagedQuestionnaireCommunicationPackage : QuestionnaireListCommunicationPackage
    {
        public string Order { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
