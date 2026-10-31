using LagoVista.PickAndPlace.VisionWorkbench.Services;
using System.Windows;

namespace LagoVista.PickAndPlace.VisionWorkbench;

public partial class LoginWindow : Window
{
    private readonly WorkbenchPlatformClient _platform;

    public LoginWindow(WorkbenchPlatformClient platform)
    {
        InitializeComponent();
        _platform = platform;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        StatusText.Text = "Signing in...";
        try
        {
            var result = await _platform.LoginAsync(EmailTextBox.Text.Trim(), PasswordBox.Password);
            if (!result.Success)
            {
                StatusText.Text = result.Error ?? "Sign in failed.";
                return;
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }
}
