using Organilog.iOS.Effects;
using System;
using UIKit;

[assembly: ResolutionGroupName("Organilog")]
[assembly: ExportEffect(typeof(BorderlessEffect), "BorderlessEffect")]

namespace Organilog.iOS.Effects
{
    public class BorderlessEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                Control.Layer.BorderWidth = 0;
                if (Control is UITextField entry)
                {
                    entry.BorderStyle = UITextBorderStyle.None;
                    //entry.AccessibilityDecrement.GetType == Type.FilterName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}