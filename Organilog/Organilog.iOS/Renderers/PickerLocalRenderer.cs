using System;
using System.ComponentModel;
using Organilog.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Picker), typeof(PickerLocalRenderer))]
namespace Organilog.iOS.Renderers
{
    public class PickerLocalRenderer : PickerRenderer
    {

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            var toolBar = (UIToolbar)Control.InputAccessoryView;
            var doneButton = toolBar.Items[1];

            toolBar.BarTintColor = UIColor.FromPatternImage(UIImage.FromFile("tempcolor.png")); //Color.FromHex("#1E90FF").ToUIColor();


        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

        }
    }
}
