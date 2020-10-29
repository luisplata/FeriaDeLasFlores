using System.Collections.Generic;
using UnityEngine;

/* 
 * 
 */
public class FlorDelMapa : MonoBehaviour
{
    [SerializeField] private Sprite florUI;

    public Sprite FlorUi()
    {
        return florUI;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent(out ControladorDePuntuacion puncuation))
            {
                puncuation.AumentoDePuntuacion(this);
                AudioManager.Play(AudioClipName.FlowerPickup);
                Destroy(gameObject);
            }
        }
    }

    public override string ToString()
    {
        return "El material es " + gameObject.GetComponent<MeshRenderer>().material.name + " ";
    }
}
