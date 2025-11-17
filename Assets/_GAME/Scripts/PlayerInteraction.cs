using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask itemLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Botão esquerdo
        {
            TryPickupItem();
        }
    }

    void TryPickupItem()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)); 
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, itemLayer))
        {
            Item item = hit.collider.GetComponent<Item>();

            if (item != null)
            {
                PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
                inventory.PickupItem(item.itemName); // registra no inventário
                Destroy(hit.collider.gameObject);   // remove do mundo
            }
        }
    }
}
