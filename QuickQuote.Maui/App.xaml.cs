namespace QuickQuote.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        
        
    }

    private async void CheckAutoLogin()
    {
        var token = await SecureStorage.GetAsync("token");
        if (!string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}