using System.Collections.Generic;
using UnityEngine;

/* 
 * 
 */
public class FlorDelMapa : MonoBehaviour
{
    /// <summary>
    /// Event raised when this flower is collected by the player.
    /// </summary>
    public System.Action<FlorDelMapa> OnCollected;

    [SerializeField] private Sprite florUI;

    public Sprite FlorUi()
    {
        return florUI;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent(out ControladorDePuntuacion puncuation))
            {
                OnCollected?.Invoke(this);
                puncuation.AumentoDePuntuacion(this);
                AudioManager.Play(AudioClipName.FlowerPickup);
                gameObject.SetActive(false);
            }
        }
    }

    public override string ToString()
    {
        return "El material es " + gameObject.GetComponent<MeshRenderer>().material.name + " ";
    }
}
