using TMPro;
using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text enemyCountText;
        [SerializeField] private TMP_Text timerText;

        private int lastHealth = int.MinValue;
        private int lastScore = int.MinValue;
        private int lastEnemies = int.MinValue;
        private int lastTenths = int.MinValue;

        public void Refresh(int score, int health, int enemies, float elapsed)
        {
            if (health != lastHealth)
            {
                lastHealth = health;
                healthText.SetText("HP: {0}", health);
            }

            if (score != lastScore)
            {
                lastScore = score;
                scoreText.SetText("Score: {0}", score);
            }

            if (enemies != lastEnemies)
            {
                lastEnemies = enemies;
                enemyCountText.SetText("Enemies: {0}", enemies);
            }

            int tenths = (int)(elapsed * 10f);

            if (tenths != lastTenths)
            {
                lastTenths = tenths;
                timerText.SetText("Time: {0:1}", elapsed);
            }
        }
    }
}