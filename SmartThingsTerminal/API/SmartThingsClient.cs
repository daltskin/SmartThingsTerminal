using SmartThingsNet.Api;
using SmartThingsNet.Client;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
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
                    var locations = GetAllLocations();
                    if (locations?.Items != null)
                    {
                        foreach (var location in locations.Items)
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
                    var apps = GetAllInstalledApps();
                    if (apps?.Items != null)
                    {
                        foreach (var app in apps.Items)
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

        public PagedRooms GetAllRooms(string locationId = null)
        {
            if (_allRooms == null)
            {
                if (locationId != null)
                {
                    _allRooms = _roomsApi.ListRooms(locationId);
                }
                else
                {
                    _allRooms = new PagedRooms();
                    var locations = GetAllLocations();
                    if (locations!.Items != null)
                    {
                        foreach (var location in locations.Items)
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
                    var locations = GetAllLocations();
                    if (locations!.Items != null)
                    {
                        foreach (var location in locations.Items)
                        {
                            var locationRules = _rulesApi.ListRules(location.LocationId.ToString());
                            if (locationRules.Items?.Count > 0)
                            {
                                _allRules.Items ??= new List<Rule>();
                                _allRules.Items.AddRange(locationRules.Items);
                            }
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
                    var apps = GetAllInstalledApps();
                    if (apps?.Items != null)
                    {
                        foreach (var app in apps.Items)
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
            }
            return _allSchedules;
        }

        public object ExecuteDevicecommand(string deviceId, DeviceCommandsRequest commandRequest)
        {
            return _devicesApi.ExecuteDeviceCommands(deviceId, commandRequest);
        }

        public Dictionary<string, AttributeState> GetDeviceCapabilityStatus(string deviceId, string componentId, string capabilityId)
        {
            return _devicesApi.GetDeviceStatusByCapability(deviceId, componentId, capabilityId);
        }

        public StandardSuccessResponse RunScene(string sceneId)
        {
            return _scenesApi.ExecuteScene(sceneId);
        }

        public Rule UpdateRule(string ruleId, string locationId, RuleRequest ruleRequest)
        {
            return _rulesApi.UpdateRule(ruleId, locationId, ruleRequest);
        }

        public void CreateRule(string ruleId, RuleRequest ruleRequest)
        {
            var retVal = _rulesApi.CreateRule(ruleId, ruleRequest);
        }

        public Rule DeleteRule(string ruleId, string locationId)
        {
            return _rulesApi.DeleteRule(ruleId, locationId);
        }

        public Location UpdateLocation(string locationId, UpdateLocationRequest locationRequest)
        {
            return _locationsApi.UpdateLocation(locationId, locationRequest);
        }

        public Room UpdateRoom(string locationId, string roomId, UpdateRoomRequest roomRequest)
        {
            return _roomsApi.UpdateRoom(locationId, roomId, roomRequest);
        }

        public App UpdateApp(string appName, UpdateAppRequest updateAppRequest)
        {
            return _appsApi.UpdateApp(appName,updateAppRequest);
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
