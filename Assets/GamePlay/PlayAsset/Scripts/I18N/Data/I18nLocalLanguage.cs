using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace I18n
{
    public static class I18nLocalLanguage
    {
        static Dictionary<LanguageType, string> I18nLanguageName = new Dictionary<LanguageType, string>{
            {LanguageType.English ,"I18nLanguageData_English"},
            {LanguageType.Japanese ,"I18nLanguageData_Japanese"},
            {LanguageType.Russian ,"I18nLanguageData_Russian"},
            {LanguageType.Korean ,"I18nLanguageData_Korean"},
        };

        private static I18nLanguageData currentI18nLanguageData = null;

        private static void InitI18NLanguageData()
        {
            if (currentI18nLanguageData == null)
            {
                LanguageType languageType = LanguageType.English;

                var sysLang = Application.systemLanguage;
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    if ((int)language == (int)sysLang)
                    {
                        languageType = language;
                        break;
                    }
                }

                languageType = LanguageType.English;

                currentI18nLanguageData = Resources.Load<I18nLanguageData>(I18nLanguageName[languageType]);
            }
        }

        public static I18nLanguageData GetI18NLanguageData()
        {
            InitI18NLanguageData();
            return currentI18nLanguageData;
        }

        public static string GetI18NLanguageStr(string key)
        {
            InitI18NLanguageData();
            return currentI18nLanguageData.GetLanguageWords(key);
        }

    }
}