using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Happimeter.Watch.Droid.Activities;
using Happimeter.Watch.Droid.ViewModels;
using System.Linq;

namespace Happimeter.Watch.Droid.Fragments
{
	public class SurveyItemFragment : Fragment
	{
		private const string ViewModelIdentifier = "SurveyItemFragmentViewModel";

		public static SurveyItemFragment NewInstance(int modelPosition)
		{
			var bundle = new Bundle();
			bundle.PutInt(ViewModelIdentifier, modelPosition);
			return new SurveyItemFragment { Arguments = bundle };
		}

		public SurveyFragmentViewModel ViewModel => ((SurveyActivity)Activity).ViewModel.Questions[ViewModelPosition];
		public int ViewModelPosition = 0;

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			var bundleToWorkWith = savedInstanceState != null ? savedInstanceState : Arguments;

			var viewModelPosition = bundleToWorkWith.GetInt(ViewModelIdentifier);
			ViewModelPosition = viewModelPosition;
		}



		public void PopulateView()
		{
			View.FindViewById<TextView>(Resource.Id.surveyQuestion).Text = ViewModel.Question;
			View.FindViewById<TextView>(Resource.Id.surveyAnserIndicator).Text = ViewModel.AnswerDisplay.ToString();
			View.FindViewById<SeekBar>(Resource.Id.surveyAnswerSeekbar).Progress = ViewModel.Answer ?? 0;
			View.FindViewById<SeekBar>(Resource.Id.surveyAnswerSeekbar).ProgressChanged += (sender, e) => ViewModel.Answer = e.Progress;
			ViewModel.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(ViewModel.AnswerDisplay))
				{
					View.FindViewById<TextView>(Resource.Id.surveyAnserIndicator).Text = (sender as SurveyFragmentViewModel)?.AnswerDisplay.ToString();
					SetMoodImage();
				}
			};
			SetMoodImage();

		}


		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var view = inflater.Inflate(Resource.Layout.Fragment_SurveyItem, container, false);
			return view;
		}

		public override void OnStart()
		{
			base.OnStart();
			PopulateView();
			//learnMoreButton.Click += LearnMoreButton_Click;
		}

		public override void OnStop()
		{
			base.OnStop();
			//learnMoreButton.Click -= LearnMoreButton_Click;
		}

		public void BecameVisible()
		{

		}

		void LearnMoreButton_Click(object sender, System.EventArgs e)
		{
			//ViewModel.OpenWebCommand.Execute(null);
		}

		private void SetMoodImage()
		{
			if (ViewModel.QuestionId != 1 && ViewModel.QuestionId != 2)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).Visibility = ViewStates.Invisible;
			}
			var activation = ViewModel?.ParentViewModel?.Questions.FirstOrDefault(x => x.QuestionId == 1)?.AnswerDisplay ?? 1;
			var pleasance = ViewModel?.ParentViewModel?.Questions.FirstOrDefault(x => x.QuestionId == 2)?.AnswerDisplay ?? 1;
			if (activation == 0 && pleasance == 0)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_0_h_0);
			}
			else if (activation == 1 && pleasance == 0)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_1_h_0);
			}
			else if (activation == 2 && pleasance == 0)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_2_h_0);
			}
			else if (activation == 0 && pleasance == 1)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_0_h_1);
			}
			else if (activation == 1 && pleasance == 1)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_1_h_1);
			}
			else if (activation == 2 && pleasance == 1)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_2_h_1);
			}
			else if (activation == 0 && pleasance == 2)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_0_h_2);
			}
			else if (activation == 1 && pleasance == 2)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_1_h_2);
			}
			else if (activation == 2 && pleasance == 2)
			{
				View.FindViewById<ImageView>(Resource.Id.moodImageView).SetImageResource(Resource.Drawable.a_2_h_2);
			}
		}
	}
}
