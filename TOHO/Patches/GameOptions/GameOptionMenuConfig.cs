using System.Collections.Generic;

namespace TOHO;

public static class GameOptionMenuConfig
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
        /***** Ejection Settings Tab *****/ /* 100-199 */
        EjectionImpostors = new BooleanOptionItem(100, "Show Impostors remaining on ejection", OptionTabs.ModSettingsEjection, true);
        EjectionNeutralKillers = new BooleanOptionItem(101, "Show Neutral Killers remaining on ejection", OptionTabs.ModSettingsEjection, true);
        EjectionCoven = new BooleanOptionItem(102, "Show Coven remaining on ejection", OptionTabs.ModSettingsEjection, true);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionImpostors);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionNeutralKillers);
        GameSettingMenuPatch.BoolModSettings.Add(EjectionCoven);
        
        Dictionary<int, string> dict = [];
        dict.Add(0, "Show Team");
        dict.Add(1, "Show Role");
        dict.Add(2, "Show Nothing");
        ConfirmEjectionsMode = new StringOptionItem(103, "Confirm Ejections mode", OptionTabs.ModSettingsEjection, dict);
        GameSettingMenuPatch.StringModSettings.Add(ConfirmEjectionsMode);

        /***** Game Settings Tab *****/ /* 200-299 */
        EveryoneCanSeeDeathReason = new BooleanOptionItem(200, "Everyone can see Death Reasons", OptionTabs.ModSettingsGame, false);
        GhostIgnoreTasks = new BooleanOptionItem(201, "Ghosts dont count toward tasks", OptionTabs.ModSettingsGame, true);
        DisableTaskWin = new BooleanOptionItem(202, "Disable Task Win", OptionTabs.ModSettingsGame, false);
        DisableSabotages = new BooleanOptionItem(203, "Disable Sabotages", OptionTabs.ModSettingsGame, false);
        DisableDoorClosures = new BooleanOptionItem(204, "Disable Door Closures", OptionTabs.ModSettingsGame, false);
        EnableCommsCamoflauge = new BooleanOptionItem(205, "Enable Comms Camoflauge", OptionTabs.ModSettingsGame, false);
        RandomMapsMode = new BooleanOptionItem(206, "Enable Random Maps Mode", OptionTabs.ModSettingsGame, false);
        CrewCanGuess = new BooleanOptionItem(207, "Crewmates can guess", OptionTabs.ModSettingsGame, false);
        ImpsCanGuess = new BooleanOptionItem(208, "Impostors can guess", OptionTabs.ModSettingsGame, true);
        NeutralsCanGuess = new BooleanOptionItem(209, "Neutrals can guess", OptionTabs.ModSettingsGame, true);
        CovenCanGuess = new BooleanOptionItem(210, "Coven can guess", OptionTabs.ModSettingsGame, true);
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

        StartingKillCooldown = new NumberOptionItem(211, "Default Kill Cooldown", OptionTabs.ModSettingsGame, 20f, 5f, 60f, 1f);
        StartingAngelCooldown = new NumberOptionItem(212, "Default Protect Cooldown", OptionTabs.ModSettingsGame, 20f, 5f, 60f, 1f);
        NumImpostors = new NumberOptionItem(213, "# of Impostors", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumNeutralKillers = new NumberOptionItem(214, "# of Neutral Killers", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumNeutralNonKillers = new NumberOptionItem(215, "# of Neutral Non-Killers", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        NumCoven = new NumberOptionItem(216, "# of Coven", OptionTabs.ModSettingsGame, 2f, 0f, 15f, 1f);
        ModifiersPerPlayer = new NumberOptionItem(217, "# of Modifiers per player", OptionTabs.ModSettingsGame, 1f, 0f, 15f, 1f);
        SabotageCooldown = new NumberOptionItem(218, "Sabotage Cooldown (in seconds)", OptionTabs.ModSettingsGame, 10f, 1f, 30f, 1f);
        GameSettingMenuPatch.NumberModSettings.Add(StartingKillCooldown);
        GameSettingMenuPatch.NumberModSettings.Add(StartingAngelCooldown);
        GameSettingMenuPatch.NumberModSettings.Add(NumImpostors);
        GameSettingMenuPatch.NumberModSettings.Add(NumNeutralKillers);
        GameSettingMenuPatch.NumberModSettings.Add(NumNeutralNonKillers);
        GameSettingMenuPatch.NumberModSettings.Add(NumCoven);
        GameSettingMenuPatch.NumberModSettings.Add(ModifiersPerPlayer);
        GameSettingMenuPatch.NumberModSettings.Add(SabotageCooldown);
        
        /***** Lobby Settings Tab *****/ /* 300-399 */
        GradientTagsOpt = new BooleanOptionItem(300, "Enable Gradient Tags", OptionTabs.ModSettingsLobby, false);
        EnableKcount = new BooleanOptionItem(301, "Enable /kcount command in-game", OptionTabs.ModSettingsLobby, true);
        KickPlayerFriendCodeInvalid = new BooleanOptionItem(302, "Kick players with invalid friend codes", OptionTabs.ModSettingsLobby, true);
        ApplyVipList = new BooleanOptionItem(303, "Apply VIP List", OptionTabs.ModSettingsLobby, true);
        ApplyBanList = new BooleanOptionItem(304, "Apply Ban List", OptionTabs.ModSettingsLobby, true);
        ApplyModeratorList = new BooleanOptionItem(305, "Apply Moderator List", OptionTabs.ModSettingsLobby, true);
        AllowStartCommand = new BooleanOptionItem(306, "Allow Moderators to use /start", OptionTabs.ModSettingsLobby, true);        
        AllowSayCommand = new BooleanOptionItem(307, "Allow Moderators to use /say", OptionTabs.ModSettingsLobby, true);
        AutoPlayAgain = new BooleanOptionItem(308, "Automatically restart the lobby after game ends", OptionTabs.ModSettingsLobby, false);
        NoGameEnd = new BooleanOptionItem(309, "No Game End (dev tool)", OptionTabs.ModSettingsLobby, false);
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

        WaitAutoStart = new NumberOptionItem(310, "Amount of time that must pass before Auto-Starting (in seconds)", OptionTabs.ModSettingsLobby, 300f, 60f, 600f, 10f);
        PlayerAutoStart = new NumberOptionItem(311, "Players required to Auto-Start", OptionTabs.ModSettingsLobby, 15f, 4f, 15f, 1f);
        AutoStartTimer = new NumberOptionItem(312, "Auto-start Start Timer (in seconds)", OptionTabs.ModSettingsLobby, 5f, 1f, 30f, 1f);
        GameSettingMenuPatch.NumberModSettings.Add(WaitAutoStart);
        GameSettingMenuPatch.NumberModSettings.Add(PlayerAutoStart);
        GameSettingMenuPatch.NumberModSettings.Add(AutoStartTimer);
    }
}