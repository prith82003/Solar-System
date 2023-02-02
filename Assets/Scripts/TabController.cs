using UnityEngine.Events;
using UnityEngine;

public class TabController : MonoBehaviour
{
    public static System.Action<TabController> OnMinimise;
    public bool isMinimised = false;
    public void OnClick()
    {
        OnMinimise?.Invoke(this);
        isMinimised = !isMinimised;
    }
}
