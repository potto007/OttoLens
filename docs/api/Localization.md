# Localization Type Analysis

**Source**: assembly_guiutils.dll (Vanilla Valheim 1.0.7)  
**Namespace**: Global (no namespace)  
**Type**: Public class

## Required Members

### Instance
- **Name**: `instance`
- **Declaration**: `public static Localization instance { get; }` (line 177-187)
- **Kind**: Property
- **Signature**: Auto-property with getter that initializes via `Initialize()` if null

### Localize(string)
- **Name**: `Localize`
- **Declaration**: `public string Localize(string text)` (line 388-417)
- **Kind**: Method
- **Signature**: `public string Localize(string text)`
- **Behavior**: Finds words prefixed with `$`, translates them, caches results. Returns empty string, "MISSING KEY", or "MISSING BUTTON" strings without caching.

## All Members

### Fields
- Line 157: `private Dictionary<Text, string> textStrings`
- Line 159: `private Dictionary<TMP_Text, string> textMeshStrings`
- Line 161: `private readonly StringBuilder m_stringBuilder`
- Line 163: `private static Localization m_instance`
- Line 165: `private static LocalizationSettings m_localizationSettings`
- Line 167: `public static Action OnLanguageChange`
- Line 169: `private char[] m_endChars`
- Line 171: `private Dictionary<string, string> m_translations`
- Line 173: `private List<string> m_languages`
- Line 175: `private readonly LRUCache<string> m_cache`

### Nested Type
- Line 13-155: `internal static class LocalizationConstants` (language string constants and BCP47 mapping dictionary)

### Properties
- Line 177-187: `public static Localization instance { get; }`

### Methods
- Line 189-196: `private static void Initialize()`
- Line 198-204: `private Localization()` (constructor)
- Line 206-218: `private void SetStartupLanguage()`
- Line 220-241: `private void SetLanguageFromLocale()`
- Line 243-252: `public void SetLanguage(string language)`
- Line 254-257: `public string GetSelectedLanguage()`
- Line 259-273: `public string GetNextLanguage(string lang)`
- Line 275-289: `public string GetPrevLanguage(string lang)`
- Line 291-313: `public void Localize(Transform root)`
- Line 315-321: `public void RemoveTextFromCache(Text text)`
- Line 323-329: `public void RemoveTextFromCache(TMP_Text text)`
- Line 331-349: `public void ReLocalizeVisible(Transform root)`
- Line 351-370: `public void ReLocalizeAll(Transform root)`
- Line 372-376: `public string Localize(string text, params string[] words)`
- Line 378-386: `private string InsertWords(string text, string[] words)`
- Line 388-417: `public string Localize(string text)`
- Line 419-447: `private bool FindNextWord(string text, int startIndex, out string word, out int wordStart, out int wordEnd)`
- Line 449-469: `private string Translate(string word)`
- Line 471-479: `public string GetBoundKeyString(string bindingName, bool emptyStringOnMissing = false)`
- Line 481-485: `private void AddWord(string key, string text)`
- Line 487-491: `private void Clear()`
- Line 493-504: `private string StripCitations(string s)`
- Line 506-523: `public bool SetupLanguage(string language)`
- Line 525-565: `public bool LoadCSV(TextAsset file, string language)`
- Line 567-614: `private List<List<string>> DoQuoteLineSplit(StringReader reader)`
- Line 616-653: `public string TranslateSingleId(string locaId, string language)`
- Line 655-658: `public List<string> GetLanguages()`
- Line 660-670: `private List<string> LoadLanguages()`
- Line 672-679: `private static bool IsConsolePlatform()`
- Line 681-688: `private static bool IsLanguageSupported(string language)`
- Line 690-693: `public static bool IsLanguageProfessionallyTranslated(string language)`

## ZDO Key Access

**None found.** The Localization type does not read or write any ZDOVars.s_* hashes or ZDO string keys. No GetFloat, GetInt, GetLong, GetString, GetBool, or Set calls to ZDO are present in this type.

## Missing Members

None requested.
