using System;
using System.Drawing;
using Organilog.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedEntry), typeof(ExtendedEntryRenderer))]
namespace Organilog.iOS.Renderers
{
    public class ExtendedEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            // Check for only Numeric keyboard
            if (this.Element != null &&  this.Element.Keyboard == Keyboard.Numeric)
            {
                this.AddDoneButton();

            }
        }

        /// <summary>
        /// Add toolbar with Done button
        /// </summary>
        protected void AddDoneButton()
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));
            toolbar.BarTintColor = UIColor.FromPatternImage(UIImage.FromFile("tempcolor.png"));
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate {
                this.Control.ResignFirstResponder();
            });

            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton
            };
            Control.InputAccessoryView = toolbar;
        }
    }
}
