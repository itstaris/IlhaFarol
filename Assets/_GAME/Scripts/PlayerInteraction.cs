using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para mudar de cena

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações de mudança de cena")]
    public string targetSceneName; // Nome da cena para mudar
    private bool canChangeScene = false; // Se o player está colidindo com o objeto certo

    // Detecta quando o player entra na área do trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Verifica se é o player
        {
            canChangeScene = true;
            Debug.Log("entrou na colisão");
        }
    }

    // Detecta quando o player sai da área do trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canChangeScene = false;
            Debug.Log("saiu da colisão");
        }
    }

    // Checa se o jogador pode mudar de cena e se clicou com o mouse
    private void Update()
    {
        if (canChangeScene && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
