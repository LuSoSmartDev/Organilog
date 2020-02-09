using System;

using Xamarin.Forms;

namespace Organilog.Views.Shared
{
    public class GroupLookUpPage : ContentPage
    {
        public GroupLookUpPage()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

