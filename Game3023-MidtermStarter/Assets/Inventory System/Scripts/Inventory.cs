using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Assertions;
using System.Linq; // For finding all gameObjects with name

public class Inventory : MonoBehaviour, ISaveHandler
{
    [Tooltip("Whether or not this is a crafting Table")]
    [SerializeField]
    public bool isCraftingTable;

    [Tooltip("is this inventory an Output Box?")]
    public bool isOutputBox;

    [Tooltip("Drag Output Box ItemSlot here")]
    [SerializeField]
    private ItemSlot OutputSlot;

    [Tooltip("Reference to the master item table")]
    [SerializeField]
    public ItemTable masterItemTable;

    [Tooltip("The object which will hold Item Slots as its direct children")]
    [SerializeField]
    private GameObject inventoryPanel;

    [Tooltip("List size determines how many slots there will be. Contents will replaced by copies of the first element")]
    [SerializeField]
    private List<ItemSlot> itemSlots;

    [Tooltip("Items to add on Start for testing purposes")]
    [SerializeField]
    private List<Item> startingItems;

    [Tooltip("Starting amount for corresponding Starting Item")]
    [SerializeField]
    private int[] StartingItemQuantities;

    [SerializeField]
    private RecipeTable MasterRecipeTable;
    private bool[] CorrectCraftingComponents = {false, false, false, false, false, false, false, false, false};
    /// <summary>
    /// Private key used for saving with playerprefs
    /// </summary>
    private string saveKey = "";

    // Start is called before the first frame update
    void Start()
    {
       
        InitItemSlots();
        InitSaveInfo();

        // init starting items for testing
        for (int i = 0; i < startingItems.Count && i < itemSlots.Count; i++)
        {
            itemSlots[i].SetContents(startingItems[i], StartingItemQuantities[i]);
        }
    }

    private void InitItemSlots()
    {
        Assert.IsTrue(itemSlots.Count > 0, "itemSlots was empty");
        Assert.IsNotNull(itemSlots[0], "Inventory is missing a prefab for itemSlots. Add it as the first element of its itemSlot list");

        // init item slots
        for (int i = 1; i < itemSlots.Count; i++)
        {
            GameObject newObject = Instantiate(itemSlots[0].gameObject, inventoryPanel.transform);
            ItemSlot newSlot = newObject.GetComponent<ItemSlot>();
            itemSlots[i] = newSlot;
            if (isCraftingTable)
                itemSlots[i].isCraftingSlot = true;
            if (isOutputBox)
                itemSlots[i].isOutputSlot = true;
        }

        foreach (ItemSlot item in itemSlots)
        {
            item.onItemUse.AddListener(OnItemUsed);
        }
    }
    private void InitSaveInfo()
    {
        // init save info
        //assert only one object with the same name, or else we can have key collisions on PlayerPrefs
        Assert.AreEqual(
            Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(gameObArg => gameObArg.name == gameObject.name).Count(),
            1,
            "More than one gameObject have the same name, therefore there may be save key collisions in PlayerPrefs"
            );

        // set a key to use for saving/loading
        saveKey = gameObject.name + this.GetType().Name;

        //Subscribe to save events on start so we are listening
        GameSaver.OnLoad.AddListener(OnLoad);
        GameSaver.OnSave.AddListener(OnSave);
    }

    private void OnDestroy()
    {
        // Remove listeners on destroy
        GameSaver.OnLoad.RemoveListener(OnLoad);
        GameSaver.OnSave.RemoveListener(OnSave);

        foreach (ItemSlot item in itemSlots)
        {
            item.onItemUse.RemoveListener(OnItemUsed);
        }
    }

    //////// Event callbacks ////////

    void OnItemUsed(Item itemUsed)
    {
        // Debug.Log("Inventory: item used of category " + itemUsed.category);
        Cursor.SetCursor(itemUsed.Icon.texture, Vector2.zero, CursorMode.Auto);
        this.SendMessage("PickedUpItem", itemUsed.ItemID);
        if (this.isOutputBox) { this.SendMessage("PickedUpOutput"); Debug.Log("Grabbed Output"); }
        
    }

    public void OnSave()
    {
        //Make empty string
        //For each item slot
        //Get its current item
        //If there is an item, write its id, and its count to the end of the string
        //If there is not an item, write -1 and 0 

        //File format:
        //ID,Count,ID,Count,ID,Count

        string saveStr = "";

        foreach(ItemSlot itemSlot in itemSlots)
        {
            int id = -1;
            int count = 0;

            if(itemSlot.HasItem())
            {
                id = itemSlot.ItemInSlot.ItemID;
                count = itemSlot.ItemCount;
            }

            saveStr += id.ToString() + ',' + count.ToString() + ',';
        }

        PlayerPrefs.SetString(saveKey, saveStr);
    }

    public void OnLoad()
    {
        //Get save string
        //Split save string
        //For each itemSlot, grab a pair of entried (ID, count) and parse them to int
        //If ID is -1, replace itemSlot's item with null
        //Otherwise, replace itemSlot with the corresponding item from the itemTable, and set its count to the parsed count

        string loadedData = PlayerPrefs.GetString(saveKey, "");

        Debug.Log(loadedData);

        char[] delimiters = new char[] { ',' };
        string[] splitData = loadedData.Split(delimiters);

        for(int i = 0; i < itemSlots.Count; i++)
        {
            int dataIdx = i * 2;

            int id = int.Parse(splitData[dataIdx]);
            int count = int.Parse(splitData[dataIdx + 1]);

            if(id < 0)
            {
                itemSlots[i].ClearSlot();
            } else
            {
                itemSlots[i].SetContents(masterItemTable.GetItem(id), count);
            }
        }
    }

    public void CheckCraftingTable()
    {
        if (isCraftingTable)
        {
            for(int i = 0; i < MasterRecipeTable.RecipeList.Length; i++)
            {
                for (int j = 0; j < MasterRecipeTable.RecipeList[i].Components.Length; j++)
                {
                    for (int k = 0; k < itemSlots.Count; j++)
                    {
                        if (MasterRecipeTable.RecipeList[i].Components[k] != itemSlots[k].ItemInSlot)
                        {
                            CorrectCraftingComponents[k] = false;
                        }
                        if (MasterRecipeTable.RecipeList[i].Components[k] == itemSlots[k].ItemInSlot)
                        {
                            CorrectCraftingComponents[k] = true;
                        }
                    }
                }
            }
            //Debug.Log("Checked Crafting Table");
            //for (int i = 0; i < MyRecipes.Count; i++)
            //{
            //    for(int j = 0; j < itemSlots.Count; j++)
            //    {
            //        if(MyRecipes[i].Components[j] != itemSlots[j].ItemInSlot)
            //        {
            //            CorrectCraftingComponents[j] = false;
            //        }
            //        if(MyRecipes[i].Components[j] == itemSlots[j].ItemInSlot)
            //        {
            //            CorrectCraftingComponents[j] = true;
            //            Debug.Log("Correct Component in slot");
            //        }
                    
            //    }

            //    Debug.Log("Performed Recipe Check, each log == how many recipes");

            //    if(FinalCraftingCheck() == true)
            //    {
            //        OutputSlot.SetContents(MyRecipes[i].Output, 1);
            //        Debug.Log("Crafting Complete!");
            //    }
                
            //}
        }
    }

    private bool FinalCraftingCheck()
    {
        for(int i = 0; i < CorrectCraftingComponents.Length; i++)
        {
            if (CorrectCraftingComponents[i] == false)
                return false;
        }

        return true;
    }
}
