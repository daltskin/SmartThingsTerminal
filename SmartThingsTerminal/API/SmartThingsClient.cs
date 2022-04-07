using SmartThingsNet.Api;
using SmartThingsNet.Client;
using SmartThingsNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartThingsTerminal
{
    public class SmartThingsClient : IDisposable
    {
        private bool _disposedValue;

        private PagedDevices _allDevices;
        private PagedLocations _allLocations;
        private PagedRooms _allRooms;
        private ScenePagedResult _allScenes;
        private PagedRules _allRules;
        private PagedSchedules _allSchedules;
        private PagedApps _allApps;
        private PagedSubscriptions _allSubscriptions;
        private PagedInstalledApps _allInstalledApps;
        private PagedDeviceProfiles _allDeviceProfiles;
        private PagedCapabilities _allCapabilities;

        private DevicesApi _devicesApi;
        private LocationsApi _locationsApi;
        private RoomsApi _roomsApi;
        private ScenesApi _scenesApi;
        private RulesApi _rulesApi;
        private SchedulesApi _schedulesApi;
        private AppsApi _appsApi;
        private SubscriptionsApi _subscriptionsApi;
        private InstalledappsApi _installedAppsApi;
        private ProfilesApi _deviceProfilesApi;
        private CapabilitiesApi _capabilitiesApi;
        private PresentationsApi _presentationApi;
        private string _accessToken;

        public SmartThingsClient(string accessToken)
        {
            var configuration = new Configuration();
            configuration.AccessToken = accessToken ?? throw new ArgumentNullException(accessToken);
            _accessToken = accessToken;

            _devicesApi = new DevicesApi(configuration);
            _locationsApi = new LocationsApi(configuration);
            _roomsApi = new RoomsApi(configuration);
            _scenesApi = new ScenesApi(configuration);
            _rulesApi = new RulesApi(configuration);
            _schedulesApi = new SchedulesApi(configuration);
            _appsApi = new AppsApi(configuration);
            _subscriptionsApi = new SubscriptionsApi(configuration);
            _installedAppsApi = new InstalledappsApi(configuration);
            _deviceProfilesApi = new ProfilesApi(configuration);
            _capabilitiesApi = new CapabilitiesApi(configuration);
            _presentationApi = new PresentationsApi(configuration);

            //_accessToken = accessToken;
            //_devicesApi = new DevicesApi();
            //_locationsApi = new LocationsApi();
            //_roomsApi = new RoomsApi();
            //_scenesApi = new ScenesApi();
            //_rulesApi = new RulesApi();
            //_schedulesApi = new SchedulesApi();
            //_appsApi = new AppsApi();
            //_subscriptionsApi = new SubscriptionsApi();
            //_installedAppsApi = new InstalledappsApi();
            //_deviceProfilesApi = new ProfilesApi();
            //_capabilitiesApi = new CapabilitiesApi();
            //_presentationApi = new PresentationsApi();
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
                _allApps = _appsApi.ListApps(_accessToken);
            }
            return _allApps;
        }

        public PagedInstalledApps GetAllInstalledApps(string locationId = null)
        {
            if (_allInstalledApps == null)
            {
                if (locationId != null)
                {
                    _allInstalledApps = _installedAppsApi.ListInstallations(_accessToken, locationId);
                }
                else
                {
                    _allInstalledApps = new PagedInstalledApps();
                    var locations = GetAllLocations();
                    if (locations?.Items != null)
                    {
                        foreach (var location in locations.Items)
                        {
                            var locationApps = _installedAppsApi.ListInstallations(_accessToken, location.LocationId.ToString());
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
                    _allSubscriptions = _subscriptionsApi.ListSubscriptions(appId, _accessToken);
                }
                else
                {
                    _allSubscriptions = new PagedSubscriptions();
                    var apps = GetAllInstalledApps();
                    if (apps?.Items != null)
                    {
                        foreach (var app in apps.Items)
                        {
                            var appSubscriptions = _subscriptionsApi.ListSubscriptions(app.InstalledAppId.ToString(), _accessToken);
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
                _allDevices = await _devicesApi.GetDevicesAsync(_accessToken);
            }
            return _allDevices;
        }

        public PagedDevices GetAllDevices()
        {
            if (_allDevices == null)
            {
                _allDevices = _devicesApi.GetDevices(_accessToken);
            }
            return _allDevices;
        }

        public PagedDeviceProfiles GetAllDeviceProfiles()
        {
            if (_allDeviceProfiles == null)
            {
                _allDeviceProfiles = _deviceProfilesApi.ListDeviceProfiles(_accessToken);
            }
            return _allDeviceProfiles;
        }

        public async Task<PagedLocations> GetAllLocationsAsync()
        {
            if (_allLocations == null)
            {
                _allLocations = await _locationsApi.ListLocationsAsync(_accessToken);
            }
            return _allLocations;
        }

        public PagedLocations GetAllLocations()
        {
            if (_allLocations == null)
            {
                _allLocations = _locationsApi.ListLocations(_accessToken);
            }
            return _allLocations;
        }

        public Location GetLocationDetails(string locationId)
        {
            return _locationsApi.GetLocation(_accessToken, locationId);
        }

        public async Task<PagedRooms> GetAllRoomsAsync(string locationId)
        {
            if (_allRooms == null)
            {
                _allRooms = await _roomsApi.ListRoomsAsync(_accessToken, locationId);
            }
            return _allRooms;
        }

        public PagedRooms GetAllRooms(string locationId = null)
        {
            if (_allRooms == null)
            {
                if (locationId != null)
                {
                    _allRooms = _roomsApi.ListRooms(_accessToken, locationId);
                }
                else
                {
                    _allRooms = new PagedRooms();
                    var locations = GetAllLocations();
                    if (locations!.Items != null)
                    {
                        foreach (var location in locations.Items)
                        {
                            var locationRooms = _roomsApi.ListRooms(_accessToken, location.LocationId.ToString());
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

        public ScenePagedResult GetAllScenes(string locationId = null)
        {
            if (_allScenes == null)
            {
                _allScenes = _scenesApi.ListScenes("", locationId);
            }
            return _allScenes;
        }

        public PagedRules GetAllRules(string locationId = null)
        {
            if (_allRules == null)
            {
                if (locationId != null)
                {
                    _allRules = _rulesApi.ListRules(_accessToken, locationId);
                }
                else
                {
                    _allRules = new PagedRules();
                    var locations = GetAllLocations();
                    if (locations!.Items != null)
                    {
                        foreach (var location in locations.Items)
                        {
                            var locationRules = _rulesApi.ListRules(_accessToken, location.LocationId.ToString());
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
                    _allSchedules = _schedulesApi.GetSchedules(appId, _accessToken);
                }
                else
                {
                    _allSchedules = new PagedSchedules();
                    var apps = GetAllInstalledApps();
                    if (apps?.Items != null)
                    {
                        foreach (var app in apps.Items)
                        {
                            var appSchedules = _schedulesApi.GetSchedules(app.InstalledAppId.ToString(), _accessToken);
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

        public PagedCapabilities GetAllCapabilities()
        {
            if (_allCapabilities == null)
            {
                _allCapabilities = _capabilitiesApi.ListCapabilities(_accessToken);
            }
            return _allCapabilities;
        }

        public Capability GetCapability(string capabilityId, int capabilityVersion)
        {
            return _capabilitiesApi.GetCapability(_accessToken, capabilityId, capabilityVersion);
        }

        public Capability CreateCapability(CreateCapabilityRequest capabilityRequest)
        {
            return _capabilitiesApi.CreateCapability(_accessToken, capabilityRequest);
        }

        public Capability UpdateCapability(string capabilityId, int capabilityVersion, UpdateCapabilityRequest updateCapabilityRequest)
        {
            return _capabilitiesApi.UpdateCapability(_accessToken, capabilityId, capabilityVersion, updateCapabilityRequest);
        }

        public object DeleteCapability(string capabilityId, int capabilityVersion)
        {
            return _capabilitiesApi.DeleteCapability(_accessToken, capabilityId, capabilityVersion);
        }

        public PublicDeviceConfiguration GetDeviceConfiguration(string presentationId)
        {
            if (presentationId == null)
            {
                return _presentationApi.GetDeviceConfiguration(_accessToken, presentationId);
            }
            return null;
        }

        public DossierDevicePresentation GetDevicePresentation(string presentationId)
        {
            if (presentationId == null)
            {
                return _presentationApi.GetDevicePresentation(_accessToken, presentationId);
            }
            return null;
        }

        public object ExecuteDevicecommand(string deviceId, DeviceCommandsRequest commandRequest)
        {
            return _devicesApi.ExecuteDeviceCommands(_accessToken, deviceId, commandRequest);
        }

        public bool TryGetDeviceCapabilityStatus(string deviceId, string componentId, string capabilityId, out Dictionary<string, AttributeState> capabilityStatus)
        {
            capabilityStatus = new Dictionary<string, AttributeState>();

            try
            {
                capabilityStatus = GetDeviceCapabilityStatus(deviceId, componentId, capabilityId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, AttributeState> GetDeviceCapabilityStatus(string deviceId, string componentId, string capabilityId)
        {
            return _devicesApi.GetDeviceStatusByCapability(_accessToken, deviceId, componentId, capabilityId);
        }

        public StandardSuccessResponse RunScene(string sceneId)
        {
            return _scenesApi.ExecuteScene(_accessToken, sceneId);
        }

        public Device UpdateDevice(string deviceId, UpdateDeviceRequest updateDeviceRequest)
        {
            return _devicesApi.UpdateDevice(_accessToken, deviceId, updateDeviceRequest);
        }

        public object DeleteDevice(string deviceId)
        {
            return _devicesApi.DeleteDevice(_accessToken, deviceId);
        }

        public Rule UpdateRule(string ruleId, string locationId, RuleRequest ruleRequest)
        {
            return _rulesApi.UpdateRule(_accessToken, ruleId, locationId, ruleRequest);
        }

        public object CreateRule(string locationId, RuleRequest ruleRequest)
        {
            return _rulesApi.CreateRule(_accessToken, locationId, ruleRequest);
        }

        public Rule DeleteRule(string ruleId, string locationId)
        {
            return _rulesApi.DeleteRule(_accessToken, ruleId, locationId);
        }

        public UpdateLocationResponse UpdateLocation(string locationId, UpdateLocationRequest locationRequest)
        {
            return _locationsApi.UpdateLocation(_accessToken, locationId, locationRequest);
        }

        public CreateLocationResponse CreateLocation(CreateLocationRequest locationRequest)
        {
            return _locationsApi.CreateLocation(_accessToken, locationRequest);
        }

        public object DeleteLocation(string locationId)
        {
            return _locationsApi.DeleteLocation(_accessToken, locationId);
        }

        public Room UpdateRoom(string locationId, string roomId, UpdateRoomRequest roomRequest)
        {
            return _roomsApi.UpdateRoom(_accessToken, locationId, roomId, roomRequest);
        }

        public Room CreateRoom(string locationId, CreateRoomRequest roomRequest)
        {
            return _roomsApi.CreateRoom(_accessToken, locationId, roomRequest);
        }

        public object DeleteRoom(string locationId, string roomId)
        {
            return _roomsApi.DeleteRoom(_accessToken, locationId, roomId);
        }

        public App UpdateApp(string appName, UpdateAppRequest updateAppRequest)
        {
            return _appsApi.UpdateApp(_accessToken, appName,updateAppRequest);
        }

        public Schedule CreateSchedule(string installedAppId, ScheduleRequest scheduleRequest)
        {
            return _schedulesApi.CreateSchedule(installedAppId, _accessToken, scheduleRequest);
        }

        public object DeleteSchedule(string installedAppId, string scheduleName)
        {
            return _schedulesApi.DeleteSchedule(installedAppId, scheduleName, _accessToken);
        }

        public Subscription SaveSubscription(string installedAppId, SubscriptionRequest subscriptionRequest)
        {
            return _subscriptionsApi.SaveSubscription(installedAppId, _accessToken, subscriptionRequest);
        }

        public SubscriptionDelete DeleteSubscription(string installedAppId, string subscriptionId)
        {
            return _subscriptionsApi.DeleteSubscription(installedAppId, subscriptionId, _accessToken);
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
