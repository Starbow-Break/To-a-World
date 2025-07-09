using System;

public class NpcEvents : IEvents
{
    public event Action<INpc> OnEnteredNpc;
    public void EnteredNpc(INpc npc) => OnEnteredNpc?.Invoke(npc);
    
    public event Action<INpc> OnExitedNpc;
    public void ExitedNpc(INpc npc) => OnExitedNpc?.Invoke(npc);
}
