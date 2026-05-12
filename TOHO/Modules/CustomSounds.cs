using Hazel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Audio;

namespace TOHO.Modules;

public static class CustomSoundsManager
{
    public static void RPCPlayCustomSound(this PlayerControl pc, string sound, bool force = false)
    {
        if (!force) if (!AmongUsClient.Instance.AmHost || !pc.IsModded()) return;
        if (pc == null || PlayerControl.LocalPlayer.PlayerId == pc.PlayerId)
        {
            Play(sound);
            return;
        }
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlayCustomSound, SendOption.Reliable, pc.GetClientId());
        writer.Write(sound);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    public static void RPCPlayCustomSoundAll(string sound)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlayCustomSound, SendOption.Reliable, -1);
        writer.Write(sound);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        Play(sound);
    }
    public static void ReceiveRPC(MessageReader reader) => Play(reader.ReadString());

    private static readonly string SOUNDS_PATH = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}/BepInEx/resources/";
    private static readonly string MUSIC_PATH = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}/BepInEx/resources/music/";
        
    public static string songname;
    
    public static void SkipSong()
    {
        StopAllSounds();

        MusicPlay(); // start fresh loop
    }

   
    public static async Task PlaySong(string[] files)
    {
        try
        {
            if (files.Length == 0) return;

            var path = files.RandomElement();

            StartPlay(path);
            songname = Path.GetFileNameWithoutExtension(path);
            Logger.SendInGameMusic($"{songname}");
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            Logger.Error($"Music loop crashed: {ex}", "CustomSounds");
        }
    }
    
    public static async void MusicPlay()
    {
        if (OperatingSystem.IsAndroid()) return;
        if (!Constants.ShouldPlaySfx() || !Main.EnableCustomSoundEffect.Value) return;



        var files = Directory.GetFiles(MUSIC_PATH, "*.wav");
        if (files.Length == 0)
        {
            Logger.Warn("No .wav files found in music folder", "CustomSounds");
            return;
        }

        Array.Sort(files);

        await PlaySong(files);
    }
    
    public static void Play(string sound)
    {
        if (OperatingSystem.IsAndroid()) return; // Android doesn't have winmm.dll
        if (!Constants.ShouldPlaySfx() || !Main.EnableCustomSoundEffect.Value) return;
        var path = SOUNDS_PATH + sound + ".wav";
        if (!Directory.Exists(SOUNDS_PATH)) Directory.CreateDirectory(SOUNDS_PATH);
        DirectoryInfo folder = new(SOUNDS_PATH);
        if ((folder.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            folder.Attributes = FileAttributes.Hidden;
        if (!File.Exists(path))
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TOHO.Resources.Sounds." + sound + ".wav");
            if (stream == null)
            {
                Logger.Warn($"Sound file missing：{sound}", "CustomSounds");
                return;
            }
            var fs = File.Create(path);
            stream.CopyTo(fs);
            fs.Close();
        }
        StartPlay(path);
        Logger.Msg($"play sound：{sound}", "CustomSounds");
    }
#if ANDROID
    private static void StartPlay(string _) {  }
    public static void StopAllSounds() {  }
    public static void PlaySound(string _, int __, int ___) {  }
#else
    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    private static extern bool PlaySound(string Filename, int Mod, int Flags);
    private static void StartPlay(string path) => PlaySound(@$"{path}", 0, 1); //第3个形参，把1换为9，连续播放
    public static void StopAllSounds()
    {
        PlaySound(null, 0, 0x40);
    }
#endif
}

