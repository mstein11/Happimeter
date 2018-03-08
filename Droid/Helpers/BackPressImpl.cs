using System;
using System.Linq;
using Android.Support.V4.App;

namespace Happimeter.Droid.Helpers
{
    public class BackPressImpl : OnBackPressListener
    {
        public Fragment ParentFragment { get; }

        public BackPressImpl(Fragment parentFragment)
        {
            ParentFragment = parentFragment;
        }

        public bool OnBackPressed()
        {
            if (ParentFragment == null) {
                return false;
            }

            var childCount = ParentFragment.ChildFragmentManager.BackStackEntryCount;

            if (childCount == 0) {
                return false;
            }
            var childFragmentMngr = ParentFragment.ChildFragmentManager;
            var childFrag = childFragmentMngr.Fragments.FirstOrDefault() as OnBackPressListener;

            if (childFrag == null || !childFrag.OnBackPressed()) {
                childFragmentMngr.PopBackStackImmediate();
            }

            return true;
        }
    }
}
