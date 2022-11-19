# Unity-Logger

<img align="center" width="400" height="400" src="https://i.imgur.com/qNxrouI.png">

```C#
Logger.Log(Channel.AI, "Finding NPC paths");
Logger.Log(Channel.Audio, "Loading audio banks");
Logger.Log(Channel.Loading, "Begin Load");
Logger.Log(Channel.Loading, Priority.Error, "Load failed");
```
---

Unity-Logger is a simple channel driven logging script for use with Unity.

It was created during the development of [Off Grid](http://www.offgridthegame.com) to allow us to focus on current tasks and bugs by disabling logs from unrelated channels.

Due to it's nicely formatted output, it also allows the user to easily distinguish between the different channels.

Provided in this repo is also a simple editor script which allows the user to turn channels on and off, this works both in play mode and whilst editing.

# Installation
Simply copy the contents of this git repo to Assets/Plugins/Unity-Logger in your Unity project
