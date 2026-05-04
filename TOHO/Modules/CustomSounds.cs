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
    
    private static bool _musicPlaying;
    
    public static bool _repeatMode = false;
    public static void ToggleRepeat() => _repeatMode = !_repeatMode;
    public static int i;
    public static string songname;
    
    private static CancellationTokenSource _musicCts;
    public static void SkipSong()
    {
        _musicCts?.Cancel(); // kill current loop

        StopAllSounds();

        i++;
        _musicPlaying = false;

        MusicPlay(); // start fresh loop
    }

    public static int GetWavDurationMs(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read); 
        using var br = new BinaryReader(fs);

    // RIFF header
        string riff = new string(br.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file");

        br.ReadInt32(); // file size

        string wave = new string(br.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file");

        int sampleRate = 0;
        short bitsPerSample = 0;
        short channels = 0;
        int dataSize = 0;

    // Walk chunks
        while (fs.Position < fs.Length)
        {
            string chunkId = new string(br.ReadChars(4));
            int chunkSize = br.ReadInt32();

            switch (chunkId)
            {
                case "fmt ":
                    short audioFormat = br.ReadInt16();
                    channels = br.ReadInt16();
                    sampleRate = br.ReadInt32();
                    br.ReadInt32(); // byte rate
                    br.ReadInt16(); // block align
                    bitsPerSample = br.ReadInt16();

                // Skip any extra fmt bytes
                    if (chunkSize > 16)
                        br.ReadBytes(chunkSize - 16);
                    break;

                case "data":
                    dataSize = chunkSize;
                // We don’t need to read the actual audio data
                    fs.Position += chunkSize;
                    break;

                default:
                    // Skip unknown chunks (LIST, fact, etc.)
                    fs.Position += chunkSize;
                    break;
            }
            if ((chunkSize & 1) == 1) fs.Position++;
        }

        if (sampleRate == 0 || bitsPerSample == 0 || channels == 0 || dataSize == 0)
            throw new InvalidDataException("Invalid WAV file");

        int bytesPerSample = (bitsPerSample / 8) * channels;
        double durationSeconds = (double)dataSize / (sampleRate * bytesPerSample);

        return (int)(durationSeconds * 1000);
    }
    
    public static async Task PlaySong(string[] files, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (files.Length == 0) return;

                if (i >= files.Length) i = 0;

                var path = files[i];

                StartPlay(path);
                songname = Path.GetFileNameWithoutExtension(path);
                Logger.SendInGameMusic($"{songname}");

                int length = GetWavDurationMs(path);

                if (!_repeatMode)
                {
                    i++;
                    if (i >= files.Length) i = 0;
                }

                await Task.Delay(length, token);

                StopAllSounds();
                
                await Task.Delay(50, token);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            Logger.Error($"Music loop crashed: {ex}", "CustomSounds");
            _musicPlaying = false;
        }
    }
    
    public static async void MusicPlay()
    {
        if (OperatingSystem.IsAndroid()) return;
        if (!Constants.ShouldPlaySfx() || !Main.EnableCustomSoundEffect.Value) return;

        if (_musicPlaying)
            return;

        _musicPlaying = true;
        _musicCts = new CancellationTokenSource();

        var files = Directory.GetFiles(MUSIC_PATH, "*.wav");
        if (files.Length == 0)
        {
            Logger.Warn("No .wav files found in music folder", "CustomSounds");
            _musicPlaying = false;
            return;
        }

        Array.Sort(files);

        if (i >= files.Length || i < 0)
            i = 0;
        await PlaySong(files, _musicCts.Token);
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
#else
    [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
    private static extern bool PlaySound(string Filename, int Mod, int Flags);
    private static void StartPlay(string path) => PlaySound(@$"{path}", 0, 1); //第3个形参，把1换为9，连续播放
    public static void StopAllSounds()
    {
        _musicPlaying = false;
        PlaySound(null, 0, 0x40);
    }
#endif
}

