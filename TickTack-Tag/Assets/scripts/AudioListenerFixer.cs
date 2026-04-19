using UnityEngine;

/// <summary>
/// Evita que hi hagi múltiples AudioListeners actius alhora,
/// cosa que provoca warnings a la consola d'Unity en carregar noves escenes.
/// </summary>
public class AudioListenerFixer : MonoBehaviour
{
    private void Start()
    {
        // Busquem tots els AudioListeners de l'escena
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        // Si en trobem més d'un, aquest (que s'acaba de carregar) es desactiva
        if (listeners.Length > 1)
        {
            AudioListener myListener = GetComponent<AudioListener>();
            if (myListener != null)
            {
                myListener.enabled = false;
                Debug.Log($"[AudioListenerFixer] S'ha desactivat un AudioListener redundant a {gameObject.name} per evitar soroll a la consola.");
            }
        }
    }
}
