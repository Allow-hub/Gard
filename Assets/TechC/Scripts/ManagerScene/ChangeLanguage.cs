using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

namespace TechC
{
    public class ChangeLanguage : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown_tmp; // 画面上のドロップダウン

        // ドロップダウンの値が変更された時のイベントから呼び出す用
        public void ChangeSelected()
        {
            ChangeLang();
        }

        // 実際に使用言語を変更する処理
        private async Task ChangeLang()
        {
            // このまま使う場合、case内の言語指定とドロップダウン内の項目順は合わせておくこと
            switch (dropdown_tmp.value) // Legacy版ならdropdown_lgc.valueを指定
            {
                case 0: // 日本語
                    LocalizationSettings.SelectedLocale = Locale.CreateLocale("ja-JP");
                    break;
                case 1: // 英語
                    LocalizationSettings.SelectedLocale = Locale.CreateLocale("en");
                    break;
                case 2: // アゼルバイジャン
                    LocalizationSettings.SelectedLocale = Locale.CreateLocale("az");
                    break;
                case 3: // 中国語(繁体字)
                    LocalizationSettings.SelectedLocale = Locale.CreateLocale("zh-TW");
                    break;
            }
            await LocalizationSettings.InitializationOperation.Task;
        }
    }
}
