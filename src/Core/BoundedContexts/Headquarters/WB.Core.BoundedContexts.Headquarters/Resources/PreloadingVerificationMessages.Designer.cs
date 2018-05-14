﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WB.Core.BoundedContexts.Headquarters.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class PreloadingVerificationMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PreloadingVerificationMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WB.Core.BoundedContexts.Headquarters.Resources.PreloadingVerificationMessages", typeof(PreloadingVerificationMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password-protected archives are not supported.
        /// </summary>
        public static string ArchiveWithPasswordNotSupported {
            get {
                return ResourceManager.GetString("ArchiveWithPasswordNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please, wait until previous import of assignments is completed.
        /// </summary>
        public static string HasAssignmentsToImport {
            get {
                return ResourceManager.GetString("HasAssignmentsToImport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PL0002] N/A.
        /// </summary>
        public static string PL0002_MoreThenOneLevel {
            get {
                return ResourceManager.GetString("PL0002_MoreThenOneLevel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Column cannot be mapped to any question in the questionnaire..
        /// </summary>
        public static string PL0003_ColumnWasntMappedOnQuestion {
            get {
                return ResourceManager.GetString("PL0003_ColumnWasntMappedOnQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File cannot be mapped to any roster in the questionnaire..
        /// </summary>
        public static string PL0004_FileWasntMappedRoster {
            get {
                return ResourceManager.GetString("PL0004_FileWasntMappedRoster", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PL0005] N/A.
        /// </summary>
        public static string PL0005_QuestionDataTypeMismatch {
            get {
                return ResourceManager.GetString("PL0005_QuestionDataTypeMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate Id values found..
        /// </summary>
        public static string PL0006_IdDublication {
            get {
                return ResourceManager.GetString("PL0006_IdDublication", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One or more parent Id column is missing..
        /// </summary>
        public static string PL0007_ServiceColumnIsAbsent {
            get {
                return ResourceManager.GetString("PL0007_ServiceColumnIsAbsent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roster record does not have a parent..
        /// </summary>
        public static string PL0008_OrphanRosterRecord {
            get {
                return ResourceManager.GetString("PL0008_OrphanRosterRecord", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roster Id is inconsistent with roster size..
        /// </summary>
        public static string PL0009_RosterIdIsInconsistantWithRosterSizeQuestion {
            get {
                return ResourceManager.GetString("PL0009_RosterIdIsInconsistantWithRosterSizeQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Linked questions cannot be preloaded..
        /// </summary>
        public static string PL0010_UnsupportedLinkedQuestion {
            get {
                return ResourceManager.GetString("PL0010_UnsupportedLinkedQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to General error occurred..
        /// </summary>
        public static string PL0011_GeneralError {
            get {
                return ResourceManager.GetString("PL0011_GeneralError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Question found in the uploaded file(s) that does not exist in the questionnaire.
        /// </summary>
        public static string PL0012_QuestionWasNotFound {
            get {
                return ResourceManager.GetString("PL0012_QuestionWasNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided value is null or empty.
        /// </summary>
        public static string PL0013_ValueIsNullOrEmpty {
            get {
                return ResourceManager.GetString("PL0013_ValueIsNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Provided question value not allowed..
        /// </summary>
        public static string PL0014_ParsedValueIsNotAllowed {
            get {
                return ResourceManager.GetString("PL0014_ParsedValueIsNotAllowed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type of question is not correct..
        /// </summary>
        public static string PL0015_QuestionTypeIsIncorrect {
            get {
                return ResourceManager.GetString("PL0015_QuestionTypeIsIncorrect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Date or Time value entered in the date question..
        /// </summary>
        public static string PL0016_ExpectedDateTimeNotParsed {
            get {
                return ResourceManager.GetString("PL0016_ExpectedDateTimeNotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid GPS value..
        /// </summary>
        public static string PL0017_ExpectedGpsNotParsed {
            get {
                return ResourceManager.GetString("PL0017_ExpectedGpsNotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only integer values are allowed..
        /// </summary>
        public static string PL0018_ExpectedIntNotParsed {
            get {
                return ResourceManager.GetString("PL0018_ExpectedIntNotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only real values are allowed..
        /// </summary>
        public static string PL0019_ExpectedDecimalNotParsed {
            get {
                return ResourceManager.GetString("PL0019_ExpectedDecimalNotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Roster size question cannot have negative values..
        /// </summary>
        public static string PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative {
            get {
                return ResourceManager.GetString("PL0022_AnswerIsIncorrectBecauseIsRosterSizeAndNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Picture question cannot be preloaded..
        /// </summary>
        public static string PL0023_UnsupportedMultimediaQuestion {
            get {
                return ResourceManager.GetString("PL0023_UnsupportedMultimediaQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Preloading data were not found..
        /// </summary>
        public static string PL0024_DataWasNotFound {
            get {
                return ResourceManager.GetString("PL0024_DataWasNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Responsible field was not provided for 1 or more observations.
        /// </summary>
        public static string PL0025_ResponsibleNameIsEmpty {
            get {
                return ResourceManager.GetString("PL0025_ResponsibleNameIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Responsible does not exist for 1 or more observations.
        /// </summary>
        public static string PL0026_ResponsibleWasNotFound {
            get {
                return ResourceManager.GetString("PL0026_ResponsibleWasNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Responsible is a locked user for 1 or more observations.
        /// </summary>
        public static string PL0027_ResponsibleIsLocked {
            get {
                return ResourceManager.GetString("PL0027_ResponsibleIsLocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to User is not Supervisor or Interviewer for 1 or more observations.
        /// </summary>
        public static string PL0028_UserIsNotSupervisorOrInterviewer {
            get {
                return ResourceManager.GetString("PL0028_UserIsNotSupervisorOrInterviewer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Answer to roster size question cannot be greater than {0}..
        /// </summary>
        public static string PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40 {
            get {
                return ResourceManager.GetString("PL0029_AnswerIsIncorrectBecauseIsRosterSizeAndMoreThan40", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GPS questions require separate columns for Latitude and Longitude.
        /// </summary>
        public static string PL0030_GpsFieldsRequired {
            get {
                return ResourceManager.GetString("PL0030_GpsFieldsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GPS question requires Latitude and Longitude to be set..
        /// </summary>
        public static string PL0030_GpsMandatoryFilds {
            get {
                return ResourceManager.GetString("PL0030_GpsMandatoryFilds", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Two or more columns have the same name..
        /// </summary>
        public static string PL0031_ColumnNameDuplicatesFound {
            get {
                return ResourceManager.GetString("PL0031_ColumnNameDuplicatesFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Latitude must be greater than -90° and less than 90°..
        /// </summary>
        public static string PL0032_LatitudeMustBeGeaterThenN90AndLessThen90 {
            get {
                return ResourceManager.GetString("PL0032_LatitudeMustBeGeaterThenN90AndLessThen90", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Longitude must be greater than -180° and less than 180°..
        /// </summary>
        public static string PL0033_LongitudeMustBeGeaterThenN180AndLessThen180 {
            get {
                return ResourceManager.GetString("PL0033_LongitudeMustBeGeaterThenN180AndLessThen180", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid decimal separator. The &quot;,&quot; symbol is not allowed in numeric answers. Please use &quot;.&quot; instead..
        /// </summary>
        public static string PL0034_CommaSymbolIsNotAllowedInNumericAnswer {
            get {
                return ResourceManager.GetString("PL0034_CommaSymbolIsNotAllowedInNumericAnswer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Only integer values are allowed..
        /// </summary>
        public static string PL0035_QuantityNotParsed {
            get {
                return ResourceManager.GetString("PL0035_QuantityNotParsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Quantity should be greater than or equal to -1..
        /// </summary>
        public static string PL0036_QuantityShouldBeGreaterThanMinus1 {
            get {
                return ResourceManager.GetString("PL0036_QuantityShouldBeGreaterThanMinus1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Column cannot be mapped to non identifying question.
        /// </summary>
        public static string PL0037_ColumnIsNotIdentifying {
            get {
                return ResourceManager.GetString("PL0037_ColumnIsNotIdentifying", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Area question cannot be preloaded..
        /// </summary>
        public static string PL0038_UnsupportedAreaQuestion {
            get {
                return ResourceManager.GetString("PL0038_UnsupportedAreaQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Audio question cannot be preloaded..
        /// </summary>
        public static string PL0039_UnsupportedAudioQuestion {
            get {
                return ResourceManager.GetString("PL0039_UnsupportedAudioQuestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File with questionnaire level data is missing..
        /// </summary>
        public static string PL0040_QuestionnaireDataIsNotFound {
            get {
                return ResourceManager.GetString("PL0040_QuestionnaireDataIsNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Answered options count to multiple choice question can&apos;t be more than max allowed answers count.
        /// </summary>
        public static string PL0041_AnswerExceedsMaxAnswersCount {
            get {
                return ResourceManager.GetString("PL0041_AnswerExceedsMaxAnswersCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Id column doesn&apos;t have any value..
        /// </summary>
        public static string PL0042_IdIsEmpty {
            get {
                return ResourceManager.GetString("PL0042_IdIsEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text list question {0} has non-unique items: {1}. Interview id: {2}.
        /// </summary>
        public static string PL0043_TextListAnswerHasDuplicatesInTexts {
            get {
                return ResourceManager.GetString("PL0043_TextListAnswerHasDuplicatesInTexts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text list question {0} has non-unique item codes: {1}. Interview id: {2}.
        /// </summary>
        public static string PL0044_TextListAnswerHasDuplicatesInCodes {
            get {
                return ResourceManager.GetString("PL0044_TextListAnswerHasDuplicatesInCodes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Inconsistency detected between the number of records in the data file {0}, which has {1} records, and the trigger question {2}, which has {3} rows. Interview id: {4}.
        /// </summary>
        public static string PL0045_TextListAnswerHasDifferentAmountOfRecordsThanRosterFile {
            get {
                return ResourceManager.GetString("PL0045_TextListAnswerHasDifferentAmountOfRecordsThanRosterFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Inconsistency detected between the items lists in the data file {0} and question {1}. Items present in the text list and absent in the file: {2}. Rows from data file absent in text list question: {3}. Interview id: {4}.
        /// </summary>
        public static string PL0046_HasDifferentTextsInListAndRosterTitles {
            get {
                return ResourceManager.GetString("PL0046_HasDifferentTextsInListAndRosterTitles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Required column {0} is missing.
        /// </summary>
        public static string PL0047_ProtectedVariables_MissingColumn {
            get {
                return ResourceManager.GetString("PL0047_ProtectedVariables_MissingColumn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Protected variable {0} is not found in provided questionnaire.
        /// </summary>
        public static string PL0048_ProtectedVariables_VariableNotFoundInQuestionnaire {
            get {
                return ResourceManager.GetString("PL0048_ProtectedVariables_VariableNotFoundInQuestionnaire", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Variable {0} has type that does not support protection.
        /// </summary>
        public static string PL0049_ProtectedVariables_VariableNotSupportsProtection {
            get {
                return ResourceManager.GetString("PL0049_ProtectedVariables_VariableNotSupportsProtection", resourceCulture);
            }
        }
    }
}
