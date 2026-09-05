using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private bool dead;
        public int CurrentHealth { get; private set; }
        private void Awake() { CurrentHealth = maxHealth; dead = false; }
        public void TakeDamage(int amount)
        {
            if (dead) return;
            CurrentHealth -= amount;
            if (CurrentHealth < 0) CurrentHealth = 0;
            GameEvents.RaisePlayerDamaged(amount);
            if (CurrentHealth <= 0)
            {
                dead = true;
                GameEvents.RaisePlayerDied();
                gameObject.SetActive(false);
            }
        }
    }
}