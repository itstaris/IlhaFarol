using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Referências dos Itens")]
    public GameObject flashlight;
    public GameObject axe;

    private int currentSlot = 0; // 0 = vazio, 1 = lanterna, 2 = machado

    void Start()
    {
        // Começa com nada equipado
        EquipItem(0);
    }

    void Update()
    {
        // Trocar de item
        if (Input.GetKeyDown(KeyCode.Alpha1))
            EquipItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            EquipItem(2);

        // Alternativamente: usar scroll do mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            EquipItem(currentSlot == 2 ? 1 : 2);
        else if (scroll < 0f)
            EquipItem(currentSlot == 1 ? 2 : 1);
    }

    void EquipItem(int slot)
    {
        currentSlot = slot;

        // Desativa todos os itens
        flashlight.SetActive(false);
        axe.SetActive(false);

        // Ativa o item correspondente
        if (slot == 1 && flashlight != null)
            flashlight.SetActive(true);
        else if (slot == 2 && axe != null)
            axe.SetActive(true);
    }

    public void PickupItem(string itemName)
    {
        // Quando pegar o item no mundo
        if (itemName == "Flashlight")
            flashlight.SetActive(true);
        else if (itemName == "Axe")
            axe.SetActive(true);
    }
}
