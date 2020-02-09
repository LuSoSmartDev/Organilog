using System;
using System.Drawing;
using Organilog.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(NumericTextBox), typeof(NumericTextBoxRenderer))]
namespace Organilog.iOS.Renderers
{
    public class NumericTextBoxRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Element == null)
                return;
            // Check only for Numeric keyboard
            if (this.Element.Keyboard == Keyboard.Numeric)
                this.AddDoneButton();
        }
        /// <summary>
        /// <para>Add toolbar with Done button</para>
        /// </summary>
        protected void AddDoneButton()
        {
            var toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                this.Control.ResignFirstResponder();
                var baseEntry = this.Element.GetType();
                ((IEntryController)Element).SendCompleted();
            });
            toolbar.Items = new UIBarButtonItem[] {
                        new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                        doneButton
            };
            this.Control.InputAccessoryView = toolbar;
        }
    }

}
