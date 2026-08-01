namespace V2
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Administrador simple de estados del juego para un runner infinito.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static GameManager Instance { get; private set; }

        // ---- Enumeraciones ----
        public enum GameState
        {
            Configuracion, // Configuración inicial
            PausaParaIniciar, // Esperando a que el jugador inicie
            Jugar, // Jugando
            Pausa, // Pausado
            FinDelJuego // Juego terminado (por Game Over o Victoria)
        }

        public enum EndReason
        {
            None,
            GameOver,
            Ganar
        }

        // ---- Variables públicas ----
        [Header("Estado actual")] [SerializeField]
        private GameState currentState = GameState.Configuracion;

        [SerializeField] private EndReason currentEndReason = EndReason.None;

        // ---- Eventos ----
        public UnityEvent<GameState, GameState> OnStateChanged; // (estado anterior, nuevo estado)
        public UnityEvent<EndReason> OnGameEnded; // Se dispara cuando entra en FinDelJuego

        // ---- Propiedades ----
        public GameState CurrentState => currentState;
        public EndReason CurrentEndReason => currentEndReason;

        // ---- Métodos de Unity ----
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Iniciamos en Configuración
            SetState(GameState.Configuracion);
        }

        // ---- Métodos públicos para transiciones ----

        /// <summary>
        /// Cambia al estado de "PausaParaIniciar" (después de la configuración).
        /// </summary>
        public void ConfigurarCompletada()
        {
            if (currentState == GameState.Configuracion)
                SetState(GameState.PausaParaIniciar);
            else
                Debug.LogWarning("No se puede pasar a PausaParaIniciar desde " + currentState);
        }

        /// <summary>
        /// Inicia la partida (de PausaParaIniciar a Jugar).
        /// </summary>
        public void StartGame()
        {
            if (currentState == GameState.PausaParaIniciar || currentState == GameState.FinDelJuego)
                SetState(GameState.Jugar);
            else
                Debug.LogWarning("No se puede iniciar desde " + currentState);
        }

        /// <summary>
        /// Pausa el juego (de Jugar a Pausa).
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Jugar)
                SetState(GameState.Pausa);
            else
                Debug.LogWarning("No se puede pausar desde " + currentState);
        }

        /// <summary>
        /// Reanuda el juego (de Pausa a Jugar).
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Pausa)
                SetState(GameState.Jugar);
            else
                Debug.LogWarning("No se puede reanudar desde " + currentState);
        }

        /// <summary>
        /// Termina el juego por Game Over.
        /// </summary>
        public void GameOver()
        {
            if (currentState == GameState.Jugar || currentState == GameState.Pausa)
                EndGame(EndReason.GameOver);
            else
                Debug.LogWarning("No se puede terminar por Game Over desde " + currentState);
        }

        /// <summary>
        /// Termina el juego por Victoria.
        /// </summary>
        public void Win()
        {
            if (currentState == GameState.Jugar || currentState == GameState.Pausa)
                EndGame(EndReason.Ganar);
            else
                Debug.LogWarning("No se puede terminar por Victoria desde " + currentState);
        }

        /// <summary>
        /// Reinicia el juego al estado inicial (Configuración).
        /// </summary>
        public void ResetGame()
        {
            currentEndReason = EndReason.None;
            SetState(GameState.Configuracion);
        }

        // ---- Métodos privados ----

        private void SetState(GameState newState)
        {
            if (currentState == newState) return;

            GameState oldState = currentState;
            currentState = newState;

            // Disparar evento
            OnStateChanged?.Invoke(oldState, newState);

            // Si el nuevo estado es FinDelJuego, disparar evento adicional
            if (newState == GameState.FinDelJuego)
            {
                OnGameEnded?.Invoke(currentEndReason);
            }
        }

        public void EndGame(EndReason reason)
        {
            currentEndReason = reason;
            SetState(GameState.FinDelJuego);
        }

        // ---- Métodos de utilidad ----

        public bool IsPlaying() => currentState == GameState.Jugar;
        public bool IsPaused() => currentState == GameState.Pausa;
        public bool IsGameOver() => currentState == GameState.FinDelJuego && currentEndReason == EndReason.GameOver;
        public bool IsWin() => currentState == GameState.FinDelJuego && currentEndReason == EndReason.Ganar;
    }
}