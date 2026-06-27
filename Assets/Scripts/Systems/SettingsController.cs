using UnityEngine;
using UnityEngine.UI;

namespace SurvivalMoba.Systems
{
    /// <summary>
    /// Minimal persistent settings: music/SFX volume and graphics quality.
    /// Values are stored in PlayerPrefs and applied on load. Hook the public
    /// setters to UI sliders/dropdowns; the rest of the menu list in the design
    /// doc (joystick size, damage numbers, screen shake) can extend this later.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        private const string MusicKey = "settings_music";
        private const string SfxKey = "settings_sfx";
        private const string QualityKey = "settings_quality";

        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private AudioSource musicSource;

        public float MusicVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 0.9f;

        private void Awake()
        {
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, 0.8f);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, 0.9f);
            int quality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());

            ApplyMusic();
            ApplyQuality(quality);

            if (musicSlider != null) { musicSlider.value = MusicVolume; musicSlider.onValueChanged.AddListener(SetMusic); }
            if (sfxSlider != null) { sfxSlider.value = SfxVolume; sfxSlider.onValueChanged.AddListener(SetSfx); }
            if (qualityDropdown != null) { qualityDropdown.value = quality; qualityDropdown.onValueChanged.AddListener(SetQuality); }
        }

        public void SetMusic(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            ApplyMusic();
        }

        public void SetSfx(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
        }

        public void SetQuality(int level)
        {
            PlayerPrefs.SetInt(QualityKey, level);
            ApplyQuality(level);
        }

        private void ApplyMusic()
        {
            if (musicSource != null) musicSource.volume = MusicVolume;
        }

        private void ApplyQuality(int level)
        {
            QualitySettings.SetQualityLevel(Mathf.Clamp(level, 0, QualitySettings.names.Length - 1), true);
        }
    }
}
