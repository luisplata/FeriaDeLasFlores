using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using V2;

public class ConfiguracionDeEscena : MonoBehaviour
{
    [SerializeField] private GameObject StartPanel;
    [SerializeField] private Button StartButton;
    [SerializeField] private GameObject EndPanel;
    [SerializeField] private Button EndButton;
    [SerializeField] private FloorManager floorManager;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private int maxCoins;
    private TeaTime configuracion;
    private bool isStart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnStateChanged.AddListener(StateChanged);
        GameManager.Instance.OnGameEnded.AddListener(GameEnded);
        EndButton.onClick.AddListener(() => { SceneManager.LoadScene(0); });
        StartButton.onClick.AddListener(() => { isStart = true; });
        StartPanel.SetActive(true);
        EndPanel.SetActive(false);
        coinText.text = "0/" + maxCoins;
        floorManager.CoinSpawner.OnCoinCollected += (coinCount) =>
        {
            coinText.text = coinCount + "/" + maxCoins;
            if (coinCount >= maxCoins)
            {
                GameManager.Instance.EndGame(GameManager.EndReason.Ganar);
            }
        };
        StartButton.onClick.AddListener(() => { isStart = true; });
        configuracion = this.tt().Pause()
            .Add(0.2f, () => { GameManager.Instance.ConfigurarCompletada(); })
            .Wait(() => isStart)
            .Add(() =>
            {
                StartPanel.SetActive(false);
                EndPanel.SetActive(false);
                floorManager.Configure();
                GameManager.Instance.StartGame();
            });

        configuracion.Play();
    }

    private void GameEnded(GameManager.EndReason arg0)
    {
        EndPanel.SetActive(true);
    }

    private void StateChanged(GameManager.GameState lastState, GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.FinDelJuego)
        {
            configuracion.Pause();
            floorManager.StopAll();
        }
    }
}