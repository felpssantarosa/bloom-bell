using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using BloomBell.src.Configuration;

namespace BloomBell.src.services;

public class PartyNotifier() : IDisposable
{
    private readonly HttpClient httpClient = new();
    private int lastPartySize = -1;
    private bool alreadyNotified = false;
    private bool lastIsCrossWorld = false;

    public async Task UpdateAsync(int currentPartySize, ulong contentId, bool isCrossWorld)
    {
        if (isCrossWorld != lastIsCrossWorld)
        {
            lastPartySize = -1;
            alreadyNotified = false;
            lastIsCrossWorld = isCrossWorld;
        }

        if (currentPartySize == lastPartySize) return;

        var maxSize = 8;

        if (currentPartySize < maxSize)
        {
            alreadyNotified = false;
        }

        if (currentPartySize == maxSize && !alreadyNotified)
        {
            alreadyNotified = true;

            if (Dalamud.Utility.Util.ApplicationIsActivated()) return;

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
