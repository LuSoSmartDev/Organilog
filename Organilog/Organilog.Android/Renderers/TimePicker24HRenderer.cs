using System;
using Android.App;
using Android.Runtime;
using Organilog.Droid.Renderers;
using Xamarin.Forms;

using Xamarin.Forms.Platform.Android;
/*
[assembly: ExportRenderer(typeof(TimePicker), typeof(TimePicker24HRenderer))]
namespace Organilog.Droid.Renderers
{

#pragma warning disable CS0618 // Type or member is obsolete
    public class TimePicker24HRenderer : ViewRenderer<Xamarin.Forms.TimePicker, Android.Widget.EditText>, TimePickerDialog.IOnTimeSetListener, IJavaObject, IDisposable
    {
        private TimePickerDialog dialog = null;

        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            this.SetNativeControl(new Android.Widget.EditText(Forms.Context));
            this.Control.Click += Control_Click;
            this.Control.Text =  e.NewElement.Time.ToString(@"hh\:mm");
            this.Control.KeyListener = null;
           
            this.Control.FocusChange += Control_FocusChange;
        }

        void Control_FocusChange(object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            if (e.HasFocus) { ShowTimePicker(); }
        }

        void Control_Click(object sender, EventArgs e)
        {
            ShowTimePicker();
        }

        private void ShowTimePicker()
        {
            if (dialog == null)
            {
                dialog = new TimePickerDialog(Forms.Context, this, DateTime.Now.Hour, DateTime.Now.Minute, true);
            }

            dialog.Show();
        }

        public void OnTimeSet(Android.Widget.TimePicker view, int hourOfDay, int minute)
        {
            var time = new TimeSpan(hourOfDay, minute, 0);    
            this.Element.SetValue(TimePicker.TimeProperty, time);

            this.Control.Text = time.ToString(@"hh\:mm");
        }

        #pragma warning restore CS0618 // Type or member is obsolete
    }
}*/
