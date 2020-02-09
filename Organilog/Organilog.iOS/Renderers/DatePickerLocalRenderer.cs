using System;
using System.ComponentModel;
using Organilog.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerLocalRenderer))]
namespace Organilog.iOS.Renderers
{
    public class DatePickerLocalRenderer: DatePickerRenderer
    {
        public DatePickerLocalRenderer()
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);
            var date = (UIDatePicker)Control.InputView;
            date.Locale = new Foundation.NSLocale("fr-FR");
            var toolBar = (UIToolbar)Control.InputAccessoryView;
            var doneButton = toolBar.Items[1];

            toolBar.BarTintColor = UIColor.FromPatternImage(UIImage.FromFile("tempcolor.png")); //Color.FromHex("#1E90FF").ToUIColor();
            if(Control!=null)
                Control.TextAlignment = UITextAlignment.Center;
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            Control.Layer.BorderWidth = 0;
            Control.BorderStyle = UITextBorderStyle.None;
        }
    }
}
