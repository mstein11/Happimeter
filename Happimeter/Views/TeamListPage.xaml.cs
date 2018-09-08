using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using Happimeter.ViewModels.Forms.Teams;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Happimeter.Converter;

namespace Happimeter.Views
{
    public partial class TeamListPage : ContentPage
    {
        private TeamListViewModel ViewModel;
        public TeamListPage()
        {
            Resources = App.ResourceDict;

            InitializeComponent();
            ViewModel = new TeamListViewModel();
            BindingContext = ViewModel;
            ViewModel.JoinTeamViewModel.WhenTeamSuccessfullyJoined().Subscribe(async x =>
            {
                await Task.Delay(500);
                await _handleClose();
            });
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            await AnimateIn();
        }

        async void Handle_Close(object sender, System.EventArgs e)
        {
            await _handleClose();
        }
        private async Task _handleClose()
        {
            await AnimateOut();
            ViewModel.JoinTeamViewModel.Reset();
        }

        private bool IsAnimated = false;


        private async Task AnimateIn()
        {

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Overlay.IsVisible = true;
            Overlay.FadeTo(0.7, 500);
            FormContentContainer.TranslateTo(0, 0, 250, Easing.SpringOut);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await JoinContainer.TranslateTo(JoinContainer.Width, 0, 250, Easing.SpringIn);
            await FormActionContainer.TranslateTo(0, 0, 250, Easing.SpringOut);
            IsAnimated = true;

        }

        private async Task AnimateOut()
        {
            IsAnimated = false;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Overlay.FadeTo(0, 500);
            FormContentContainer.TranslateTo(0, Height, 250, Easing.SpringIn);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await FormActionContainer.TranslateTo(FormActionContainer.Width, 0, 250, Easing.SpringIn);
            await JoinContainer.TranslateTo(0, 0, 250, Easing.SpringOut);
            Overlay.IsVisible = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!IsAnimated)
            {
                FormActionContainer.TranslationX = FormActionContainer.Width;
                FormContentContainer.TranslationY = height;
            }
            else
            {
                JoinContainer.TranslationX = JoinContainer.Width;
            }

        }
    }
}
