using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class UIHelper : MonoBehaviour
{
    public Sprite CheckBoxEmpty;
    public Sprite CheckBoxChecked;
    public Sprite Play;
    public Sprite Pause;
    static UIHelper instance;

    private void Awake()
    {
        instance = this;
    }

    public static Sprite GetPlayPNG() => instance.Play;
    public static Sprite GetPausePNG() => instance.Pause;
    public static Sprite GetCheckBoxEmptyPNG() => instance.CheckBoxEmpty;
    public static Sprite GetCheckBoxCheckedPNG() => instance.CheckBoxChecked;

    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        int UILayer = LayerMask.NameToLayer("UI");
        var eventSystemRaysastResults = GetEventSystemRaycastResults();
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public static void UpdateBodyDropdowns()
    {
        var bodies = FindObjectsOfType<CelestialBody>();
        var options = bodies.Cast<CelestialBody>().Select<CelestialBody, string>(x => x.name).ToList();

        var dropdown = GameObject.Find("Focus Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();

        dropdown = GameObject.Find("Predict Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }
}
