using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private MakerInputReader inputReader;

    public void OpenMenu()
    {
        inputReader.ToggleInput(false);
        gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        inputReader.ToggleInput(true);
        gameObject.SetActive(false);
    }
}
