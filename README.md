# Bard Afar

Audio streaming server for Windows, intended for music ambience for online tabletop RPG sessions, but possibly usable for a number of applications.

The host runs Bard Afar; clients need only to connect with a web browser.

## Screenshots: Host

![image](https://user-images.githubusercontent.com/44771168/226324058-2395ce9a-0d47-480f-8f38-ff96b3606260.png)

![image](https://user-images.githubusercontent.com/44771168/226331131-df570a86-0ca4-41fc-98bc-264bd07af4a6.png)

## Screenshots: Client

![image](https://user-images.githubusercontent.com/44771168/226339516-ea85c449-bc64-4c4c-a678-c8c6b2e77e78.png)

## Raison D'etre

In 2021, YouTube started [taking down](https://www.theverge.com/2021/9/12/22669502/youtube-discord-rythm-music-bot-closure) a variety of Discord bots that were able to play audio from Discord. These bots were the go-to way for many online [tabletop RPG](https://en.wikipedia.org/wiki/Tabletop_role-playing_game) groups to add music to their gaming sessions, given that Discord was commonly used for voice chat anyway.

Nowadays, some [Virtual Tabletops](https://en.wikipedia.org/wiki/Digital_tabletop_game#Virtual_tabletops) have ways to play music. Some do it better than others, and not everyone uses a Virtual Tabletop for online play.

I thought it would be trivially easy to find easy-to-use live-audio-sharing software. I couldn't find anything. So I made Bard Afar. Bard Afar ***does not*** do what the old Discord bots does: it doesn't play YouTube videos. Instead it plays audio files on your computer. But that's good enough for me.

(Maybe my search-fu is weak and there are are simple live-audio-sharing programs out there. If I missed some, oh well, now Bard Afar is another!)

## Design Philosopy

Bard Afar started as a [minimum viable product](https://en.wikipedia.org/wiki/Minimum_viable_product). If it proves to be a popular tool, I may fix bugs and add new functionality.

# ⚠️ Important Notes ⚠️

Please read and understand the following before use.

1. This software is new, and hasn't been extensively tested yet.
2. This software exposes files on your hard drive to the internet. It could share personal documents or other private files if:
   1. you mis-configure the software, or
   2. the software has bugs.
3. There are some technical steps involved in setting up a Bard Afar server. Common issues are explained below. But I cannot predict every possible issue for every possible network.

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

Generally it's easiest to use the special wildcard character `*`, which listens on all 

### Port (HTTP) 

The port used for the HTTP server. The default is `58470`.

### Port (WebSocket)

The port used for the websocket server. The default is `58471`.

### Audio Files Directory

The full path to a directory containing the audio/music files you wish to play to clients.

* This directory can contain any number of sub-directories. You will be able to navigate freely between the contents of the specified directory and all sub-directories.

### Silence Between Tracks (Seconds)

Sometimes a client can be delayed in playing a track. This can happen because of network latency or limited bandwidth. In any case, it can cause the audio on the client end to end prematurely when the server moves onto the next track. This can be allievated by adding some "padding" to the end of each track. This setting controls how many seconds of silent "padding" is added to the end of each track.

Generally a value of around `10` seconds is recommended.

## Using the Player

1. Bard Afar will now start the server. The player interface will be shown.
2. At this time, you can have clients connect to your server.
   * Clients will need to know an address to connect to. See [here](#server-address-for-clients) for more information.
3. Close the window when you are done. This will stop playback and close the server.

![image](https://user-images.githubusercontent.com/44771168/226331131-df570a86-0ca4-41fc-98bc-264bd07af4a6.png)

Player notes:

* Any connected client will hear what you play.
* Hover over a button or control to see a tool-tip giving the button's purpose.
* To navigate between sub-directories, click the directory names.
  * Click on `..` to move to the parent directory. You cannot move to the parent of the [Audio Files Directory](#audio-files-directory) as configured above.

# Using the Bard Afar Client

1. Receive an address from the person running the server. More information [here](#server-address-for-clients).
2. Open a modern-day web browser (such as Chrome or Firefox).
   * Bard Afar should work on modern-day browsers on ***any device***. PCs, phones, tablets, etc.
4. Enter that address into the address bar.
5. You should see the Bard Afar page.<br/>
   ![image](https://user-images.githubusercontent.com/44771168/226333277-cd5cc942-69c4-4e6c-8969-a5448d67de8d.png)
5. Click the "Click to Connect" button.
6. The LED indicator should turn green.<br/>
   ![image](https://user-images.githubusercontent.com/44771168/226334098-166079f3-6091-49d8-bd9b-69e515d69009.png)
8. You are now able to hear music from the server.
   * You can't hear a track that is in-progress. Wait for the next track to start.
9. You can adjust the volume with the slider.
10. You can download the currently-playing track by clicking the floppy disk icon 💾.
11. You can close the browser, or the browser tab, safely at any time to stop using Bard Afar.
12. If there's a failure of any sort, try refreshing the page.

# Networking Issues

## Server Address for Clients

# Contact the Author

I can be contacted [here](https://deck16.net/contact).

