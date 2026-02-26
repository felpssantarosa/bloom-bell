namespace BloomBell.src.Library.External.Game.PartyList;

public readonly struct PartyListMemberInfo
{
    public readonly long Id { get; init; }
    public readonly string Name { get; init; }
    public readonly uint WorldId { get; init; }
    public readonly uint ClassJobId { get; init; }
    public readonly uint Level { get; init; }

    public override string ToString() => $"{{Id={Id}, Name=\"{Name}\", WorldId={WorldId}, ClassJobId={ClassJobId}, Level={Level}}}";
}
