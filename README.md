# 🌸 Bloom Bell

### A plugin for FFXIV that provides warnings for when you are waiting to fill a party.

This plugin was made as an intent to solve a problem that players may have when they need to afk for a moment or just want to turn off their brains and not worry about the final fantasy xiv window for a moment.

# Features

- Discord integration to send you a message when your party is ready.

# How to use

### Requirements:
 - You need to be in the same server as the bot for it to work.
 - Your privacy settings must allow you to receive DMs from the server the bot is present in at very least.

<br/>

1. Clone the repository.

```bash
git clone https://github.com/felpssantarosa/bloom-bell.git
```

2. Build the plugin using your preferred IDE (Visual Studio, Rider, etc.) or using the command line

```bash
dotnet build
```
3. Go to Dalamud Settings -> Experimental, and under the Dev Plugin Locations, add the path to the dll

```
C:\path\to\project\BloomBell\bin\x64\Debug\BloomBell.dll
```

4. Enable the plugin and authenticate with Discord.

5. Enjoy!

When you hit 8 players in a party (normal or crossworld) you will get notified through Discord's DMs
