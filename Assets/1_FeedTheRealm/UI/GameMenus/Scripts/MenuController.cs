using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private MakerInputReader inputReader;

    void Awake()
    {
        inputReader.ToggleInput(false);
    }

    public void CloseMenu()
    {
        inputReader.ToggleInput(true);
        Destroy(gameObject);
    }

    public void OpenMenu(GameObject menuprefab)
    {
        CloseMenu();
        Instantiate(menuprefab);
    }
}
