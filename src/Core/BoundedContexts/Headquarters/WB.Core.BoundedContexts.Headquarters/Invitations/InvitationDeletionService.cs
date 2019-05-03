﻿using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    internal class InvitationsDeletionService : IInvitationsDeletionService
    {
        private readonly IUnitOfWork sessionFactory;

        public InvitationsDeletionService(IUnitOfWork sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Delete(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM plainstore.invitations as i " +
                            $"USING plainstore.assignments as a " +
                            $"WHERE i.assignmentid = a.id " +
                            $"  AND a.questionnaireid = '{questionnaireIdentity.QuestionnaireId}' " +
                            $"  AND a.questionnaireversion = {questionnaireIdentity.Version}";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.ExecuteUpdate();
        }
    }

    public interface IInvitationsDeletionService
    {
        void Delete(QuestionnaireIdentity questionnaireIdentity);
    }
}