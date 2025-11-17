using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public GameObject lantern;
    public GameObject axe;

    private bool hasFlashlight = false;
    private bool hasAxe = false;

    private int currentSlot = 0; // 0=nada, 1=lanterna, 2=machado

    void Start()
    {
        EquipItem(0);
    }

    void Update()
    {
        // Só troca para itens que o jogador já pegou
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasFlashlight)
            EquipItem(1);

        if (Input.GetKeyDown(KeyCode.Alpha2) && hasAxe)
            EquipItem(2);

        // Scroll também só troca se o item existir
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            if (currentSlot == 1 && hasAxe) EquipItem(2);
            else if (currentSlot == 2 && hasFlashlight) EquipItem(1);
        }
        else if (scroll < 0f)
        {
            if (currentSlot == 1 && hasAxe) EquipItem(2);
            else if (currentSlot == 2 && hasFlashlight) EquipItem(1);
        }
    }

    public void PickupItem(string itemName)
    {
        if (itemName == "Lantern")
        {
            hasFlashlight = true;
            EquipItem(1); // equipa automaticamente
        }
        else if (itemName == "Axe")
        {
            hasAxe = true;
            EquipItem(2); // equipa automaticamente
        }
    }

    void EquipItem(int slot)
    {
        currentSlot = slot;

        lantern.SetActive(false);
        axe.SetActive(false);

        if (slot == 1 && hasFlashlight)
            lantern.SetActive(true);

        if (slot == 2 && hasAxe)
            axe.SetActive(true);
    }
}
