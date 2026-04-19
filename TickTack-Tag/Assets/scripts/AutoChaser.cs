using UnityEngine;

public class AutoChaser : MonoBehaviour
{
    public Transform objetivoAmarillo;
    public float velocidad = 2.5f;

    void Update()
    {
        if (objetivoAmarillo != null)
        {
            // Se mueve como un zombi hacia el amarillo
            transform.position = Vector2.MoveTowards(transform.position, objetivoAmarillo.position, velocidad * Time.deltaTime);
        }
    }
}