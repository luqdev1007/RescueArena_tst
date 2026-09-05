using UnityEngine;
using UnityEngine.UI;

namespace WebGLRescueArena
{
    public sealed class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private SaveService saveService;
        [SerializeField] private AudioManager audioManager;
        private void OnEnable()
        {
            if (saveService == null) saveService = FindFirstObjectByType<SaveService>();
            if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>();
            if (saveService == null) return;
            if (musicToggle != null) musicToggle.isOn = saveService.MusicEnabled;
            if (sfxToggle != null) sfxToggle.isOn = saveService.SfxEnabled;
        }
        public void SetMusic(bool enabled)
        {
            if (saveService != null) saveService.SetMusicEnabled(enabled);
            if (audioManager != null) audioManager.SetMusic(enabled);
        }
        public void SetSfx(bool enabled)
        {
            if (saveService != null) saveService.SetSfxEnabled(enabled);
            if (audioManager != null) audioManager.SetSfx(enabled);
        }
    }
}