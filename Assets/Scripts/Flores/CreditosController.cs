using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CreditosController : MonoBehaviour
{

    public TextMeshProUGUI score, nombre, ranking;
    public TMP_InputField nombreDeJugador;
    public GameObject button, jugar;
    [SerializeField] private string nombreDeJuego;
    [SerializeField] private string endPoint;
    [SerializeField] private GameOverHUD contro;

    // Start is called before the first frame update
    void Start()
    {
        CrearScore();
        button.SetActive(contro.GanoElPlayer);
        jugar.SetActive(!contro.GanoElPlayer);
    }


    public void CrearScore()
    {
        score.text = "Cargando";
        nombre.text = "...";
        ranking.text = "";
        StartCoroutine(GetRequest(endPoint + "score/best/"+nombreDeJuego+"/10/0"));
    }

    public void RegistrarNombre()
    {
        StartCoroutine(Upload());
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {

                ranking.text = "Error";
                score.text = "En El";
                nombre.text = "Servidor";
            }
            else
            {
                score.text = "";
                nombre.text = "";
                ranking.text = "";
                Debug.Log(")))))" + webRequest.downloadHandler.text);
                Score[] s = JsonHelper.FromJson<Score>(webRequest.downloadHandler.text);
                int count = 1;
                foreach (Score sco in s)
                {
                    ranking.text += count + "\n";
                    score.text += sco.score + "\n";
                    nombre.text += sco.nombre + "\n";
                    Debug.Log(sco.nombre + " => " + sco.score);
                    count++;
                }
            }
        }
    }
    IEnumerator Upload()
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombreDeJugador.text);
        int score;
        //lo sacamos de las preferencias
        if (!PlayerPrefs.HasKey("Score"))
        {
            score = 0;
        }
        else
        {
            score = PlayerPrefs.GetInt("Score");
        }
        form.AddField("score", score);

        using (UnityWebRequest www = UnityWebRequest.Post(endPoint + "guardarData/"+nombreDeJuego, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                CrearScore();
                PlayerPrefs.SetInt("Score", 0);
                button.SetActive(!contro.GanoElPlayer);
                jugar.SetActive(contro.GanoElPlayer);
            }
        }
    }

    public void JugarDeNuevo()
    {
        SceneManager.LoadScene(0);
    }
}
