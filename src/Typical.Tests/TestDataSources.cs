namespace Typical.Tests;

public static class TestDataSources
{
    public static IEnumerable<Func<string>> AdditionTestData()
    {
        yield return () => "af_ZA";
        yield return () => "fr_CH";
        yield return () => "ar";
        yield return () => "ge";
        yield return () => "az";
        yield return () => "hr";
        yield return () => "cz";
        yield return () => "id_ID";
        yield return () => "de";
        yield return () => "it";
        yield return () => "de_AT";
        yield return () => "ja";
        yield return () => "de_CH";
        yield return () => "ko";
        yield return () => "el";
        yield return () => "lv";
        yield return () => "en";
        yield return () => "nb_NO";
        yield return () => "en_AU";
        yield return () => "ne";
        yield return () => "en_AU_ocker";
        yield return () => "nl";
        yield return () => "en_BORK";
        yield return () => "nl_BE";
        yield return () => "en_CA";
        yield return () => "pl";
        yield return () => "en_GB";
        yield return () => "pt_BR";
        yield return () => "en_IE";
        yield return () => "pt_PT";
        yield return () => "en_IND";
        yield return () => "ro";
        yield return () => "en_NG";
        yield return () => "ru";
        yield return () => "en_US";
        yield return () => "sk";
        yield return () => "en_ZA";
        yield return () => "sv";
        yield return () => "es";
        yield return () => "tr";
        yield return () => "es_MX";
        yield return () => "uk";
        yield return () => "fa";
        yield return () => "vi";
        yield return () => "fi";
        yield return () => "zh_CN";
        yield return () => "fr";
        yield return () => "zh_TW";
        yield return () => "fr_CA";
        yield return () => "zu_ZA";
    }
}
