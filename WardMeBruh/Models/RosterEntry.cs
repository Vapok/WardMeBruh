namespace WardMeBruh.Models;

public struct RosterEntry
{
    public long PlayerId { get; set; }
    public string PlayerName { get; set; }

    public RosterEntry(long playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
    }
}
