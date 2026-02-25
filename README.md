# 🌸 Bloom Bell

### A plugin for FFXIV that provides warnings for when you are waiting to fill a party.

This plugin was made as an intent to solve a problem that players may have when they need to afk for a moment or just want to turn off their brains and not worry about the final fantasy xiv window for a moment.

<br/>

> [!WARNING]
> if you plan on self hosting the bot, make sure to clone the backend repository as well and follow the instructions there to set it up. The bot is required for the plugin to work, and you won't be able to receive notifications without it. The link to the backend repository is [here](https://github.com/felpssantarosa/bloom-bell-backend).

# 🪀 Features

- Discord integration to send you a message when your party is ready.

# 📖 How to use

### Requirements:

- You need to be in the same server as the bot for it to work.
- Your privacy settings must allow you to receive DMs from the server the bot is present in at very least.

<br/>

## 🌐 Installation

### Adding a Custom Plugin Repository

1. Open Dalamud Plugin Installer or type `/xlplugins`.
2. Click Settings at the bottom.
3. Open Experimental tab.
4. Paste the Following Repository in Custom Plugin Repositories field.

```bash
https://raw.githubusercontent.com/felpssantarosa/bloom-bell/refs/heads/master/repo.json
```

5. Click +.
6. Click 💾 to Save changes and close.
7. Install the plugin opening the plugins' list, it should show up there.

<br/>

---

# 🤓 Nerd Zone

## 🛠️ How to build the project

1. Clone the repository.

```bash
git clone https://github.com/felpssantarosa/bloom-bell.git
````

2. Build the plugin using your preferred IDE (Visual Studio, Rider, etc.) or using the command line

```bash
dotnet build
```

3. Go to Dalamud Settings -> Experimental, and under the Dev Plugin Locations, add the path to the dll

```
C:\path\to\project\BloomBell\bin\x64\Debug\BloomBell.dll
```

4. Enable the plugin and authenticate with Discord.
