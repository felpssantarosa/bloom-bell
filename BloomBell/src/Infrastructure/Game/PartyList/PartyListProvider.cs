using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace BloomBell.src.Infrastructure.Game.PartyList;

public class PartyListProvider : IDisposable
{
    private const int UpdateTicks = 60;
    private const int BufferSize = 24;

    private int bufferIndex = 0;
    private int ticksCount = UpdateTicks;

    private readonly Dictionary<long, PartyListMemberInfo>[] buffers = [new(BufferSize), new(BufferSize)];

    public delegate void OnEventDelegate(bool status, PartyListMemberInfo member);
    public event OnEventDelegate OnEvent = delegate { };

    public PartyListProvider()
    {
        GameServices.Framework.Update += onUpdate;
    }

    public void Dispose()
    {
        GameServices.Framework?.Update -= onUpdate;
        buffers[0]?.Clear();
        buffers[1]?.Clear();
    }

    private Dictionary<long, PartyListMemberInfo> current => buffers[bufferIndex];
    private Dictionary<long, PartyListMemberInfo> previous => buffers[1 - bufferIndex];

    public bool IsCrossWorld => InfoProxyCrossRealm.IsCrossRealmParty();
    public bool IsAliance => IsCrossWorld ? InfoProxyCrossRealm.IsAllianceRaid() : GameServices.PartyList.IsAlliance;

    public Dictionary<long, PartyListMemberInfo> GetPartyMembers()
    {
        if (GameServices.PlayerState.ContentId == 0) return [];

        Dictionary<long, PartyListMemberInfo> buffer = new(current.Count);

        updateBuffer(buffer);

        return buffer;
    }

    private void onUpdate(IFramework framework)
    {
        try
        {
            ticksCount++;

            if (ticksCount < UpdateTicks) return;

            ticksCount = 0;

            if (GameServices.PlayerState.ContentId == 0) return;

            bufferIndex = 1 - bufferIndex;

            current.Clear();
            updateBuffer(current);

            foreach (var member in current)
            {
                if (previous.ContainsKey(member.Key)) continue;

                try
                {
                    OnEvent.Invoke(true, member.Value);
                }
                catch (Exception exception)
                {
                    GameServices.PluginLog.Error(exception, "Unexpected Exception");
                }
            }

            foreach (var member in previous)
            {
                if (current.ContainsKey(member.Key)) continue;

                try
                {
                    OnEvent.Invoke(false, member.Value);
                }
                catch (Exception exception)
                {
                    GameServices.PluginLog.Error(exception, "Unexpected Exception");
                }
            }
        }
        catch (Exception exception)
        {
            GameServices.PluginLog.Error(exception, "Unexpected Exception");
            Dispose();
        }
    }

    private unsafe void updateBuffer(Dictionary<long, PartyListMemberInfo> buffer)
    {
        if (!IsCrossWorld)
        {
            foreach (var member in GameServices.PartyList)
            {
                if (member.ContentId == 0) continue;
                buffer[member.ContentId] = new()
                {
                    Id = member.ContentId,
                    Name = member.Name.TextValue,
                    WorldId = member.World.RowId,
                    ClassJobId = member.ClassJob.RowId,
                    Level = member.Level,
                };
            }
            return;
        }

        var infoProxyCrossRealmInstance = InfoProxyCrossRealm.Instance();

        if (infoProxyCrossRealmInstance == null)
        {
            buffer.Clear();
            return;
        }

        ref readonly var infoProxyCrossRealm = ref *infoProxyCrossRealmInstance;

        var groups = infoProxyCrossRealm.CrossRealmGroups;
        int groupCount = infoProxyCrossRealm.GroupCount;

        for (var groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            ref readonly var group = ref groups[groupIndex];

            for (var memberIndex = 0; memberIndex < group.GroupMemberCount; memberIndex++)
            {
                ref readonly var member = ref group.GroupMembers[memberIndex];

                if (member.ContentId == 0) continue;

                buffer[(long)member.ContentId] = new()
                {
                    Id = (long)member.ContentId,
                    Name = SeString.Parse(member.Name).TextValue,
                    WorldId = (uint)member.HomeWorld,
                    ClassJobId = member.ClassJobId,
                    Level = member.Level,
                };
            }
        }
    }

    public unsafe int GetPartySize()
    {
        if (!InfoProxyCrossRealm.IsCrossRealmParty())
        {
            return GameServices.PartyList.Count;
        }

        var infoProxyCrossRealmInstance = InfoProxyCrossRealm.Instance();
        if (infoProxyCrossRealmInstance == null) return 0;

        var total = 0;
        ref readonly var infoProxyCrossRealm = ref *infoProxyCrossRealmInstance;

        var groups = infoProxyCrossRealm.CrossRealmGroups;
        int groupCount = infoProxyCrossRealm.GroupCount;

        for (var index = 0; index < groupCount; index++)
        {
            ref readonly var group = ref groups[index];
            total += group.GroupMemberCount;
        }

        return total;
    }
}
