#Martridge - The DMOD Wizard
## What is this?
Ever heard of Dink Smallwood? Well, now you have! If you have no idea what I'm talking about, just head over to [The Dink Network](https://www.dinknetwork.com/forum.cgi) for more info about Dink!

Anyway, this Dink game has a ton of community content in the form of custom stories called DMODs. 

This application is a DMOD manager/launcher for Dink Smallwood.

## What can it do?
- Runs on Windows and Linux using .NET [1]
- Automagically install a number of versions of the Dink engine on Windows
- Select from multiple user configurable Dink engine versions to launch a DMOD with [2]
- Detect DMODs already installed in multiple user configurable locations
- Is localizable using .json files [3] 
- Supports DMOD localization, only when using FreeDink
- Features an online DMOD browser that parses HTML data from the Dink Network, so you can browse and install dmods with just a click or two

### Notes
1. The application uses [AvaloniaUI](https://avaloniaui.net/) for its GUI Framework


2. The following versions can be installed under windows:
- FreeDink - considered the best way to play Dink by the community
- DinkHD - the current official release of Dink, available on multiple platforms like Android or MacOS but unfortunately not on Linux systems
- The original Dink Smallwood V1.08 - the decade old original release, still runs under Windows, although you probably don't wnat to use this anymore...

3. Application Localization is WIP - Technically works but only english is available right now, I need to add more info about how to define the localization files using a template later...

## Building
Assuming that you're somewhat familiar with dotnet applications...
1. Clone this git
2. Restore submodules
```git submodule update --init --recursive```
3. Publish the project to folder using dotnet

Here are my dotnet publish commandline options:
- Windows x64 using NetCore 3.1 ```dotnet publish ./martridge/src/martridge.csproj --output ./publish/netcoreapp3.1_win-x64/martridge/ --configuration Release --framework netcoreapp3.1 --self-contained true --runtime win-x64 -p:PublishSingleFile=true```
- Windows x86 using NetCore 3.1 ```dotnet publish ./martridge/src/martridge.csproj --output ./publish/netcoreapp3.1_win-x86/martridge/ --configuration Release --framework netcoreapp3.1 --self-contained true --runtime win-x86 -p:PublishSingleFile=true```
- Windows x64 using Net 6.0 ```dotnet publish ./martridge/src/martridge.csproj --output ./publish/net6.0_win-x64/martridge/ --configuration Release --framework net6.0 --self-contained true --runtime win-x64 -p:PublishSingleFile=true```
- Linux x64 using Net 6.0 ```dotnet publish ./martridge/src/martridge.csproj --output ./publish/net6.0_linux-x64/martridge/ --configuration Release --framework net6.0 --self-contained true --runtime linux-x64 -p:PublishSingleFile=true```

If you are using JetBrains Rider, you should be able to use the run profiles included in the source.
If you are using Visual Studio, you should be able to create your own publish profile based on the run profiles and the information above.

## Overview

### First time?
When the application launches for the first time, it should look something like this:
![](doc/images/screenshot_nothinginstalled.png)

If you are on Windows, you can click Install Dink to be taken to the auto-magical Dink installer interface. (Also accessible via File-> Install Dink!) It should be pretty straight forward...

Note that under linux the installer will tell you to manually install FreeDink instead. Normally, the application should detect the default FreeDink installation if you already have it installed under certain distributions like Ubuntu for example.

### Dink Installer (Windows)
The dink installer will first ask you to select which version you want to install:
![](doc/images/screenshot_dinkinstall_01.png)

Afterwards, you will see a detailed description of the files involved in the installation, and of course choose your install destination:
![](doc/images/screenshot_dinkinstall_02.png)

Important note: The automated-installer can be scripted to some extent using the file located in the subfolder "config/configInstallerList.json", but more on this later...


If you try to install over existing files you will be given this warning:
![](doc/images/screenshot_dinkinstall_warning.png)
WARNING: Choosing YES will delete everything in the destination directory!

The installer will do its thing and have a log of the actions it's taking and any possible errors that might pop up (hopefully not, hah)
![](doc/images/screenshot_dinkinstall_03.png)

### Settings
The settings interface looks like this:
![](doc/images/screenshot_settings.png)

These can also be manually edited in "config/config.json"

- Show App Log Window on startup: exactly as it says, the log window should display any errors or warnings that occur, this is mostly for debugging purposes
- Show advanced features: (WIP) this currently doesn't do anything
- Try to use relative paths in config files: this will store all subfolders in the config.json file as relative to the application root, helps if you want the application to be portable to another computer
- Application Language: self explanatory, however only english is available right now (WIP)
- Default DMOD location: this is the main location where the application should install dmods (although it lets you choose others during DMOD installation too...)
- Additional DMOD locations: the application can look in these directories for installed dmods
- Game locations: Dink engine executables used to launch the game, you can even have different versions of the same engine if you so desire

### DMOD Browser
The DMOD browser looks like this:
![](doc/images/screenshot_dmodbrowser_ex01.png)
On the left you have a list of DMODS that were found. On the right you have the currently selected DMOD info. In the top left above the list, you can hit the refresh button to scan the directories for DMODS again or use the Search input box to filter the DMODS if you have too many.

In the DMOD info, you can select which Game Engine to use to launch the game (if you have more than one configured in Settings).
You have the usual launch options as in DFArc, and below the launch button you have the FreeDink localization selection. Obviously, this only works if the selected DMOD has localization files in the first place, and you are using FreeDink.

### Online DMOD Browser
The online DMOD browser shows you all kinds of DMOD related info straight from [The Dink Network](https://www.dinknetwork.com/files/category_dmod/sort_title-asc/page_1/)
NOTE: Currently data is not automatically refreshed, and it is only downloaded if missing or you hit the Refresh button. To refresh existing DMOD data you have to right click in the DMOD info area on the right and select "Reload DMOD Online Data".
Online data gets cached in the local subfolder "webcache".

You can view DMOD reviews, screenshots and all the released versions straight from the application. NOTE that this is NOT a browser renderer, but rather parses the HTML data and renders it using native AvaloniaUI controls.
![](doc/images/screenshot_dmodonline_ex02.png)
![](doc/images/screenshot_dmodonline_ex01.png)


## TODO...
The application is mostly complete and functional, but there are still things left to do:
- Bug hunting and fixing
- User suggestions
- GUI improvements?
- Make online DMOD browser automagically refresh without spamming the Dink Network website
- Figure out a way to automagically install FeeDink under Linux
- Build and test how this runs under MacOS (Currently don't have a macbook to do this though)
- Figure out commandline dotnet stuff so I can explain how you build this on linux yourself (I'm using JetBrains Rider myself right now)
- Figure out GitHub actions and releases and see if I can automate building/publishing the application?...