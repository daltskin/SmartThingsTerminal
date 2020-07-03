# SmartThings Terminal

![.NET Core](https://github.com/daltskin/SmartThingsTerminal/workflows/.NET%20Core/badge.svg)
![Release](https://img.shields.io/github/v/release/daltskin/SmartThingsTerminal?include_prereleases)

![screenshot](docs/home.png)

# Overview

SmartThings Terminal is a cross platform CLI tool for the SmartThings API, providing a simple GUI terminal for navigating devices and settings within your SmartThings environment.  The functionality of this CLI is driven by the supporting [SmartThingsNet](https://github.com/daltskin/SmartThingsNet) dotnet sdk, which uses the underlying [SmartThings API](https://smartthings.developer.samsung.com/docs/api-ref/st-api.html). This tool is aimed at the SmartThings developer ecosystem to get quick insight into their smart home environments.

# Features

![screenshot](docs/devices.png)

The current version is limited to read-only access to devices and configuration settings within SmartThings, with the ability to run scenes.  This tool provides insights into what goes on under the hood of SmartThings such as:

* View Devices - view all of the devices registered
* Install Applications - view details of any additional installed applications
* Locations - view the different locations you have configured
* Rooms - view the different rooms you have configured in all locations
* Rules - view automations
* Scenes - view the details your scenes and run them
* Schedules - view integration schedules
* Subscriptions - view integration notification events
* F5 Refresh - refresh data at any point

# Usage

Either clone the repo, or simply download the latest executable binaries version for your operating system from [releases](https://github.com/daltskin/SmartThingsTerminal/releases)

Simply run from your command prompt providing your access token.  If you don't have an access token, it's easy to set one up here: https://account.smartthings.com/tokens

![screenshot](docs/pat.png)

## Command line

Using the -t (or --accesstoken) argument, provide your personal access token from the above into the STT  tool eg:

```bash
$ ./STT -t {accesstoken}
```


Alternatively, you can jump straight into a specific screen by providing the screen name using the -s (or --screen) argument using one of the following values:

| devices | installedapps | locations | rules | scenes | schedules | subscriptions |

eg:

```bash
$ ./STT -t {accesstoken} -s devices
```


# Futures

A future version may provide the ability to update configuration.

# Credits

This uses [Miguel de Icaza's](https://github.com/migueldeicaza) [console-based user inteface toolkit](https://github.com/migueldeicaza/gui.cs). Which, at the time of writing, is undergoing a substantial overhaul - hence the local project reference.

This also uses the [CommandLineParser](https://github.com/commandlineparser/commandline) project for manipulating command line arguments.