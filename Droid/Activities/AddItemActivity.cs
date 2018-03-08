using System;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.Design.Widget;
using Happimeter.Views;
using Xamarin.Forms.Platform.Android;

namespace Happimeter.Droid
{
    [Activity(Label = "AddItemActivity")]
    public class AddItemActivity : Activity
    {
        FloatingActionButton saveButton;
        EditText title, description;

        public ItemsViewModel ViewModel { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel = BrowseFragment.ViewModel;

            // Create your application here
            SetContentView(Resource.Layout.activity_add_item);
            saveButton = FindViewById<FloatingActionButton>(Resource.Id.save_button);
            var frameLayout = FindViewById<FrameLayout>(Resource.Id.test_frame_layout);
            //var fragment = SurveyItemFragment.NewInstance(ViewModel.GetCurrentQuestionPosition());
            var fragment = new InitializeSurveyView().CreateFragment(Application.Context);
            // In case this activity was started with special instructions from an
            // Intent, pass the Intent's extras to the fragment as arguments
            //firstFragment.setArguments(getIntent().getExtras());

            var transaction = FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.test_frame_layout, fragment);
            transaction.Commit();
            //saveButton.Click += SaveButton_Click;
        }

        void SaveButton_Click(object sender, EventArgs e)
        {
            var item = new Item
            {
                Text = title.Text,
                Description = description.Text
            };
            ViewModel.AddItemCommand.Execute(item);

            Finish();
        }
    }
}
