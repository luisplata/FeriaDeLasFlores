using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MobileInputManager : MonoBehaviour
{
    [Header("Configuración de deslizamiento")]
    public float minSwipeDistance = 50f;      // Distancia mínima en píxeles
    public float maxSwipeTime = 0.5f;          // Tiempo máximo para considerar swipe

    [Header("Eventos (direcciones)")]
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;

    // Variables para el swipe actual
    private Vector2 fingerStartPos;
    private float fingerStartTime;
    private bool isSwiping = false;

    private void OnEnable()
    {
        // Habilitar el sistema de toques mejorados (para usar EnhancedTouch)
        EnhancedTouchSupport.Enable();
        // Suscribirse al evento de toque
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        // Desuscribirse y deshabilitar EnhancedTouch
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void OnFingerDown(Finger finger)
    {
        // Solo consideramos el primer dedo (puedes cambiar si quieres multi-touch)
        if (finger.index == 0)
        {
            fingerStartPos = finger.screenPosition;
            fingerStartTime = Time.time;
            isSwiping = true;
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger.index == 0 && isSwiping)
        {
            Vector2 fingerEndPos = finger.screenPosition;
            float swipeDistance = (fingerEndPos - fingerStartPos).magnitude;
            float swipeTime = Time.time - fingerStartTime;

            if (swipeDistance >= minSwipeDistance && swipeTime <= maxSwipeTime)
            {
                Vector2 direction = (fingerEndPos - fingerStartPos).normalized;

                // Determinar dirección dominante
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    if (direction.x > 0)
                        OnSwipeRight?.Invoke();
                    else
                        OnSwipeLeft?.Invoke();
                }
                else
                {
                    if (direction.y > 0)
                        OnSwipeUp?.Invoke();
                    else
                        OnSwipeDown?.Invoke();
                }
            }

            isSwiping = false;
        }
    }

    // Simulación con teclado (opcional, usando el Input System)
    // Si quieres usar el teclado con el nuevo Input System, puedes añadir un InputAction para las flechas.
    // Pero aquí lo dejamos como opción rápida con los eventos del script.
    private void Update()
    {
        // Simulación con teclado usando el nuevo Input System (opcional)
        // Puedes usar Keyboard.current para leer las teclas.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame) OnSwipeLeft?.Invoke();
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) OnSwipeRight?.Invoke();
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) OnSwipeUp?.Invoke();
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) OnSwipeDown?.Invoke();
        }
    }
}