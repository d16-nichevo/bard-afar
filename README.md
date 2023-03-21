# Bard Afar

An audio streaming solution, intended for music ambience for online tabletop RPG sessions, but possibly usable for a number of applications.

* **The server** runs Bard Afar, a Windows application, and plays files from the local filesystem.
* **Clients** connect with modern web browsers (Chrome, Firefox, etc) from any device (PC, tablet, phone, etc), any operating system (Windows, Linux, etc), across a LAN or even the internet.

## Screenshots: Host

![image](https://user-images.githubusercontent.com/44771168/226324058-2395ce9a-0d47-480f-8f38-ff96b3606260.png)

![image](https://user-images.githubusercontent.com/44771168/226340459-8a68653f-d1e0-4058-a564-18f5586f6991.png)

## Screenshots: Client

![image](https://user-images.githubusercontent.com/44771168/226339516-ea85c449-bc64-4c4c-a678-c8c6b2e77e78.png)

## Raison D'etre

In 2021, YouTube started [taking down](https://www.theverge.com/2021/9/12/22669502/youtube-discord-rythm-music-bot-closure) a variety of Discord bots that were able to play audio from Discord. These bots were the go-to way for many online [tabletop RPG](https://en.wikipedia.org/wiki/Tabletop_role-playing_game) groups to add music to their gaming sessions, given that Discord was commonly used for voice chat anyway.

Nowadays, some [Virtual Tabletops](https://en.wikipedia.org/wiki/Digital_tabletop_game#Virtual_tabletops) have ways to play music. Some do it better than others, and not everyone uses a Virtual Tabletop for online play.

I thought it would be trivially easy to find easy-to-use live-audio-sharing software. I couldn't find anything. So I made Bard Afar. Bard Afar ***does not*** do what the old Discord bots does: it doesn't play YouTube videos. Instead it plays audio files on your computer. But that's good enough for me.

(Maybe my search-fu is weak and there are are simple live-audio-sharing programs out there. If I missed some, oh well, now Bard Afar is another!)

# ⚠️ Important Notes ⚠️

Please read and understand the following before use.

1. This software is new, and hasn't been extensively tested yet.
2. This software exposes files on your hard drive to the internet. It could share personal documents or other private files if:
   1. you mis-configure the software, or
   2. the software has bugs.
3. There are some technical steps involved in setting up a Bard Afar server. Common issues are [explained below](#troubleshooting). But I cannot predict every possible issue for every possible PC setup.
4. ***The software is provided "as is", and you use the software at your own risk.***

# Using the Bard Afar Server

## Installation

1. View the [releases page](https://github.com/d16-nichevo/bard-afar/releases) for this project.
2. Find the latest release.
3. Click to download the installer `BardAfar-Installer.exe`.
4. Run that file.

## Server Setup

1. Run Bard Afar from your Start Menu (or any other way you prefer).   
1. You are presented with some options (see below).
1. Hit the "Start Server" button.

![image](https://user-images.githubusercontent.com/44771168/226324058-2395ce9a-0d47-480f-8f38-ff96b3606260.png)

### Host or IP Address

The host the server listens on. Must be a fully qualified domain name, an IPv4 or IPv6 literal string, or a wildcard. More information [here](https://learn.microsoft.com/en-gb/windows/win32/http/urlprefix-strings).

Generally it's easiest to use the special wildcard character `*`, which is a "catch-all" option. 

### Port (HTTP) 

The port used for the HTTP server. The default is `58470`.

### Port (WebSocket)

The port used for the websocket server. The default is `58471`.

### Audio Files Directory

The full path to a directory containing the audio/music files you wish to play to clients.

* This directory can contain any number of sub-directories. You will be able to navigate freely between the contents of the specified directory and all sub-directories when selecting files to play.
* Note that all files in this directory and its sub-directories will be accessible by clients.
  * Files that you play are trivially easy to download (indeed, [it's a client feature](#using-the-bard-afar-client) to do this).
  * Files that you don't play are technically accessible, but the client would have to guess the relative path and filename for the file.
  * It is recommended that the directory you select for this option contains ***only*** music you want to play to clients.

### Silence Between Tracks (Seconds)

Sometimes a client can be delayed in playing a track. This can happen because of network latency or limited bandwidth. In any case, it can cause the audio on the client end to end prematurely when the server moves onto the next track. This can be allievated by adding some "padding" to the end of each track. This setting controls how many seconds of silent "padding" is added to the end of each track.

Generally a value of around `10` seconds is recommended.

## Using the Player

1. After you have [chosen server settings](#server-setup), Bard Afar will start as a server. The player interface will be shown.
2. At this time, you can have clients connect to your server.
   * Clients will need to know an address to connect to. See [here](#server-address-for-clients) for more information.
3. Close the window when you are done. This will stop playback and close the server.

![image](https://user-images.githubusercontent.com/44771168/226340497-2e9bd57d-585e-4605-bec6-26f7890223bf.png)

Player notes:

* Any connected client will hear what you play.
* Hover over a button or control to see a tool-tip giving the button's purpose.
* To navigate between sub-directories, click the directory names.
  * Click on `..` at the top of the file list to move to the parent directory. You cannot move to the parent of the [Audio Files Directory](#audio-files-directory) as configured above.

# Using the Bard Afar Client

1. Receive an address from the person running the server. More information [here](#server-address-for-clients).
2. Open a modern-day web browser (such as Chrome or Firefox).
   * Bard Afar should work on modern-day browsers on ***any device***. PCs, phones, tablets, etc.
4. Enter the address from step 1 into the address bar.
5. You should see the Bard Afar page.<br/>
   ![image](https://user-images.githubusercontent.com/44771168/226333277-cd5cc942-69c4-4e6c-8969-a5448d67de8d.png)
5. Click the "Click to Connect" button.
6. The LED indicator should turn green.<br/>
   ![image](https://user-images.githubusercontent.com/44771168/226334098-166079f3-6091-49d8-bd9b-69e515d69009.png)
8. You are now able to hear music from the server.
   * You can't hear a track that is in-progress. Wait for the next track to start.
9. You can adjust the volume with the slider.
10. You can download the currently-playing track by clicking the floppy disk icon 💾.
11. You can safely close the browser, or the browser tab, at any time to stop using Bard Afar.

# Troubleshooting

## Known Issues

1. Some audio files are not playable by the server, some are not playable by clients, and some are not playable by either.
1. Clients will not play the track that is currently playing at time of connection. They will start playing the next song.

## First Things to Try

### Server: Run as Administrator

If the server is having issues, try running Bard Afar as an administrator. Strictly speaking this shouldn't be necessary, but it can solve a number of potential issues.

Right-click on the Bard Afar icon in your Start Menu and select "Run as Administrator".

### Client: Refresh the Page

If a client is having issues, simply refresh the page and reconnect.

## Network Issues

### Port Forwarding

In order for your server to be accessible over the internet, you'll almost certainly need to do something called "port forwarding".

Explaining port forwarding is out-of-scope for this documentation. [This](https://www.howtogeek.com/66214/how-to-forward-ports-on-your-router/) is a good guide relevant to most home setups. More complex setups -- as seen in larger businesses, schools, or some apartments -- may require you to contact IT support.

The ports you need to forward are below. Both use TCP.

1. The port you set for the HTTP server, as per [here](#port-http). The default is `58470`.
1. The port you set for the websocket server, as per [here](#port-websocket). The default is `58471`.

### Firewall

Firewalls may block connection to Bard AFar, mostly on the computer running the server.

The [installer](#installation) automatically adds firewall rules to allow incoming traffic on ports `58470` and `58471`. (These rules are removed as a part of uninstallation.) This should allow Bard Afar to work on typical home PC setups.

If you want to create firewall rules for different ports:

1. Run a command prompt as an administrator. ([Here](https://www.howtogeek.com/194041/how-to-open-the-command-prompt-as-administrator-in-windows-8.1/) is a guide if needed.)
1. Enter these commands, where "XXX" is [the port chosen for HTTP](#port-http) and "YYY" is [the port chosen for websocket](#port-websocket).
   * `netsh.exe advfirewall firewall add rule name= "BardAfarCustom" dir=in action=allow protocol=TCP localport=XXX,YYY`
   * `netsh.exe http add urlacl url=http://*:XXX/ user="Everyone"`
1. If you want to undo these commands at a later date:
   * `netsh.exe advfirewall firewall delete rule name="BardAfarCustom"`
   * `netsh.exe http delete urlacl url=http://*:{#PortHttp}/`
   
Business or school machines may have stricter firewall setups, where these rules are not sufficient. Contact IT support if you're in such a situation.

### Server Address for Clients

Clients connect via a web browser, and will need to supply an address to do so.

Addresses look something like `http://my.domain.com.au:58470/` or `http://20.112.52.29:58470/`

If you are running a server on your machine, your address is `http://XXX:YYY/`, where:

* `XXX` is your host name or IP address.
  * It's probably easiest to use an IPv4 address.
  * If you're hosting over the internet, you want your internet-facing IP address, not your IP address on your local network. Do an internet search for "what is my IP address"; there are a number of sites that will return your IP address. It should look something like `20.112.52.29`.
  * IPv6 addresses *should* work. But I've not tested them, and so haven't documented them.
* `YYY` is the HTTP port. This should be exactly the number you entered [here](#port-http), the default being `58470`.

# Improvements

Bard Afar started as a [minimum viable product](https://en.wikipedia.org/wiki/Minimum_viable_product). If it proves to be a popular tool, I may fix bugs and add new functionality.

Here are some improvements I would like to implement:

* Fix [known issues](#known-issues).
* UPnP support.
* Better track synchronisation, where the clients indicate to the host when they are ready for the next track.
* Client support for "dark mode" using `prefers-color-scheme` media query feature.

# Thanks To

Bard Afar makes use of:

* [Inno Setup](https://jrsoftware.org/isinfo.php)
* [Simple HTTP](https://github.com/dajuric/simple-http/)
* [websocket-sharp](http://sta.github.io/websocket-sharp/)

Bard Afar was created in [Microsoft Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/).

# Contact the Author

I can be contacted [here](https://deck16.net/contact).
