using System;

public class NpcEvents : IEvents
{
    public event Action<Npc> OnEnteredNpc;
    public void EnteredNpc(Npc npc) => OnEnteredNpc?.Invoke(npc);
    
    public event Action<Npc> OnExitedNpc;
    public void ExitedNpc(Npc npc) => OnExitedNpc?.Invoke(npc);
}
