using System.Collections.Generic;

namespace TOHO;

public static class Options
{
    public static BooleanOptionItem EjectionImpostors;
    public static BooleanOptionItem EjectionNeutralKillers;
    public static BooleanOptionItem EjectionCoven;
    public static StringOptionItem ConfirmEjectionsMode;

    public static BooleanOptionItem EveryoneCanSeeDeathReason;
    public static BooleanOptionItem GhostIgnoreTasks;
    public static BooleanOptionItem DisableTaskWin;
    public static BooleanOptionItem DisableSabotages;
    public static BooleanOptionItem DisableDoorClosures;
    public static BooleanOptionItem EnableCommsCamoflauge;
    public static BooleanOptionItem RandomMapsMode;
    public static BooleanOptionItem CrewCanGuess;
    public static BooleanOptionItem ImpsCanGuess;
    public static BooleanOptionItem NeutralsCanGuess;
    public static BooleanOptionItem CovenCanGuess;
    public static NumberOptionItem StartingKillCooldown;
    public static NumberOptionItem StartingAngelCooldown;
    public static NumberOptionItem NumImpostors;
    public static NumberOptionItem NumNeutralKillers;
    public static NumberOptionItem NumNeutralNonKillers;
    public static NumberOptionItem NumCoven;
    public static NumberOptionItem ModifiersPerPlayer;
    public static NumberOptionItem SabotageCooldown;

    public static BooleanOptionItem GradientTagsOpt;
    public static BooleanOptionItem EnableKcount;
    public static BooleanOptionItem KickPlayerFriendCodeInvalid;
    public static BooleanOptionItem ApplyVipList;
    public static BooleanOptionItem ApplyBanList;
    public static BooleanOptionItem ApplyModeratorList;
    public static BooleanOptionItem AllowSayCommand;
    public static BooleanOptionItem AllowStartCommand;
    public static BooleanOptionItem AutoPlayAgain;
    public static BooleanOptionItem NoGameEnd;
    public static NumberOptionItem WaitAutoStart;
    public static NumberOptionItem PlayerAutoStart;
    public static NumberOptionItem AutoStartTimer;


