using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Android.Controls.MaskedEditTextControl;
using WB.UI.Shared.Android.Helpers;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class PictureQuestionView : AbstractQuestionView
    {
        public PictureQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey, ICommandService commandService,
                                IAnswerOnQuestionCommandService answerCommandService, IAuthentication membership, IPlainFileRepository plainFileRepository)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.plainFileRepository = plainFileRepository;

            if (this.IsThereAnAppToTakePictures())
            {
                this.pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                var wrapper = new LinearLayout(this.Context)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent)
                };
                wrapper.Orientation = Orientation.Horizontal;

                this.llWrapper.AddView(wrapper);
               
                var imageLayoutParams = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent,4);
                
                this.ivImage = new ImageView(this.Context) { LayoutParameters = imageLayoutParams };
                ivImage.SetAdjustViewBounds(true);
                ivImage.SetScaleType(ImageView.ScaleType.Center);
                wrapper.AddView(this.ivImage);

                var buttonLayoutParams = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent,1);

                var button = new Button(this.Context) { Text = this.IsPicturePresent() ? this.Remove : this.TakePicture, LayoutParameters = buttonLayoutParams };
                button.Click += this.BtnTakePictureClick;
                wrapper.AddView(button);


                
                this.PutAnswerStoredInModelToUI();
            }
            else
            {
                var tvWarning = new TextView(this.Context);
                tvWarning.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);
                tvWarning.Text = "Camera is absent";
                this.llWrapper.AddView(tvWarning);
            }
        }

        private readonly IPlainFileRepository plainFileRepository;
        private readonly IMvxPictureChooserTask pictureChooserTask;
        protected readonly ImageView ivImage;
        private readonly string TakePicture = "Take Picture";
        private readonly string Remove = "Remove";

        protected ValueQuestionViewModel TypedMode
        {
            get { return this.Model as ValueQuestionViewModel; }
        }

        protected bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities = Context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected void BtnTakePictureClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            if (IsPicturePresent())
            {
                plainFileRepository.RemoveInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
                ivImage.SetImageDrawable(null);
                this.SavePictureToAR(string.Empty);

                button.Text = TakePicture;
            }
            else
            {
                pictureChooserTask.TakePicture(400, 95, (s) => OnPicture(s, button), () => { });
            }
        }

        private void OnPicture(Stream pictureStream, Button button)
        {
            var pictureFileName = String.Format("{0}{1}.jpg", Model.Variable,
                string.Join("-", Model.PublicKey.InterviewItemPropagationVector));
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                pictureStream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                plainFileRepository.StoreInterviewBinaryData(this.QuestionnairePublicKey, pictureFileName, data);
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            ivImage.SetImageBitmap(bitmap);
            this.SavePictureToAR(pictureFileName);
            button.Text = Remove;
        }

        private void SavePictureToAR(string pictureFileName)
        {
            this.SaveAnswer(pictureFileName,
               new AnswerPictureQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                   questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                   answerTime: DateTime.UtcNow, pictureFileName: pictureFileName));
        }

        protected bool IsPicturePresent()
        {
            return !string.IsNullOrEmpty(this.Model.AnswerString);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            if (!IsPicturePresent())
                return;

            var bytes = plainFileRepository.GetInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
            if (bytes == null || bytes.Length == 0)
            {
                ivImage.SetImageResource(Resource.Drawable.no_image_found);
                return;
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            ivImage.SetImageBitmap(bitmap);
        }
    }
}