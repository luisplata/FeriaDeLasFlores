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
    [SerializeField] private List<GameObject> cosasParaActivar, cosasParaDesactivar;

    public bool GanoElPlayer => ConfigurationUtils.CollectedFlowers == ConfigurationUtils.TotalFlowers;

    void Start()
    {
        if(GanoElPlayer)
        {
            //ganaste
            gameOverText.text = "Has ganado! \ndeja tu nombre para la historia!";
            foreach(GameObject cosa in cosasParaActivar)
            {
                cosa.SetActive(true);
            }
            foreach (GameObject cosa in cosasParaDesactivar)
            {
                cosa.SetActive(false);
            }
            PlayerPrefs.SetInt("Score", ConfigurationUtils.PlaySeconds);
        }
        else
        {
            //perdiste
            gameOverText.text = "Has perdido\nintenta ganar para dejar tu nombre para la historia!";
        }
        flowerScore.text = $"recogiste {ConfigurationUtils.CollectedFlowers} / {ConfigurationUtils.TotalFlowers} flores";
        timeScore.text = $"tiempo jugado: {ConfigurationUtils.PlaySeconds} segundos";
    }
}
