namespace WardMeBruh.Models;

public struct PlayerRoleAssignment
{
    public long PlayerId { get; set; }
    public int RoleId { get; set; }
    public string PlayerName { get; set; }

    public PlayerRoleAssignment(long playerId, int roleId, string playerName)
    {
        PlayerId = playerId;
        RoleId = roleId;
        PlayerName = playerName;
    }
}
