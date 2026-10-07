using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace I18n
{
    public enum LanguageType
    {
        English = SystemLanguage.English,
        Japanese = SystemLanguage.Japanese,
        Russian = SystemLanguage.Russian,
        Korean = SystemLanguage.Korean,
    }

    [Serializable]
    public class TermData
    {
        public string key;
        public string Languages;
    }

    [CreateAssetMenu(fileName = "I18nLanguageData", menuName = "I18n Localization/I18nLanguageData")]
    public class I18nLanguageData : ScriptableObject
    {
        public LanguageType languageType;

        public List<TermData> mTerms;
        [NonSerialized]
        public Dictionary<string, string> mDictionary = null;

        private void InitDic()
        {
            if (mDictionary == null)
            {
                mDictionary = new Dictionary<string, string>();
                foreach (var it in mTerms)
                {
                    mDictionary.Add(it.key, it.Languages);
                }
            }
        }
        public string GetLanguageWords(string key)
        {
            InitDic();
            if (mDictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return key;
        }
    }
}
