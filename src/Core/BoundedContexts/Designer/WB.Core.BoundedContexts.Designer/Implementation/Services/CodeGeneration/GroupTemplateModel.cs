using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface ITemplateModel {
        Guid Id { set; get; }
        string VariableName { set; get; }
        string Conditions { set; get; }
    }

    public class GroupTemplateModel : ITemplateModel
    {
        public Guid Id { set; get; }
        public string VariableName { set; get; }

        public string Conditions { set; get; }
        public string GeneratedStateName { set; get; }
        public string GeneratedIdName { set; get; }
        public string GeneratedConditionsMethodName { set; get; }

        public string RosterScopeName { set; get; }
    }
}