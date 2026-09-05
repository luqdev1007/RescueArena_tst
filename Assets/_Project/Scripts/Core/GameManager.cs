using UnityEngine;

namespace WebGLRescueArena
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private HUDController hud;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private bool stressMode;
        private SaveService saveService;
        private SceneLoader sceneLoader;
        private int score;
        private float elapsedTime;
        private bool ended;
        public bool StressMode => stressMode;
        private void Awake()
        {
            saveService = FindFirstObjectByType<SaveService>();
            sceneLoader = FindFirstObjectByType<SceneLoader>();
            if (stressMode) enemySpawner.EnableStressMode();
        }
        private void OnEnable() { GameEvents.EnemyKilled += OnEnemyKilled; GameEvents.PlayerDied += OnPlayerDied; }
        private void OnDisable() { GameEvents.EnemyKilled -= OnEnemyKilled; GameEvents.PlayerDied -= OnPlayerDied; }
        private void Start() { score = 0; gameOverUI.Hide(); GameEvents.RaiseGameStarted(); }
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            hud.Refresh(score, playerHealth.CurrentHealth, enemySpawner.ActiveEnemyCount, elapsedTime);
            if (Input.GetKeyDown(KeyCode.F8)) enemySpawner.EnableStressMode();
        }
        private void OnEnemyKilled(int value) { score += value; GameEvents.RaiseScoreChanged(score); }
        private void OnPlayerDied()
        {
            if (ended) return;
            ended = true;
            saveService.SaveBestScore(score);
            gameOverUI.Show(score, saveService.BestScore);
            GameEvents.RaiseGameEnded();
        }
        public void Restart() => sceneLoader.RestartGame();
        public void ReturnToMenu() => sceneLoader.LoadMainMenu();
    }
}