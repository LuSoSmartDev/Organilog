using System;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
    public class NumericTextBox : Entry
    {
        public NumericTextBox()
        {
            this.Keyboard = Keyboard.Numeric;
        }
    }
}
