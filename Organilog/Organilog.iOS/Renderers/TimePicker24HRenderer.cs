using System;
using Foundation;
using Organilog.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePicker24HRenderer))]
namespace Organilog.iOS.Renderers
{
    public class TimePicker24HRenderer : TimePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            var timePicker = (UIDatePicker)Control.InputView;
            timePicker.Locale = new NSLocale("no_nb");

            var toolBar = (UIToolbar)Control.InputAccessoryView;
            var doneButton = toolBar.Items[1];

            toolBar.BarTintColor = UIColor.FromPatternImage(UIImage.FromFile("tempcolor.png")); //Color.FromHex("#1E90FF").ToUIColor();

        }
    }
}

