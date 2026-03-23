//Thanks EHR for https://github.com/Gurge44/EndlessHostRoles/blob/main/Roles/Modifiers/IModifier.cs and everything related ;)

using System.Reflection;

namespace TOHO.Roles.Modifiers
{
    [Obfuscation(Exclude = true)]
    public enum ModifierTypes
    {
        Impostor,
        Helpful,
        Harmful,
        Misc,
        Guesser,
        Mixed,
        Experimental
    }
    public interface IModifier
    {
        public CustomRoles Role { get; }
        public ModifierTypes Type { get; }
        public void SetupCustomOption();

        public void Init();
        public void Add(byte playerId, bool gameIsLoading = true);
        public void Remove(byte playerId);
        public void OnFixedUpdate(PlayerControl pc)
        { }
        public void OnFixedUpdateLowLoad(PlayerControl pc)
        { }
    }
}
