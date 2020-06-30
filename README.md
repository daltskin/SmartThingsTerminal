# SmartThings Terminal

![.NET Core](https://github.com/daltskin/SmartThingsTerminal/workflows/.NET%20Core/badge.svg)
![Release](https://img.shields.io/github/v/release/daltskin/SmartThingsTerminal?include_prereleases)

![screenshot](docs/home.png)

# Overview

SmartThings Terminal is a cross platform CLI tool for the SmartThings API, providing a simple GUI terminal for navigating devices and settings within your SmartThings environment.  The functionality of this CLI is driven by the supporting [SmartThingsNet](https://github.com/daltskin/SmartThingsNet) dotnet sdk, which uses the underlying [SmartThings API](https://smartthings.developer.samsung.com/docs/api-ref/st-api.html). 

# Features

![screenshot](docs/devices.png)

The current version is limited to read-only access to devices and configuration settings within SmartThings.  This tool provides insights into what goes on under the hood of SmartThings such as:

* View Devices - view all of the devices registered
* Install Applications - view details of any additional installed applications
* Locations - view the different locations you have configured
* Rooms - view the different rooms you have configured in all locations
* Rules - view automations
* Scenes - view the details of all your scenes
* Schedules - view integration schedules
* Subscriptions - view integration notification events

# Usage

Simply run from your command prompt providing your access token.  If you don't have an access token, it's easy to set one up here: https://account.smartthings.com/tokens

![screenshot](docs/pat.png)

```bash
./STT -t {GUID}
```

Or you can jump straight into a specific screen by providing the screen name: 

| devices | installedapps | locations | rules | scenes | schedules | subscriptions |

example:

```bash
./STT -t {GUID} -s devices
```


# Futures

A future version may provide the ability to execute commands such as run scenes, turn on/off devices etc.