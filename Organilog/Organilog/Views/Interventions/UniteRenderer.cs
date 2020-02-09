using Organilog.Effects;
using Organilog.Models;
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Converters;
using Xamarin.Forms.Extensions;

namespace Organilog.Views.Interventions
{
    public class UniteRenderer : ContentView
    {
        public static readonly BindableProperty UniteProperty = BindableProperty.Create(nameof(Unite), typeof(Unite), typeof(UniteRenderer), null, BindingMode.TwoWay, propertyChanged: (b, o, n) => ((UniteRenderer)b).OnUniteChanged(b, o, n));

        public Unite Unite
        {
            get { return (Unite)GetValue(UniteProperty); }
            set { SetValue(UniteProperty, value); }
        }

        public UniteRenderer()
        {
        }

        private void OnUniteChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (Unite != null)
            {
                RenderView();
            }
        }

        private void RenderView()
        {
            try
            {
                switch (Unite.FieldType)
                {
                    case 1:
                        RenderEditor();
                        break;

                    case 3:
                        RenderYesNo();
                        break;

                    case 4:
                        RenderPicker();
                        break;

                    case 10:
                        RenderMultiChoice();
                        break;

                    case 12:
                        RenderLabel();
                        break;

                    default:
                        RenderEntry();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void RenderEditor()
        {
            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                TextColor = Color.Black
            };
            stk.Children.Add(title);

            var input = new Editor()
            {
                FontSize = 14,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                AutoSize = EditorAutoSizeOption.TextChanges,
                //BackgroundColor = Color.FromHex("F1F1F4")
            };
            input.SetBinding(Editor.TextProperty, new Binding("Value", BindingMode.TwoWay, source: Unite));
            //input.Effects.Add(new BottomLineEffect());
            
            stk.Children.Add(input);
            //stk.Children.Add(stk1);
            Content = stk;
        }

        private void RenderYesNo()
        {
            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                //Text = "input.SetBinding(ToggleBox.IsToggledProperty BindingMode.Two/ay, converter: new IntToBoolConverter",
                TextColor = Color.Black,
                VerticalOptions = LayoutOptions.Center,

                WidthRequest = 280,
            };
            

            stk.Children.Add(title);

            var input = new ToggleBox()
            {
                HeightRequest = 28,
                WidthRequest = 30,
                CheckedImage = "ic_checked_box_black",
                UnCheckedImage = "ic_unchecked_box_black",
                VerticalOptions = LayoutOptions.Center
            };

            input.SetBinding(ToggleBox.IsToggledProperty, new Binding("Value", BindingMode.TwoWay, converter: new IntToBoolConverter(), source: Unite));
            if (!string.IsNullOrWhiteSpace(Unite.Value) && ConvertToBool(Unite.Value, out bool value))
            {
                input.IsToggled = value;
            }
            stk.Children.Add(input);

            Content = stk;
        }

        public bool ConvertToBool(object value, out bool result)
        {
            try
            {
                result =  Convert.ToInt32(value) != 0;
                return true;
            }
            catch
            {
                result = false;
                return false;
            }
        }

        private void RenderPicker()
        {
            if (string.IsNullOrWhiteSpace(Unite.TypeStr))
            {
                RenderEntry();
                return;
            }

            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                TextColor = Color.Black
            };
            stk.Children.Add(title);

            var input = new Picker()
            {
                Title = TranslateExtension.GetValue("select"),
                FontSize = 14,
                TextColor = Color.Black,
                ItemDisplayBinding = new Binding("Value")
            };
            input.SetBinding(Picker.ItemsSourceProperty, new Binding("UniteItems", BindingMode.TwoWay, source: Unite));
            input.SelectedIndexChanged += Input_SelectedIndexChanged;
            if (!string.IsNullOrWhiteSpace(Unite.Value) && Unite.UniteItems.Find(ui => ui.Value.Equals(Unite.Value)) is UniteItem uniteItem)
            {
                input.SelectedItem = uniteItem;
            }
            stk.Children.Add(input);

            Content = stk;
        }

