using UnityEngine;
using UnityEngine.Events;

public class WorldMover : MonoBehaviour
{
    [Header("Movimiento")] public float speed = 10f; // Velocidad de movimiento
    public Vector3 direction = Vector3.back; // Dirección (por defecto -Z)
    public bool isMoving; // Permite pausar/reanudar

    [Header("Referencia")] public Transform referencePoint; // Si es null, se busca al Player

    [Header("Evento")] public UnityEvent OnPassedBehind; // Se dispara cuando pasa detrás del punto de referencia

    private float thresholdDistance = 5f; // Distancia detrás para considerar "pasado"

    void Start()
    {
        // Si no se asignó referencia, buscamos al jugador por tag
        if (referencePoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                referencePoint = player.transform;
            else
                Debug.LogWarning("WorldMover en " + gameObject.name +
                                 ": no se encontró referencia. Asigna manualmente.");
        }

        direction.Normalize();
    }

    void Update()
    {
        if (!isMoving) return;

        // Movimiento en dirección global
        transform.Translate(direction * (speed * Time.deltaTime), Space.World);

        // Comprobar si el objeto ya pasó detrás del punto de referencia
        if (referencePoint != null)
        {
            Vector3 dir = direction.normalized;
            float dot = Vector3.Dot(transform.position - referencePoint.position, dir);
            if (dot < -thresholdDistance)
            {
                OnPassedBehind?.Invoke();
                // No detenemos ni desactivamos aquí; dejamos que el listener decida.
                // Si quieres que se detenga al pasar, puedes añadir: isMoving = false;
            }
        }
    }

    /// <summary>
    /// Pausa o reanuda el movimiento.
    /// </summary>
    public void SetMoving(bool moving) => isMoving = moving;
}