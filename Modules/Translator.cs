using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TOHE;

public static class Translator
{
    public static Dictionary<string, Dictionary<int, string>> translateMaps;
    public const string LANGUAGE_FOLDER_NAME = Main.LANGUAGE_FOLDER_NAME;
    private static readonly Dictionary<SupportedLangs, Dictionary<CustomRoles, string>> ActualRoleNames = [];
    public static readonly Dictionary<CustomRoles, HashSet<string>> CrossLangRoleNames = [];
    public static void Init()
    {
        Logger.Info("Loading language files...", "Translator");
        LoadLangs();
        Logger.Info("Language file loaded successfully", "Translator");
    }
    public static void LoadLangs()
    {
        try
        {
            // Get the assembly containing the resources
            var assembly = Assembly.GetExecutingAssembly();
            // Get the directory containing the JSON files (e.g., TOHE.Resources.Lang)
            string[] jsonFileNames = GetJsonFileNames(assembly, "TOHE.Resources.Lang");

            translateMaps = [];


            if (jsonFileNames.Length == 0)
            {
                Logger.Warn("Json Translation files does not exist.", "Translator");
                return;
            }
            foreach (string jsonFileName in jsonFileNames)
            {
                // Read the JSON file content
                using Stream resourceStream = assembly.GetManifestResourceStream(jsonFileName);

                if (resourceStream != null)
                {
                    using StreamReader reader = new(resourceStream);

                    string jsonContent = reader.ReadToEnd();
                    // Deserialize the JSON into a dictionary
                    Dictionary<string, string> jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                    if (jsonDictionary.TryGetValue("LanguageID", out string languageIdObj) && int.TryParse(languageIdObj, out int languageId))
                    {
                        // Remove the "LanguageID" entry
                        jsonDictionary.Remove("LanguageID");

                        // Handle the rest of the data and merge it into the resulting translation map
                        MergeJsonIntoTranslationMap(translateMaps, languageId, jsonDictionary);
                    }
                    else
                    {
                        //Logger.Warn(jsonDictionary["HostText"], "Translator");
                        Logger.Warn($"Invalid JSON format in {jsonFileName}: Missing or invalid 'LanguageID' field.", "Translator");
                    }
                }
            }

            // Convert the resulting translation map to JSON
            string mergedJson = JsonSerializer.Serialize(translateMaps, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            Logger.Error($"Error: {ex}", "Translator");
        }
        if (!Directory.Exists(LANGUAGE_FOLDER_NAME)) Directory.CreateDirectory(LANGUAGE_FOLDER_NAME);

        CreateTemplateFile();

        //Load vanilla role names into CrossLangRoleNames
        BuildInitialCrossLangRoleNames();
        
        foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
        {
            if (File.Exists(@$"./{LANGUAGE_FOLDER_NAME}/{lang}.dat"))
            {
                Logger.Info($"Loading custom translation file from: {lang}.dat", "Translator");
                if (!ActualRoleNames.ContainsKey(lang))
                    ActualRoleNames.Add(lang, []);
                foreach (var role in CustomRolesHelper.AllRoles)
                {
                    if (ActualRoleNames[lang].ContainsKey(role))
                        ActualRoleNames[lang][role] = GetString(role.ToString(), lang);
                    else
                    {
                        ActualRoleNames[lang].Add(role, GetString(role.ToString(), lang));
                    }
                }
                UpdateCustomTranslation($"{lang}.dat"/*, lang*/);
                LoadCustomTranslation($"{lang}.dat", lang);
            }
        }

        // Load all custom translation role names into CrossLangRoleNames
        AttachCustomCrossLangRoleNames();
    }
    static void MergeJsonIntoTranslationMap(Dictionary<string, Dictionary<int, string>> translationMaps, int languageId, Dictionary<string, string> jsonDictionary)
    {
        foreach (var kvp in jsonDictionary)
        {
            string textString = kvp.Key;
            if (kvp.Value is string translation)
            {

                // If the textString is not already in the translation map, add it
                if (!translationMaps.ContainsKey(textString))
                {
                    translationMaps[textString] = [];
                }

                // Add or update the translation for the current id and textString
                translationMaps[textString][languageId] = translation.Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
    }

    // Function to get a list of JSON file names in a directory
    static string[] GetJsonFileNames(System.Reflection.Assembly assembly, string directoryName)
    {
        string[] resourceNames = assembly.GetManifestResourceNames();
        return resourceNames.Where(resourceName => resourceName.StartsWith(directoryName) && resourceName.EndsWith(".json")).ToArray();
    }

    public static void GetActualRoleName(this CustomRoles role, out string RealName)
    {
        var currentlang = TranslationController.Instance.currentLanguage.languageID;
        if (ActualRoleNames.TryGetValue(currentlang, out var RoleList))
        {
            if (RoleList.TryGetValue(role, out var RoleString))
                RealName = RoleString;
            else
            {
                RealName = GetString(role.ToString());
                Logger.Info($"Error while obtaining Rolename for LANG: {currentlang}/{role}", "Translator.GetActualRoleName");
            }
            return;
        }
        else
        {
            RealName = GetString(role.ToString());
        }
    }
    public static string GetActualRoleName(this CustomRoles role)
    {
        return ActualRoleNames.TryGetValue(TranslationController.Instance.currentLanguage.languageID, out var RoleList) && RoleList.TryGetValue(role, out var RoleString)
            ? RoleString
            : GetString($"{role}");
    }
    public static string GetString(string s, Dictionary<string, string> replacementDic = null, bool console = false, bool showInvalid = true, bool vanilla = false)
    {
        if (vanilla)
        {
            string nameToFind = s;
            if (Enum.TryParse(nameToFind, out StringNames text))
            {
                return DestroyableSingleton<TranslationController>.Instance.GetString(text);
            }
            else
            {
                return showInvalid ? $"<INVALID:{nameToFind}> (vanillaStr)" : nameToFind;
            }
        }
        var langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : SupportedLangs.English;
        if (console) langId = SupportedLangs.English;
        if (Main.ForceOwnLanguage.Value) langId = GetUserTrueLang();
        string str = GetString(s, langId, showInvalid);
        if (replacementDic != null)
            foreach (var rd in replacementDic)
            {
                str = str.Replace(rd.Key, rd.Value);
            }
        return str;
    }
    public static bool TryGetStrings(string strItem, out string[] s)
    {
        // Basically if you wanna let the user infinitely expand a function to their liking
        // I need to test if this shit works lol, I plan a usecase for it in 2.1.0 (see: https://discord.com/channels/1094344790910455908/1251264307052675134)

        var langId = TranslationController.InstanceExists ? TranslationController.Instance.currentLanguage.languageID : SupportedLangs.English;
        if (Main.ForceOwnLanguage.Value) langId = GetUserTrueLang();
        s = [""];

        try
        {

            var CaptureStr = translateMaps
                 .Where(x => x.Key.ToLower().Contains(strItem.ToLower()))
                 .ToDictionary(
                          x => x.Key,
                          x => x.Value
                          .Where(inner => inner.Key == (int)langId)
                          .Select(x => x.Value).ToArray()
                 );

            if (CaptureStr.Keys.Any())
            {
                List<string> strings = [];

                foreach (var melon in CaptureStr)
                {
                    var cache = GetString(melon.Key, langId);
                    Logger.Info($" Adding < {cache} > to the list of strings", "Translator.TryGetStrings");
                    strings.Add(cache);
                }
                s = [.. strings];
                return true;
            }
        }
        catch (Exception err)
        {
            Logger.Exception(err, "Translator.TryGetStrings");
        }

        return false;
    }

    public static string GetString(string str, SupportedLangs langId, bool showInvalid = true)
    {
        var res = showInvalid ? $"<INVALID:{str}>" : str;
        try
        {
            if (translateMaps.TryGetValue(str, out var dic))
            {
                if (!dic.TryGetValue((int)langId, out res) || res == "" || (langId is not SupportedLangs.SChinese and not SupportedLangs.TChinese && Regex.IsMatch(res, @"[\u4e00-\u9fa5]") && res == GetString(str, SupportedLangs.SChinese)))
                {
                    if (langId == SupportedLangs.English) res = $"*{str}";
                    else res = GetString(str, SupportedLangs.English);
                }
            }
            else
            {
                var stringNames = EnumHelper.GetAllValues<StringNames>().Where(x => x.ToString() == str).ToArray();
                if (stringNames != null && stringNames.Any())
                    res = GetString(stringNames.FirstOrDefault());
            }
        }
        catch (Exception Ex)
        {
            Logger.Fatal($"Error oucured at [{str}] in String.csv", "Translator");
            Logger.Error("Here was the error:\n" + Ex.ToString(), "Translator");
        }
        return res;
    }
    public static string GetString(StringNames stringName)
        => DestroyableSingleton<TranslationController>.Instance.GetString(stringName, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
    public static string GetRoleString(string str, bool forUser = true)
    {
        var CurrentLanguage = TranslationController.Instance.currentLanguage.languageID;
        var lang = forUser ? CurrentLanguage : SupportedLangs.English;
        if (Main.ForceOwnLanguageRoleName.Value)
            lang = GetUserTrueLang();

        return GetString(str, lang);
    }
    public static SupportedLangs GetUserTrueLang()
    {
        try
        {
            var name = CultureInfo.CurrentUICulture.Name;
            if (name.StartsWith("en")) return SupportedLangs.English;
            if (name.StartsWith("zh_CHT")) return SupportedLangs.TChinese;
            if (name.StartsWith("zh")) return SupportedLangs.SChinese;
            if (name.StartsWith("ru")) return SupportedLangs.Russian;
            return TranslationController.Instance.currentLanguage.languageID;
        }
        catch
        {
            return SupportedLangs.English;
        }
    }
    static void UpdateCustomTranslation(string filename/*, SupportedLangs lang*/)
    {
        string path = @$"./{LANGUAGE_FOLDER_NAME}/{filename}";
        if (File.Exists(path))
        {
            Logger.Info("Updating Custom Translations", "UpdateCustomTranslation");
            try
            {
                List<string> textStrings = [];
                using (StreamReader reader = new(path, Encoding.GetEncoding("UTF-8")))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Split the line by ':' to get the first part
                        string[] parts = line.Split(':');

                        // Check if there is at least one part before ':'
                        if (parts.Length >= 1)
                        {
                            // Trim any leading or trailing spaces and add it to the list
                            string textString = parts[0].Trim();
                            textStrings.Add(textString);
                        }
                    }
                }
                var sb = new StringBuilder();
                foreach (var templateString in translateMaps.Keys)
                {
                    if (!textStrings.Contains(templateString)) sb.Append($"{templateString}:\n");
                }

                using FileStream fileStream = new(path, FileMode.Append, FileAccess.Write);
                using StreamWriter writer = new(fileStream);

                writer.WriteLine(sb.ToString());

            }
            catch (Exception e)
            {
                Logger.Error("An error occurred: " + e.Message, "Translator");
            }
        }
    }
    public static void LoadCustomTranslation(string filename, SupportedLangs lang)
    {
        string path = @$"./{LANGUAGE_FOLDER_NAME}/{filename}";
        if (File.Exists(path))
        {
            Logger.Info($"加载自定义翻译文件：{filename}", "LoadCustomTranslation");
            using StreamReader sr = new(path, Encoding.GetEncoding("UTF-8"));
            string text;
            string[] tmp = [];
            while ((text = sr.ReadLine()) != null)
            {
                tmp = text.Split(":");
                if (tmp.Length > 1 && tmp[1] != "")
                {
                    try
                    {
                        translateMaps[tmp[0]][(int)lang] = tmp.Skip(1).Join(delimiter: ":").Replace("\\n", "\n").Replace("\\r", "\r");
                    }
                    catch (KeyNotFoundException)
                    {
                        Logger.Warn($"无效密钥：{tmp[0]}", "LoadCustomTranslation");
                    }
                }
            }
        }
        else
        {
            Logger.Error($"找不到自定义翻译文件：{filename}", "LoadCustomTranslation");
        }
    }

    private static void CreateTemplateFile()
    {
        var sb = new StringBuilder();
        foreach (var title in translateMaps) sb.Append($"{title.Key}:\n");
        File.WriteAllText(@$"./{LANGUAGE_FOLDER_NAME}/template.dat", sb.ToString());
    }
    public static void ExportCustomTranslation()
    {
        LoadLangs();
        var sb = new StringBuilder();
        var lang = TranslationController.Instance.currentLanguage.languageID;
        foreach (var title in translateMaps)
        {
            var text = title.Value.GetValueOrDefault((int)lang, string.Empty);
            sb.Append($"{title.Key}:{text.Replace("\n", "\\n").Replace("\r", "\\r")}\n");
        }
        File.WriteAllText(@$"./{LANGUAGE_FOLDER_NAME}/export_{lang}.dat", sb.ToString());
    }
    
    private static void BuildInitialCrossLangRoleNames()
    {
        // Runs before Load Custom Translations to get all vanilla texts
        foreach (var role in CustomRolesHelper.AllRoles)
        {
            if (!CrossLangRoleNames.ContainsKey(role))
            {
                CrossLangRoleNames.Add(role, []);
            }
            else
            {
                continue;
            }

            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
            {
                var name = GetString($"{role}", lang).ToLower().Trim().Replace(" ", string.Empty);
                if (!CrossLangRoleNames[role].Contains(name))
                {
                    CrossLangRoleNames[role].Add(name);
                }
            }
        }
    }

    private static void AttachCustomCrossLangRoleNames()
    {
        // Add custom role names to the cross-lang role names
        // Sort and Remove Invaild
        foreach (var item in CrossLangRoleNames)
        {
            foreach (var lang in EnumHelper.GetAllValues<SupportedLangs>())
            {
                var name = GetString($"{item}", lang).ToLower().Trim().Replace(" ", string.Empty);
                if (!CrossLangRoleNames[item.Key].Contains(name))
                {
                    CrossLangRoleNames[item.Key].Add(name);
                }
            }

            item.Value.Where(x => x.Contains("<INVALID:".ToLower())).ToList().ForEach(x => item.Value.Remove(x));
        }
    }
}
