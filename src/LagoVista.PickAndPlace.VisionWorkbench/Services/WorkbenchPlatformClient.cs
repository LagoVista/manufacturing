using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Core;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Users;

namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

public sealed class WorkbenchPlatformClient
{
    private IAuthManager? _authManager;
    private IAuthClient? _authClient;
    private IRestClient? _restClient;
    private IAppConfig? _appConfig;

    public bool IsAuthenticated => _authManager?.IsAuthenticated == true;
    public string UserLabel => _authManager?.User?.Name ?? "Signed in";

    public async Task InitializeAsync()
    {
        _authManager = SLWIOC.Get<IAuthManager>();
        _authClient = SLWIOC.Get<IAuthClient>();
        _restClient = SLWIOC.Get<IRestClient>();
        _appConfig = SLWIOC.Get<IAppConfig>();

        await _authManager.LoadAsync();
        if (_authManager.IsAuthenticated &&
            _authManager.AccessTokenExpirationUTC.ToDateTime() < DateTime.UtcNow)
        {
            var refreshed = await _restClient.RenewRefreshToken();
            if (!refreshed.Successful)
                _authManager.IsAuthenticated = false;
        }
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        if (_authManager is null || _authClient is null || _restClient is null || _appConfig is null)
            throw new InvalidOperationException("Platform client has not been initialized.");

        var request = new AuthRequest
        {
            AuthType = _appConfig.AuthType,
            DeviceRepoId = _appConfig.DeviceRepoId,
            AppId = _appConfig.AppId,
            DeviceId = _appConfig.DeviceId,
            ClientType = _appConfig.ClientType,
            Email = email,
            Password = password,
            UserName = email,
            GrantType = "password"
        };

        var login = await _authClient.LoginAsync(request);
        if (!login.Successful)
            return (false, login.ErrorMessage);

        var auth = login.Result;
        _authManager.AccessToken = auth.AccessToken;
        _authManager.AccessTokenExpirationUTC = auth.AccessTokenExpiresUTC;
        _authManager.RefreshToken = auth.RefreshToken;
        _authManager.AppInstanceId = auth.AppInstanceId;
        _authManager.RefreshTokenExpirationUTC = auth.RefreshTokenExpiresUTC;
        _authManager.IsAuthenticated = true;

        var userResult = await _restClient.GetAsync("/api/user", new CancellationTokenSource());
        if (!userResult.Success)
            return (false, userResult.ErrorMessage);

        _authManager.User = userResult.DeserializeContent<DetailResponse<UserInfo>>().Model;
        await _authManager.PersistAsync();
        return (true, null);
    }

    public async Task<IReadOnlyList<MachineSummary>> GetMachinesAsync()
    {
        if (_restClient is null || !IsAuthenticated)
            return Array.Empty<MachineSummary>();

        var response = await _restClient.GetListResponseAsync<MachineSummary>(
            "/api/mfg/machines", ListRequest.CreateForAll());
        if (!response.Successful)
            throw new InvalidOperationException(response.ErrorMessage);

        return response.Model.ToArray();
    }

    public async Task<LagoVista.Manufacturing.Models.Machine> LoadMachineAsync(string id)
    {
        if (_restClient is null || !IsAuthenticated)
            throw new InvalidOperationException("Sign in before loading a machine.");

        var response = await _restClient.GetAsync<DetailResponse<LagoVista.Manufacturing.Models.Machine>>(
            $"/api/mfg/machine/{id}");
        if (!response.Successful)
            throw new InvalidOperationException(response.ErrorMessage);

        return response.Result.Model;
    }
}
