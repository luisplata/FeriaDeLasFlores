using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverHUD : MonoBehaviour
{
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text flowerScore;
    [SerializeField] private Text timeScore;
    [SerializeField] private Button backButton;

    void Start()
    {
        gameOverText.text = ConfigurationUtils.CollectedFlowers == ConfigurationUtils.TotalFlowers ?
            "Has ganado!" : "Has perdido :c";
        flowerScore.text = $"recogiste {ConfigurationUtils.CollectedFlowers} / {ConfigurationUtils.TotalFlowers} flores";
        timeScore.text = $"tiempo jugado: {ConfigurationUtils.PlaySeconds} segundos";
        backButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); });
    }
}
