// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b101332fc04ff4109a57daf1aed0fc22600afc6220bfd2f83839dde0e887ca3f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Interfaces;
using System.Threading;
using System.Windows;
using LagoVista.Client.Core;
using System;
using LagoVista.Core;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {

        IAppConfig _appConfig;
        IAuthManager _authManager;
        IAuthClient _authClient;
        IRestClient _restClient;
        IStorageService _storageService;
        private string _password;
        private string _emailAddress;

        public Splash()
        {
            InitializeComponent();
            this.Loaded += Splash_Loaded;
            this.LocationChanged += Splash_LocationChanged;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Splash_LocationChanged(object sender, EventArgs e)
        {
            
        }

        private async void Splash_Loaded(object sender, RoutedEventArgs e)
        {
            _authClient = SLWIOC.Get<IAuthClient>();
            _authManager = SLWIOC.Get<IAuthManager>();
            _appConfig = SLWIOC.Get<IAppConfig>();
            _restClient = SLWIOC.Get<IRestClient>();
            _storageService = SLWIOC.Get<IStorageService>();

            UserName.Text = "kevinw@slsys.net";

            await _authManager.LoadAsync();
            if(_authManager.IsAuthenticated )
            {
                if(_authManager.AccessTokenExpirationUTC.ToDateTime() < DateTime.UtcNow )
                {
                    var result = await _restClient.RenewRefreshToken();
                    if (!result.Successful)
                    {
                        MessageBox.Show(result.ErrorMessage);
                        LoginSection.Visibility = Visibility.Visible;
                        return;
                    }
                }

                var previousOrg = _authManager.User.CurrentOrganization;

                var currentOrgResult = await _restClient.GetAsync<InvokeResult<OrganizationSummary>>("/api/org/current/summary", new CancellationTokenSource());
                if (currentOrgResult.Successful)
                {
                    if (previousOrg.Id == currentOrgResult.Result.Result.Id)
                    {
                        var main = new Home();
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        var result = MessageBox.Show($"Your previous organization was {previousOrg.Text}, your last organization on the server was {currentOrgResult.Result.Result.Name}, would you like to chnage to your previous organization?  If you do not, you will not be able to load any machines or jobs from the previous organization. ",
                             "Change Organization?", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            var getUserReault = await _restClient.GetAsync<InvokeResult<AppUser>>($"/api/org/{previousOrg.Id}/change");
                            if (getUserReault.Successful)
                            {
                                _authManager.User = getUserReault.Result.Result.ToUserInfo();
                                await _authManager.PersistAsync();

                                var main = new Home();
                                main.Show();
                                this.Close();
                            }
                        }
                        else
                        {
                            var main = new Home();
                            main.Show();
                            this.Close();
                        }
                    }
                }
            }
            else
            {
                LoginSection.Visibility = Visibility.Visible;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _emailAddress = UserName.Text;
            _password = Password.Password;

            var loginInfo = new AuthRequest()
            {
                AuthType = _appConfig.AuthType,
                DeviceRepoId = _appConfig.DeviceRepoId,
                AppId = _appConfig.AppId,
                DeviceId = _appConfig.DeviceId,
                ClientType = _appConfig.ClientType,
                Email = _emailAddress,
                Password = _password,
                UserName = _emailAddress,
                GrantType = "password"
            };

            var loginResult = await _authClient.LoginAsync(loginInfo);
            if (!loginResult.Successful)
            {
                MessageBox.Show(loginResult.ErrorMessage);
            }
            else
            {
                var authResult = loginResult.Result;
                _authManager.AccessToken = authResult.AccessToken;
                _authManager.AccessTokenExpirationUTC = authResult.AccessTokenExpiresUTC;
                _authManager.RefreshToken = authResult.RefreshToken;
                _authManager.AppInstanceId = authResult.AppInstanceId;
                _authManager.RefreshTokenExpirationUTC = authResult.RefreshTokenExpiresUTC;
                _authManager.IsAuthenticated = true;
                var getUserResult = await _restClient.GetAsync("/api/user", new CancellationTokenSource());
                if (getUserResult.Success)
                {
                    _authManager.User = getUserResult.DeserializeContent<DetailResponse<UserInfo>>().Model;
                    await _authManager.PersistAsync();
                }

                var main = new Home();
                main.Show();

                this.Close();
            }

        }
    }

}
