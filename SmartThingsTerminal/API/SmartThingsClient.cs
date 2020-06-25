using SmartThingsNet.Api;
using SmartThingsNet.Client;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartThingsTerminal
{
    public class SmartThingsClient
    {
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
            if (accessToken == null)
            {
                throw new ArgumentNullException(accessToken);
            }

            var configuration = new Configuration();
            configuration.AccessToken = accessToken;

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
                _allApps = _appsApi.ListApps();
            }
            return _allApps;
        }

        public PagedInstalledApps GetAllInstalledApps(string locationId = null)
        {
            if (_allInstalledApps == null)
            {
                if (locationId != null)
                {
                    _allInstalledApps = _installedAppsApi.ListInstallations(locationId);
                }
                else
                {
                    _allInstalledApps = new PagedInstalledApps();
                    foreach (var location in GetAllLocations().Items)
                    {
                        var locationApps = _installedAppsApi.ListInstallations(location.LocationId.ToString());
                        if (locationApps.Items?.Count > 0)
                        {
                            _allInstalledApps.Items ??= new List<InstalledApp>();
                            _allInstalledApps.Items.AddRange(locationApps.Items);
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
                    _allSubscriptions = _subscriptionsApi.ListSubscriptions(appId);
                }
                else
                {
                    _allSubscriptions = new PagedSubscriptions();
                    foreach (var app in GetAllInstalledApps().Items)
                    {
                        var appSubscriptions = _subscriptionsApi.ListSubscriptions(app.InstalledAppId.ToString());
                        if (appSubscriptions.Items?.Count > 0)
                        {
                            _allSubscriptions.Items ??= new List<Subscription>();
                            _allSubscriptions.Items.AddRange(appSubscriptions.Items);
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
                _allDevices = await _devicesApi.GetDevicesAsync();
            }
            return _allDevices;
        }

        public PagedDevices GetAllDevices()
        {
            if (_allDevices == null)
            {
                _allDevices = _devicesApi.GetDevices();
            }
            return _allDevices;
        }

        public PagedDeviceProfiles GetAllDeviceProfiles()
        {
            if (_allDeviceProfiles == null)
            {
                _allDeviceProfiles = _deviceProfilesApi.ListDeviceProfiles();
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
                _allLocations = _locationsApi.ListLocations();
            }
            return _allLocations;
        }

        public async Task<PagedRooms> GetAllRoomsAsync(string locationId)
        {
            if (_allRooms == null)
            {
                _allRooms = await _roomsApi.ListRoomsAsync(locationId);
            }
            return _allRooms;
        }

        public PagedRooms GetAllRooms(string locationId)
        {
            if (_allRooms == null)
            {
                _allRooms = _roomsApi.ListRooms(locationId);
            }
            return _allRooms;
        }

        public PagedScene GetAllScenes(string locationId = null)
        {
            if (_allScenes == null)
            {
                _allScenes = _scenesApi.ListScenes(locationId);
            }
            return _allScenes;
        }

        public PagedRules GetAllRules(string locationId = null)
        {
            if (_allRules == null)
            {
                if (locationId != null)
                {
                    _allRules = _rulesApi.ListRules(locationId);
                }
                else
                {
                    _allRules = new PagedRules();
                    foreach (var location in GetAllLocations().Items)
                    {
                        var locationRules = _rulesApi.ListRules(location.LocationId.ToString());
                        if (locationRules.Items?.Count > 0)
                        {
                            _allRules.Items ??= new List<SmartThingsNet.Model.Rule>();
                            _allRules.Items.AddRange(locationRules.Items);
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
                    _allSchedules = _schedulesApi.GetSchedules(appId);
                }
                else
                {
                    _allSchedules = new PagedSchedules();
                    foreach (var app in GetAllInstalledApps().Items)
                    {
                        var appSchedules = _schedulesApi.GetSchedules(app.InstalledAppId.ToString());
                        if (appSchedules.Items?.Count > 0)
                        {
                            _allSchedules.Items ??= new List<Schedule>();
                            _allSchedules.Items.AddRange(appSchedules.Items);
                        }
                    }
                }
            }
            return _allSchedules;
        }
    }
}
