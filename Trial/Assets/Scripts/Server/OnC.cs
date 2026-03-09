using UnityEngine;

public class OnC : MonoBehaviour
{
    public GameObject Objects;

    public void OpenUI()
    {
        Objects.SetActive(true);
    }
    public void CloudUI()
    {
        Objects.SetActive(false);
    }
}
