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
        private string _password;
        private string _emailAddress;

        public Splash()
        {
            InitializeComponent();
            this.Loaded += Splash_Loaded;
        }


        private async void Splash_Loaded(object sender, RoutedEventArgs e)
        {
            _authClient = SLWIOC.Get<IAuthClient>();
            _authManager = SLWIOC.Get<IAuthManager>();
            _appConfig = SLWIOC.Get<IAppConfig>();
            _restClient = SLWIOC.Get<IRestClient>();

            UserName.Text = "kevinw@slsys.net";

            await _authManager.LoadAsync();
            if(_authManager.IsAuthenticated)
            {
                if(_authManager.AccessTokenExpirationUTC.ToDateTime() < DateTime.UtcNow )
                {
                    var result = await _restClient.RenewRefreshToken();
                    if (!result.Successful)
                    {
                        MessageBox.Show(result.ErrorMessage);
                        return;
                    }
                }

                var main = new Home();
                main.Show();
                this.Close();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _emailAddress = UserName.Text;
            _password = Password.Text;

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

            //if (!EntityHeader.IsNullOrEmpty(_appConfig.SystemOwnerOrg))
            //{
            //    loginInfo.OrgId = _appConfig.SystemOwnerOrg.Id;
            //    loginInfo.OrgName = _appConfig.SystemOwnerOrg.Text;
            //}

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
