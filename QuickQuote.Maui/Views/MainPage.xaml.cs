using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickQuote.Maui.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public void OnButtonClick (object sender, EventArgs e)
    {
        Test.Text = "You Clicked Me!";
    }
}