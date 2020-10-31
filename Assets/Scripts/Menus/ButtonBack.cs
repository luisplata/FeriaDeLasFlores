using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonBack : MonoBehaviour
{
    [SerializeField] private Button instructionsButton;
    // Start is called before the first frame update
    void Start()
    {
        instructionsButton.onClick.AddListener(delegate { SceneManager.LoadScene(0); });
    }
}
