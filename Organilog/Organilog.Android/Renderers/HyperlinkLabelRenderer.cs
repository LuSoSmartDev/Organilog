using Android.Content;
using Android.Text.Util;

using Java.Lang;
using Java.Util.Regex;
using Organilog.Droid.Renderers;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(HyperlinkLabel), typeof(HyperlinkLabelRenderer))]
namespace Organilog.Droid.Renderers
{
  
   public class HyperlinkLabelRenderer : LabelRenderer
    {
        public HyperlinkLabelRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != e.OldElement)
            {
                if (e.OldElement != null)
                    e.OldElement.PropertyChanged -= Element_PropertyChanged;

                if (e.NewElement != null)
                    e.NewElement.PropertyChanged += Element_PropertyChanged;
            }
            SetText();
        }

        private void SetText()
        {
            if (Element is HyperlinkLabel hyperlinkLabelElement && hyperlinkLabelElement != null)
            {
                string text = hyperlinkLabelElement.GetText(out List<HyperlinkLabelLink> links);
                Control.Text = text;
                if (text != null)
                {
                    foreach (var item in links)
                    {
                        var pattern = Pattern.Compile(item.Text);
                        Linkify.AddLinks(Control, pattern, "",
                            new CustomMatchFilter(item.Start),
                            new CustomTransformFilter(item.Link));
                        Control.MovementMethod = Android.Text.Method.LinkMovementMethod.Instance;
                    }
                }
            }
        }

        private void Element_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == HyperlinkLabel.RawTextProperty.PropertyName)
                SetText();
        }
    }

    public class CustomTransformFilter : Object, Linkify.ITransformFilter
    {
        readonly string url;
        public CustomTransformFilter(string url)
        {
            this.url = url;
        }

        public string TransformUrl(Matcher match, string url)
            => this.url;
    }

    public class CustomMatchFilter : Object, Linkify.IMatchFilter
    {
        readonly int start;
        public CustomMatchFilter(int start)
        {
            this.start = start;
        }

        public bool AcceptMatch(ICharSequence s, int start, int end)
            => start == this.start;
    }
}
