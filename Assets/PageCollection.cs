using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PageCollection : MonoBehaviour
{
    private int Page = 0;
    public TextMeshProUGUI pageText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Page"))
        {
            Page++;
            pageText.text = "Pages: " + Page;
            Destroy(other.gameObject);
            Debug.Log("Páginas coletadas: " + Page);
        }
    }

}
