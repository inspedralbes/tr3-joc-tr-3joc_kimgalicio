using UnityEngine;

public class AudioListenerFixer : MonoBehaviour
{
    private void Start()
    {

        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

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
