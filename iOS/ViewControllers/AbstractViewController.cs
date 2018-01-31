using System;
using Foundation;
using UIKit;

namespace Happimeter.iOS.ViewControllers
{
    public class AbstractViewController : UIViewController
    {
        public AbstractViewController(IntPtr handle) : base(handle)
        {
        }


        #region Keyboard adjust

        /// <summary>
        /// Set this field to any view inside the scroll view to center this view instead of the current responder
        /// </summary>
        protected UIView ViewToCenterOnKeyboardShown;
        public virtual bool HandlesKeyboardNotifications()
        {
            return false;
        }

        protected virtual void RegisterForKeyboardNotifications()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
        }

        /// <summary>
        /// Gets the UIView that represents the "active" user input control (e.g. textfield, or button under a text field)
        /// </summary>
        /// <returns>
        /// A <see cref="UIView"/>
        /// </returns>
        protected virtual UIView KeyboardGetActiveView()
        {
            return View.FindFirstResponder();
        }

        private void OnKeyboardNotification(NSNotification notification)
        {
            if (!IsViewLoaded) return;

            //Check if the keyboard is becoming visible
            var visible = notification.Name == UIKeyboard.WillShowNotification;

            //Start an animation, using values from the keyboard
            UIView.BeginAnimations("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState(true);
            UIView.SetAnimationDuration(UIKeyboard.AnimationDurationFromNotification(notification));
            UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

            //Pass the notification, calculating keyboard height, etc.
            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var keyboardFrame = visible
                                    ? UIKeyboard.FrameEndFromNotification(notification)
                                    : UIKeyboard.FrameBeginFromNotification(notification);

            OnKeyboardChanged(visible, landscape ? keyboardFrame.Width : keyboardFrame.Height);

            //Commit the animation
            UIView.CommitAnimations();
        }

        /// <summary>
        /// Override this method to apply custom logic when the keyboard is shown/hidden
        /// </summary>
        /// <param name='visible'>
        /// If the keyboard is visible
        /// </param>
        /// <param name='keyboardHeight'>
        /// Calculated height of the keyboard (width not generally needed here)
        /// </param>
        protected virtual void OnKeyboardChanged(bool visible, nfloat keyboardHeight)
        {
            var activeView = ViewToCenterOnKeyboardShown ?? KeyboardGetActiveView();
            if (activeView == null)
                return;

            var scrollView = activeView.FindSuperviewOfType(View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;

            if (!visible)
                RestoreScrollPosition(scrollView);
            else
                CenterViewInScroll(activeView, scrollView, keyboardHeight);
        }

        protected virtual void CenterViewInScroll(UIView viewToCenter, UIScrollView scrollView, nfloat keyboardHeight)
        {
            var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
            scrollView.ContentInset = contentInsets;
            scrollView.ScrollIndicatorInsets = contentInsets;

            // Position of the active field relative isnside the scroll view
            var relativeFrame = viewToCenter.Superview.ConvertRectToView(viewToCenter.Frame, scrollView);

            bool landscape = InterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || InterfaceOrientation == UIInterfaceOrientation.LandscapeRight;
            var spaceAboveKeyboard = (landscape ? scrollView.Frame.Width : scrollView.Frame.Height) - keyboardHeight;

            // Move the active field to the center of the available space
            var offset = relativeFrame.Y - (spaceAboveKeyboard - viewToCenter.Frame.Height) / 2;
            scrollView.ContentOffset = new CoreGraphics.CGPoint(0, offset);
        }

        protected virtual void RestoreScrollPosition(UIScrollView scrollView)
        {
            scrollView.ContentInset = UIEdgeInsets.Zero;
            scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }

        /// <summary>
        /// Call it to force dismiss keyboard when background is tapped
        /// </summary>
        protected void DismissKeyboardOnBackgroundTap()
        {
            // Add gesture recognizer to hide keyboard
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => View.EndEditing(true));
            View.AddGestureRecognizer(tap);
        }

        #endregion


    }
}
