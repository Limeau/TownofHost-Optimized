using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

namespace TOHO.Modules;

public class ModdedSupportBAUEvent
{
    public bool OnBAULoad(BasePlugin bauPlugin)
    {
        return true;
    }
    
    public void OnBAUOptionsLoaded(object[] options)
    { }
    
    public void OnBAUConfigEntriesLoaded(ConfigEntryBase[] configs)
    { }
}