    public static void InitModSettings()
    {
        /***** Ejection Settings Tab *****/
        EjectionImpostors = new BooleanOptionItem("Ejection.Impostors", "Show Impostors remaining on ejection", OptionTabs.ModSettingsEjection, true);
        EjectionNeutralKillers = new BooleanOptionItem("Ejection.NeutralKillers", "Show Neutral Killers remaining on ejection", OptionTabs.ModSettingsEjection, true);
        EjectionCoven = new BooleanOptionItem("Ejection.Coven", "Show Coven remaining on ejection", OptionTabs.ModSettingsEjection, true);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionImpostors);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionNeutralKillers);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionCoven);
        
        Dictionary<int, string> dict = [];
        dict.Add(0, "Show Team");
        dict.Add(1, "Show Role");
        dict.Add(2, "Show Nothing");
        ConfirmEjectionsMode = new StringOptionItem("Ejection.ConfirmEjects", "Confirm Ejections mode", OptionTabs.ModSettingsEjection, dict);
        GameSettingMenuPatch.StringModSettings.Add(ConfirmEjectionsMode);

        /***** Game Settings Tab *****/
        EveryoneCanSeeDeathReason = new BooleanOptionItem("Game.DeathReason", "Everyone can see Death Reasons", OptionTabs.ModSettingsGame, false);
        GhostIgnoreTasks = new BooleanOptionItem("Game.GhostTasks", "Ghosts dont count toward tasks", OptionTabs.ModSettingsGame, true);
        DisableTaskWin = new BooleanOptionItem("Game.TaskWin", "Disable Task Win", OptionTabs.ModSettingsGame, false);
        DisableSabotages = new BooleanOptionItem("Game.Sabotages", "Disable Sabotages", OptionTabs.ModSettingsGame, false);
        DisableDoorClosures = new BooleanOptionItem("Game.DoorClosures", "Disable Door Closures", OptionTabs.ModSettingsGame, false);
        EnableCommsCamoflauge = new BooleanOptionItem("Game.CommsCamoflauge", "Enable Comms Camoflauge", OptionTabs.ModSettingsGame, false);
        RandomMapsMode = new BooleanOptionItem("Game.RandomMaps", "Enable Random Maps Mode", OptionTabs.ModSettingsGame, false);
        CrewCanGuess = new BooleanOptionItem("Game.CrewGuess", "Crewmates can guess", OptionTabs.ModSettingsGame, false);
        ImpsCanGuess = new BooleanOptionItem("Game.ImpsGuess", "Impostors can guess", OptionTabs.ModSettingsGame, true);
        NeutralsCanGuess = new BooleanOptionItem("Game.NeutralsGuess", "Neutrals can guess", OptionTabs.ModSettingsGame, true);
        CovenCanGuess = new BooleanOptionItem("Game.CovenGuess", "Coven can guess", OptionTabs.ModSettingsGame, true);
        GameSettingMenuPatch.BoolModSettings.Add(EveryoneCanSeeDeathReason);
        GameSettingMenuPatch.BoolModSettings.Add(GhostIgnoreTasks);
        GameSettingMenuPatch.BoolModSettings.Add(DisableTaskWin);
        GameSettingMenuPatch.BoolModSettings.Add(DisableSabotages);
        GameSettingMenuPatch.BoolModSettings.Add(DisableDoorClosures);
        GameSettingMenuPatch.BoolModSettings.Add(EnableCommsCamoflauge);
        GameSettingMenuPatch.BoolModSettings.Add(RandomMapsMode);
        GameSettingMenuPatch.BoolModSettings.Add(CrewCanGuess);
        GameSettingMenuPatch.BoolModSettings.Add(ImpsCanGuess);
        GameSettingMenuPatch.BoolModSettings.Add(NeutralsCanGuess);
        GameSettingMenuPatch.BoolModSettings.Add(CovenCanGuess);

        StartingKillCooldown = new NumberOptionItem("Game.KillCd", "Default Kill Cooldown", OptionTabs.ModSettingsGame, 20f, 5f, 60f, 1f);
        StartingAngelCooldown = new NumberOptionItem("Game.ProtectCd", "Default Protect Cooldown", OptionTabs.ModSettingsGame, 20f, 5f, 60f, 1f);
        NumImpostors = new NumberOptionItem("Game.NumImps", "# of Impostors", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumNeutralKillers = new NumberOptionItem("Game.NumNks", "# of Neutral Killers", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumNeutralNonKillers = new NumberOptionItem("Game.NumNnks", "# of Neutral Non-Killers", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumCoven = new NumberOptionItem("Game.NumCoven", "# of Coven", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        ModifiersPerPlayer = new NumberOptionItem("Game.NumModifiers", "# of Modifiers per player", OptionTabs.ModSettingsGame, 1f, 0f, 15f, 1f);
        SabotageCooldown = new NumberOptionItem("Game.SabotageCd", "Sabotage Cooldown (in seconds)", OptionTabs.ModSettingsGame, 10f, 1f, 30f, 1f);
        GameSettingMenuPatch.NumberModSettings.Add(StartingKillCooldown);
        GameSettingMenuPatch.NumberModSettings.Add(StartingAngelCooldown);
        GameSettingMenuPatch.NumberModSettings.Add(NumImpostors);
        GameSettingMenuPatch.NumberModSettings.Add(NumNeutralKillers);
        GameSettingMenuPatch.NumberModSettings.Add(NumNeutralNonKillers);
        GameSettingMenuPatch.NumberModSettings.Add(NumCoven);
        GameSettingMenuPatch.NumberModSettings.Add(ModifiersPerPlayer);
        GameSettingMenuPatch.NumberModSettings.Add(SabotageCooldown);
        
        /***** Lobby Settings Tab *****/
        GradientTagsOpt = new BooleanOptionItem("Lobby.GradientTags", "Enable Gradient Tags", OptionTabs.ModSettingsLobby, false);
        EnableKcount = new BooleanOptionItem("Lobby.Kcount", "Enable /kcount command in-game", OptionTabs.ModSettingsLobby, true);
        KickPlayerFriendCodeInvalid = new BooleanOptionItem("Lobby.InvalidFC", "Kick players with invalid friend codes", OptionTabs.ModSettingsLobby, true);
        ApplyVipList = new BooleanOptionItem("Lobby.Vip", "Apply VIP List", OptionTabs.ModSettingsLobby, true);
        ApplyBanList = new BooleanOptionItem("Lobby.Ban", "Apply Ban List", OptionTabs.ModSettingsLobby, true);
        ApplyModeratorList = new BooleanOptionItem("Lobby.Mod", "Apply Moderator List", OptionTabs.ModSettingsLobby, true);
        AllowStartCommand = new BooleanOptionItem("Lobby.StartCmd", "Allow Moderators to use /start", OptionTabs.ModSettingsLobby, true);        
        AllowSayCommand = new BooleanOptionItem("Lobby.SayCmd", "Allow Moderators to use /say", OptionTabs.ModSettingsLobby, true);
        AutoPlayAgain = new BooleanOptionItem("Lobby.AutoPlay", "Automatically restart the lobby after game ends", OptionTabs.ModSettingsLobby, false);
        NoGameEnd = new BooleanOptionItem("Lobby.NoGameEnd", "No Game End (dev tool)", OptionTabs.ModSettingsLobby, false);
        GameSettingMenuPatch.BoolModSettings.Add(GradientTagsOpt);
        GameSettingMenuPatch.BoolModSettings.Add(EnableKcount);
        GameSettingMenuPatch.BoolModSettings.Add(KickPlayerFriendCodeInvalid);
        GameSettingMenuPatch.BoolModSettings.Add(ApplyVipList);
        GameSettingMenuPatch.BoolModSettings.Add(ApplyBanList);
        GameSettingMenuPatch.BoolModSettings.Add(ApplyModeratorList);
        GameSettingMenuPatch.BoolModSettings.Add(AllowStartCommand);
        GameSettingMenuPatch.BoolModSettings.Add(AllowSayCommand);
        GameSettingMenuPatch.BoolModSettings.Add(AutoPlayAgain);
        GameSettingMenuPatch.BoolModSettings.Add(NoGameEnd);

        WaitAutoStart = new NumberOptionItem("Lobby.WaitAutoStart", "Amount of time that must pass before Auto-Starting (in seconds)", OptionTabs.ModSettingsLobby, 300f, 60f, 600f, 10f);
        PlayerAutoStart = new NumberOptionItem("Lobby.PlayerAutoStart", "Players required to Auto-Start", OptionTabs.ModSettingsLobby, 15f, 4f, 15f, 1f);
        AutoStartTimer = new NumberOptionItem("Lobby.AutoStartTimer", "Auto-start Start Timer (in seconds)", OptionTabs.ModSettingsLobby, 5f, 1f, 30f, 1f);
        GameSettingMenuPatch.NumberModSettings.Add(WaitAutoStart);
        GameSettingMenuPatch.NumberModSettings.Add(PlayerAutoStart);
        GameSettingMenuPatch.NumberModSettings.Add(AutoStartTimer);
    }
}