        private void Input_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is Picker picker && Unite.UniteItems.ElementAt(picker.SelectedIndex) is UniteItem uniteItem)
            {
                Unite.Value = uniteItem.Value;
            }
        }

        private void RenderMultiChoice()
        {
            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                TextColor = Color.Black,
                VerticalOptions = LayoutOptions.Center
            };
            stk.Children.Add(title);

            string[] choice = null;

            if (!string.IsNullOrWhiteSpace(Unite.Value))
            {
               
                if(Unite.Value.Length>3 && ( Unite.Value.Contains(",") || Unite.Value.Contains(";"))) {

                    string[] dataItem;
                    if(Unite.Value.Contains(","))
                        dataItem = Unite.Value.Split(',');
                    else
                        dataItem = Unite.Value.Split(';');
                    int n;
                    if (dataItem != null && dataItem.Length > 0 && int.TryParse(dataItem[0], out n))
                    {
                        var temp = "";
                        for (int i = 0; i < dataItem.Length; i++)
                        {
                            var uniteItem = App.LocalDb.Table<UniteItem>().ToList().FirstOrDefault(uni => uni.ServerId == int.Parse(dataItem[i]));
                            temp += uniteItem.Value + ";";
                        }
                        if (temp.Length > 1)
                            Unite.Value = temp;
                    }
                }

                choice = Unite.Value.Split(';');
            }

            foreach (var ui in Unite.UniteItems)
            {
                var stk2 = new StackLayout()
                {
                    Orientation = StackOrientation.Horizontal,
                   
                };
               
                
                var input2 = new ToggleBox()
                {
                    HeightRequest = 25,
                    WidthRequest = 25,
                    CheckedImage = "ic_checked_box_black",
                    UnCheckedImage = "ic_unchecked_box_black"
                };
                input2.SetBinding(ToggleBox.IsToggledProperty, new Binding("Selected", BindingMode.TwoWay, source: ui));

                if (choice != null && choice.Any(c => c.Trim().Equals(ui.Value.Trim())))
                {
                    input2.IsToggled = true;
                }

                stk2.Children.Add(input2);

                var title2 = new Label()
                {
                    FontSize = 14,
                    Text = ui.Value,
                    TextColor = Color.Black,
                    VerticalOptions = LayoutOptions.Center
                };
                stk2.Children.Add(title2);

                stk.Children.Add(stk2);
            }

            Content = stk;
        }

        private void RenderLabel()
        {
            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                TextColor = Color.Black
            };
            if (!string.IsNullOrWhiteSpace(Unite.Value))
            {
                title.Text = Unite.Value;
            }
            stk.Children.Add(title);

            Content = stk;
        }

        private void RenderEntry()
        {
            var stk = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Spacing = 0
            };

            var title = new Label()
            {
                FontSize = 14,
                Text = Unite.Name,
                TextColor = Color.Black
            };
            stk.Children.Add(title);

            var input = new Entry()
            {
                FontSize = 14,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            var stk1 = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Padding = 0.5,
                //BackgroundColor = Color.FromHex("F1F1F4"),
                
            };
            switch (Unite.FieldType)
            {
                case 2:
                    input.Keyboard = Keyboard.Numeric;
                    break;

                case 5:
                    input.Keyboard = Keyboard.Numeric;
                    break;

                case 6:
                    input.Keyboard = Keyboard.Numeric;
                    break;

                case 7:
                    input.Keyboard = Keyboard.Default;
                    break;

                case 9:
                    input.Keyboard = Keyboard.Telephone;
                    break;
            }
            input.SetBinding(Entry.TextProperty, new Binding("Value", BindingMode.TwoWay, source: Unite));
            //input.Effects.Add(new BottomLineEffect());
            if (!string.IsNullOrWhiteSpace(Unite.Value))
            {
                input.Text = Unite.Value;
            }
            stk.Children.Add(input);
            //stk.Children.Add(stk1);

            Content = stk;
        }
    }
}