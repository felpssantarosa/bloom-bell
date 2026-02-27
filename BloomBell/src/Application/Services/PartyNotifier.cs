using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BloomBell.src.Infrastructure.Configuration;
using BloomBell.src.Infrastructure.Game;
using Newtonsoft.Json;

namespace BloomBell.src.Application.Services;

/// <summary>
/// Monitors party size changes and sends notifications to the backend
/// when the party reaches the configured threshold.
/// </summary>
public sealed class PartyNotifier(PluginConfiguration pluginConfiguration) : IDisposable
{
    private readonly HttpClient httpClient = new();
    private bool isInitialized = false;
    private int lastPartySize = 0;
    private bool alreadyNotified = false;
    private bool lastIsCrossWorld = false;

    public async Task UpdateAsync(int currentPartySize, ulong contentId, bool isCrossWorld)
    {
        var maxSize = pluginConfiguration.maxPartySize;

        if (!isInitialized)
        {
            isInitialized = true;
            lastPartySize = currentPartySize;
            lastIsCrossWorld = isCrossWorld;
            alreadyNotified = currentPartySize == maxSize;
            return;
        }

        if (isCrossWorld != lastIsCrossWorld)
        {
            lastIsCrossWorld = isCrossWorld;
            lastPartySize = currentPartySize;
            alreadyNotified = currentPartySize == maxSize;
            return;
        }

        if (currentPartySize == lastPartySize) return;

        if (currentPartySize < maxSize) alreadyNotified = false;

        if (currentPartySize == maxSize && !alreadyNotified)
        {
            alreadyNotified = true;

            if (pluginConfiguration.pauseNotifications) return;

            if (!pluginConfiguration.notifyWhenFocused && Dalamud.Utility.Util.ApplicationIsActivated()) return;

            var payload = new
            {
                pluginUserId = contentId.ToString(),
                partySize = currentPartySize,
                maxSize
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await httpClient.PostAsync(InternalConfiguration.NotificationUrl, content);
        }

        lastPartySize = currentPartySize;
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
