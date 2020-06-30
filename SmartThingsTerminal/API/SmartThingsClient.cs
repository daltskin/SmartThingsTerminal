using SmartThingsNet.Api;
using SmartThingsNet.Client;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SmartThingsTerminal
{
    public class SmartThingsClient : IDisposable
    {
        private bool _disposedValue;

        private PagedDevices _allDevices;
        private PagedLocations _allLocations;
        private PagedRooms _allRooms;
        private PagedScene _allScenes;
        private PagedRules _allRules;
        private PagedSchedules _allSchedules;
        private PagedApps _allApps;
        private PagedSubscriptions _allSubscriptions;
        private PagedInstalledApps _allInstalledApps;
        private PagedDeviceProfiles _allDeviceProfiles;

        private DevicesApi _devicesApi;
        private LocationsApi _locationsApi;
        private RoomsApi _roomsApi;
        private ScenesApi _scenesApi;
        private RulesApi _rulesApi;
        private SchedulesApi _schedulesApi;
        private AppsApi _appsApi;
        private SubscriptionsApi _subscriptionsApi;
        private InstalledAppsApi _installedAppsApi;
        private DeviceProfilesApi _deviceProfilesApi;

        public SmartThingsClient(string accessToken)
        {
            var configuration = new Configuration();
            configuration.AccessToken = accessToken ?? throw new ArgumentNullException(accessToken);
            //configuration.BasePath = "https://graph-eu01-euwest1.api.smartthings.com/v1";

            _devicesApi = new DevicesApi(configuration);
            _locationsApi = new LocationsApi(configuration);
            _roomsApi = new RoomsApi(configuration);
            _scenesApi = new ScenesApi(configuration);
            _rulesApi = new RulesApi(configuration);
            _schedulesApi = new SchedulesApi(configuration);
            _appsApi = new AppsApi(configuration);
            _subscriptionsApi = new SubscriptionsApi(configuration);
            _installedAppsApi = new InstalledAppsApi(configuration);
            _deviceProfilesApi = new DeviceProfilesApi(configuration);
        }

        public void ResetData()
        {
            _allDevices = null;
            _allLocations = null;
            _allRooms = null;
            _allScenes = null;
            _allRules = null;
            _allSchedules = null;
            _allApps = null;
            _allSubscriptions = null;
            _allInstalledApps = null;
            _allDeviceProfiles = null;
        }

        public PagedApps GetAllApps()
        {
            if (_allApps == null)
            {
                try
                {
                    _allApps = _appsApi.ListApps();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allApps;
        }

        public PagedInstalledApps GetAllInstalledApps(string locationId = null)
        {
            if (_allInstalledApps == null)
            {
                if (locationId != null)
                {
                    try
                    {
                        _allInstalledApps = _installedAppsApi.ListInstallations(locationId);
                    }
                    catch (Exception exp)
                    {
                        Debug.WriteLine(exp);
                    }
                }
                else
                {
                    _allInstalledApps = new PagedInstalledApps();
                    foreach (var location in GetAllLocations().Items)
                    {
                        try
                        {
                            var locationApps = _installedAppsApi.ListInstallations(location.LocationId.ToString());
                            if (locationApps.Items?.Count > 0)
                            {
                                _allInstalledApps.Items ??= new List<InstalledApp>();
                                _allInstalledApps.Items.AddRange(locationApps.Items);
                            }
                        }
                        catch (Exception exp)
                        {
                            Debug.WriteLine(exp);
                        }
                    }
                }
            }
            return _allInstalledApps;
        }

        public PagedSubscriptions GetAllSubscriptions(string appId = null)
        {
            if (_allSubscriptions == null)
            {
                if (appId != null)
                {
                    try
                    {
                        _allSubscriptions = _subscriptionsApi.ListSubscriptions(appId);
                    }
                    catch (Exception exp)
                    {
                        Debug.WriteLine(exp);
                    }
                }
                else
                {
                    _allSubscriptions = new PagedSubscriptions();
                    foreach (var app in GetAllInstalledApps().Items)
                    {
                        try
                        {
                            var appSubscriptions = _subscriptionsApi.ListSubscriptions(app.InstalledAppId.ToString());
                            if (appSubscriptions.Items?.Count > 0)
                            {
                                _allSubscriptions.Items ??= new List<Subscription>();
                                _allSubscriptions.Items.AddRange(appSubscriptions.Items);
                            }
                        }
                        catch (Exception exp)
                        {
                            Debug.WriteLine(exp);
                        }
                    }
                }
            }
            return _allSubscriptions;
        }

        public async Task<PagedDevices> GetAllDevicesAsync()
        {
            if (_allDevices == null)
            {
                try
                {
                    _allDevices = await _devicesApi.GetDevicesAsync();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allDevices;
        }

        public PagedDevices GetAllDevices()
        {
            if (_allDevices == null)
            {
                try
                {
                    _allDevices = _devicesApi.GetDevices();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allDevices;
        }

        public PagedDeviceProfiles GetAllDeviceProfiles()
        {
            if (_allDeviceProfiles == null)
            {
                try
                {
                    _allDeviceProfiles = _deviceProfilesApi.ListDeviceProfiles();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allDeviceProfiles;
        }

        public async Task<PagedLocations> GetAllLocationsAsync()
        {
            if (_allLocations == null)
            {
                _allLocations = await _locationsApi.ListLocationsAsync();
            }
            return _allLocations;
        }

        public PagedLocations GetAllLocations()
        {
            if (_allLocations == null)
            {
                try
                {
                    _allLocations = _locationsApi.ListLocations();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allLocations;
        }

        public async Task<PagedRooms> GetAllRoomsAsync(string locationId)
        {
            if (_allRooms == null)
            {
                try
                {
                    _allRooms = await _roomsApi.ListRoomsAsync(locationId);
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allRooms;
        }

        public PagedRooms GetAllRooms(string locationId = null)
        {
            if (_allRooms == null)
            {
                if (locationId != null)
                {
                    try
                    {
                        _allRooms = _roomsApi.ListRooms(locationId);
                    }
                    catch (Exception exp)
                    {
                        Debug.WriteLine(exp);
                    }
                }
                else
                {
                    _allRooms = new PagedRooms();
                    foreach (var location in GetAllLocations().Items)
                    {
                        var locationRooms = _roomsApi.ListRooms(location.LocationId.ToString());
                        if (locationRooms.Items?.Count > 0)
                        {
                            _allRooms.Items ??= new List<SmartThingsNet.Model.Room>();
                            _allRooms.Items.AddRange(locationRooms.Items);
                        }
                    }
                }
            }
            return _allRooms;
        }

        public PagedScene GetAllScenes(string locationId = null)
        {
            if (_allScenes == null)
            {
                try
                {
                    _allScenes = _scenesApi.ListScenes(locationId);
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp);
                }
            }
            return _allScenes;
        }

        public PagedRules GetAllRules(string locationId = null)
        {
            if (_allRules == null)
            {
                if (locationId != null)
                {
                    try
                    {
                        _allRules = _rulesApi.ListRules(locationId);
                    }
                    catch (Exception exp)
                    {
                        Debug.WriteLine(exp);
                    }
                }
                else
                {
                    _allRules = new PagedRules();
                    foreach (var location in GetAllLocations().Items)
                    {
                        try
                        {
                            var locationRules = _rulesApi.ListRules(location.LocationId.ToString());
                            if (locationRules.Items?.Count > 0)
                            {
                                _allRules.Items ??= new List<SmartThingsNet.Model.Rule>();
                                _allRules.Items.AddRange(locationRules.Items);
                            }
                        }
                        catch (Exception exp)
                        {
                            Debug.WriteLine(exp);
                        }
                    }
                }
            }
            return _allRules;
        }

        public PagedSchedules GetAllSchedules(string appId = null)
        {
            if (_allSchedules == null)
            {
                if (appId != null)
                {
                    try
                    {
                        _allSchedules = _schedulesApi.GetSchedules(appId);
                    }
                    catch (Exception exp)
                    {
                        Debug.WriteLine(exp);
                    }
                }
                else
                {
                    _allSchedules = new PagedSchedules();
                    foreach (var app in GetAllInstalledApps().Items)
                    {
                        try
                        {
                            var appSchedules = _schedulesApi.GetSchedules(app.InstalledAppId.ToString());
                            if (appSchedules.Items?.Count > 0)
                            {
                                _allSchedules.Items ??= new List<Schedule>();
                                _allSchedules.Items.AddRange(appSchedules.Items);
                            }
                        }
                        catch (Exception exp)
                        {
                            Debug.WriteLine(exp);
                        }
                    }
                }
            }
            return _allSchedules;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _allDevices = null;
                    _allLocations = null;
                    _allRooms = null;
                    _allScenes = null;
                    _allRules = null;
                    _allSchedules = null;
                    _allApps = null;
                    _allSubscriptions = null; 
                    _allInstalledApps = null;
                    _allDeviceProfiles = null;

                    _devicesApi = null;
                    _locationsApi = null;
                    _roomsApi = null;
                    _scenesApi = null;
                    _rulesApi = null;
                    _schedulesApi = null;
                    _appsApi = null;
                    _subscriptionsApi = null;
                    _installedAppsApi = null;
                    _deviceProfilesApi = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
