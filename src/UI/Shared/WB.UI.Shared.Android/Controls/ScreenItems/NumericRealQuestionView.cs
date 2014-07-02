using System;
using System.Globalization;
using Android.Content;
using Android.Text;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class NumericRealQuestionView : NumericQuestionView<decimal>
    {
        public NumericRealQuestionView(Context context, IMvxAndroidBindingContext bindingActivity,
            QuestionViewModel source, Guid questionnairePublicKey,
            ICommandService commandService,
            IAnswerOnQuestionCommandService answerCommandService,
            IAuthentication membership)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
        }

        protected override InputTypes KeyboardTypeFlags
        {
            get { return InputTypes.ClassNumber | InputTypes.NumberFlagDecimal | InputTypes.NumberFlagSigned; }
        }

        protected override string FormatString(string s)
        {
            if (s.EndsWith(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator))
                return s;
            decimal parsedAnswer;
            if (!IsParseAnswerStringSucceeded(s, out parsedAnswer))
                return s;
            return parsedAnswer.ToString("##,###.############################", CultureInfo.CurrentCulture);
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.AttachDecimalPlacesFilterToEditTextIfNeeded();
        }

        private void AttachDecimalPlacesFilterToEditTextIfNeeded()
        {
            var valueModel = this.Model as ValueQuestionViewModel;

            if (valueModel == null)
                throw new InvalidCastException("Something bad happened with mapping question's viewmodel to question's view");

            if (valueModel.CountOfDecimalPlaces.HasValue)
                this.etAnswer.SetFilters(new IInputFilter[] { new DecimalPlacesFilter(valueModel.CountOfDecimalPlaces.Value) });
        }

        protected override bool IsParseAnswerStringSucceeded(string newAnswer, out decimal answer)
        {
            var replacedAnswer = newAnswer.Replace(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            Console.WriteLine("text to parse - {0}, with culture {1}", replacedAnswer, CultureInfo.CurrentCulture.Name);
            return decimal.TryParse(replacedAnswer, NumberStyles.Number, CultureInfo.CurrentCulture, out answer);
        }

        protected override AnswerQuestionCommand CreateAnswerQuestionCommand(decimal answer)
        {
            return new AnswerNumericRealQuestionCommand(this.QuestionnairePublicKey,
                this.Membership.CurrentUser.Id,
                this.Model.PublicKey.Id,
                this.Model.PublicKey.InterviewItemPropagationVector,
                DateTime.UtcNow, answer);
        }
    }
}