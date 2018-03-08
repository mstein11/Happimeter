﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Happimeter.Droid.Activities;
using Happimeter.Droid.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Happimeter.Droid.Fragments
{
    public class FragmentContainer : Android.Support.V4.App.Fragment, OnBackPressListener
    {
        
        public FragmentContainer(ContentPage childPage)
        {
            ChildPage = childPage;
        }
        public ContentPage ChildPage { get; set; }



        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            Android.Views.View rootView = inflater.Inflate(Resource.Layout.fragment_container, container, false);
            var transaction = ChildFragmentManager.BeginTransaction();

            var childFragment = ChildPage.CreateSupportFragment(TabMainActivity.Instance);
            transaction.Replace(Resource.Id.fragment_container_content, childFragment);
            transaction.Commit();
            return rootView;
        }

        public bool OnBackPressed()
        {
            return new BackPressImpl(this).OnBackPressed();
        }
    }
}
