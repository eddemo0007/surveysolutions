﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Main.Core.Entities.SubEntities;
    using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
    using WB.Core.SharedKernels.DataCollection.Portable;
    using System.Text.RegularExpressions;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class LevelTemplate : LevelTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("\r\n    internal partial class ");
            
            #line 11 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.ClassName));
            
            #line default
            #line hidden
            this.Write(" : LevelFunctions, ");
            
            #line 11 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IInterviewLevel).Name));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 11 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IIndexedInterviewLevel).Name));
            
            #line default
            #line hidden
            this.Write("\r\n    {\r\n        \r\n        public ");
            
            #line 14 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.ClassName));
            
            #line default
            #line hidden
            this.Write("(RosterVector rosterVector, ");
            
            #line 14 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Storage.ClassName));
            
            #line default
            #line hidden
            this.Write(" storage) \r\n        {\r\n            this._storage = storage;\r\n            this.Que" +
                    "st = new QuestionnaireRandom(storage.state.Properties);\r\n            this.Roster" +
                    "Vector = rosterVector;\r\n            this.Identity = new Identity(IdOf.");
            
            #line 19 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Model.Variable));
            
            #line default
            #line hidden
            this.Write(", this.RosterVector);\r\n\r\n");
            
            #line 21 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var method in Storage.GetEnablementConditions(Model.ClassName)) 
    {

            
            #line default
            #line hidden
            this.Write("            \r\n            enablementConditions.Add(IdOf.");
            
            #line 25 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 25 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.MethodName));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 26 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 29 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var methodGroup in Storage.GetValidationConditions(Model.ClassName)) 
    {

            
            #line default
            #line hidden
            this.Write("            validationConditions.Add(IdOf.");
            
            #line 33 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(methodGroup.Key));
            
            #line default
            #line hidden
            this.Write(", new Func<bool>[] { ");
            
            #line 33 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Join(",",  methodGroup.Value)));
            
            #line default
            #line hidden
            this.Write(" });\r\n");
            
            #line 34 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 37 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var method in Storage.GetVariableExpressions(Model.ClassName)) 
    {

            
            #line default
            #line hidden
            this.Write("            \r\n            variableExpressions.Add(IdOf.");
            
            #line 41 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 41 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.MethodName));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 42 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 45 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var method in Storage.GetCategoricalOptionsFilters(Model.ClassName)) 
    {

            
            #line default
            #line hidden
            this.Write("            \r\n            categoricalFilters.Add(IdOf.");
            
            #line 49 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 49 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.MethodName));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 50 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 53 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var method in Storage.GetLinkedFilters(Model.ClassName)) 
    {

            
            #line default
            #line hidden
            this.Write("            \r\n            linkedFilters.Add(IdOf.");
            
            #line 57 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 57 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(method.MethodName));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 58 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            this.Write(@"        }

        public Func<IInterviewLevel, bool> GetLinkedQuestionFilter(Identity identity)
        {
            if (linkedFilters.TryGetValue(identity.Id, out var result))
            {
                return result;
            }

            return null;
        }

        public Func<bool> GetConditionExpression(Identity identity)
        {
            if (enablementConditions.TryGetValue(identity.Id, out var result))
            {
                return result;
            }

            return null;
        }

        public Func<bool>[] GetValidationExpressions(Identity identity)
        {
            if (validationConditions.TryGetValue(identity.Id, out var result))
            {
                return result;
            }

            return null;
        }

        public Func<object> GetVariableExpression(Identity identity)
        {
            if (variableExpressions.TryGetValue(identity.Id, out var result))
            {
                return result;
            }

            return null;
        }

        public Func<int, bool> GetCategoricalFilter(Identity identity)
        {
            if (categoricalFilters.TryGetValue(identity.Id, out var result))
            {
                return result;
            }

            return null;
        }

        private readonly Dictionary<Guid, Func<");
            
            #line 113 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IInterviewLevel).Name));
            
            #line default
            #line hidden
            this.Write(", bool>> linkedFilters = new Dictionary<Guid, Func<");
            
            #line 113 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IInterviewLevel).Name));
            
            #line default
            #line hidden
            this.Write(@", bool>>();
        private readonly Dictionary<Guid, Func<int, bool>> categoricalFilters = new Dictionary<Guid, Func<int, bool>>();
        private readonly Dictionary<Guid, Func<bool>> enablementConditions = new Dictionary<Guid, Func<bool>>();
        private readonly Dictionary<Guid, Func<bool>[]> validationConditions = new Dictionary<Guid, Func<bool>[]>();
        private readonly Dictionary<Guid, Func<object>> variableExpressions = new Dictionary<Guid, Func<object>>();

        private readonly ");
            
            #line 119 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Storage.ClassName));
            
            #line default
            #line hidden
            this.Write(" _storage;\r\n\r\n        private ");
            
            #line 121 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IInterviewStateForExpressions).Name));
            
            #line default
            #line hidden
            this.Write(" _state => _storage.state;\r\n\r\n        private ");
            
            #line 123 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(typeof(IInterviewPropertiesForExpressions).Name));
            
            #line default
            #line hidden
            this.Write(@" properties => _state.Properties;

        public RosterVector RosterVector { get; private set; }

        public Identity Identity  { get; private set; }

        public int @rowcode => this.RosterVector[this.RosterVector.Length - 1];

        public int @rowindex => _state.GetRosterIndex(Identity);

        public string @rowname => _state.GetRosterTitle(Identity);
        
        public int RowCode => @rowcode;

        public int RowIndex => @rowindex;

        // backward compatibility
        private QuestionnaireRandom Quest;		
");
            
            #line 141 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var question in Model.Questions) 
    {

            
            #line default
            #line hidden
            this.Write("        public ");
            
            #line 144 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(question.TypeName));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 144 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(question.Variable));
            
            #line default
            #line hidden
            this.Write(" => _state.GetAnswer<");
            
            #line 144 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(question.TypeName));
            
            #line default
            #line hidden
            this.Write(">(IdOf.");
            
            #line 144 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(question.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 144 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(question.RosterScope.Length > 0 ? "RosterVector.Take(" + question.RosterScope.Length + ")" : "RosterVector.Empty"));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 146 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 149 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var variable in Model.Variables) 
    {

            
            #line default
            #line hidden
            this.Write("        public ");
            
            #line 152 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(variable.TypeName));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 152 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(variable.Variable));
            
            #line default
            #line hidden
            this.Write(" => _state.GetVariable<");
            
            #line 152 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(variable.TypeName));
            
            #line default
            #line hidden
            this.Write(">(IdOf.");
            
            #line 152 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(variable.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 152 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(variable.RosterScope.Length > 0 ? "RosterVector.Take(" + variable.RosterScope.Length + ")" : "RosterVector.Empty"));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 154 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 157 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var roster in Model.Rosters) 
    {

            
            #line default
            #line hidden
            this.Write("        public RostersCollection<");
            
            #line 160 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(roster.ClassName));
            
            #line default
            #line hidden
            this.Write("> ");
            
            #line 160 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(roster.Variable));
            
            #line default
            #line hidden
            this.Write(" => _storage.GetLevels<");
            
            #line 160 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(roster.ClassName));
            
            #line default
            #line hidden
            this.Write(">(IdOf.");
            
            #line 160 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(roster.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 160 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(roster.RosterScope.Length == 1? "null" : "Identity"));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 161 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 164 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var table in Storage.LookupTables) 
    {

            
            #line default
            #line hidden
            this.Write("        public static Dictionary<int, ");
            
            #line 167 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(table.TypeName));
            
            #line default
            #line hidden
            this.Write("> ");
            
            #line 167 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(table.TableName));
            
            #line default
            #line hidden
            this.Write(" => LookupTables.");
            
            #line 167 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(table.TableName));
            
            #line default
            #line hidden
            this.Write(";\r\n");
            
            #line 168 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            
            #line 171 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"

    foreach (var section in Model.Sections) 
    {

            
            #line default
            #line hidden
            this.Write("        public Section ");
            
            #line 174 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(section.Variable));
            
            #line default
            #line hidden
            this.Write(" => _state.GetSection(IdOf.");
            
            #line 174 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(section.Variable));
            
            #line default
            #line hidden
            this.Write(", ");
            
            #line 174 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(section.RosterScope.Length > 0 ? "RosterVector.Take(" + section.RosterScope.Length + ")" : "RosterVector.Empty"));
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 176 "C:\src\wb\src\Core\BoundedContexts\Designer\WB.Core.BoundedContexts.Designer\CodeGenerationV2\CodeTemplates\LevelTemplate.tt"
 
    }

            
            #line default
            #line hidden
            this.Write("\r\n    }\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class LevelTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
