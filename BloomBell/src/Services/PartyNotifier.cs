using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using BloomBell.src.Configuration;

namespace BloomBell.src.services;

public class PartyNotifier(PluginConfiguration pluginConfiguration) : IDisposable
{
    private readonly HttpClient httpClient = new();
    private int lastPartySize = -1;
    private bool alreadyNotified = false;
    private bool lastIsCrossWorld = false;

    public async Task UpdateAsync(int currentPartySize, ulong contentId, bool isCrossWorld)
    {
        var maxSize = pluginConfiguration.maxPartySize;

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

            // if (Dalamud.Utility.Util.ApplicationIsActivated()) return;

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
