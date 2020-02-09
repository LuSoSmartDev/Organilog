using System;
using System.Linq;
using Organilog.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using FormsShadowEffectEffect = Organilog.Effects.ShadowEffect;

[assembly: ExportEffect(typeof(ShadowEffect), "ShadowEffect")]
namespace Organilog.Droid.Effects
{
    public class ShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var control = Control as Android.Widget.TextView;
                var effect = (FormsShadowEffectEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);
                if (effect != null)
                {
                    float radius = effect.Radius;
                    float distanceX = effect.DistanceX;
                    float distanceY = effect.DistanceY;
                    Android.Graphics.Color color = effect.Color.ToAndroid();
                    control.SetShadowLayer(radius, distanceX, distanceY, color);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: {0} ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
