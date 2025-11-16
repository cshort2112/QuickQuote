using QuickQuote.Maui.Services;

namespace QuickQuote.Maui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void Submit(object sender, EventArgs e)
    {
        try
        {
            var result = await ApiService.LoginAsync(Username.Text, Password.Text);
            if (result != null)
            {
                await SecureStorage.SetAsync("token", result.Token);
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage.Text = "Invalid username or password";
                ErrorMessage.TextColor = new Color(255, 0, 0, 255);
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
        
        

    }
}