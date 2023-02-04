using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;

public class UIController : MonoBehaviour
{
    public GameObject selectedObj;
    public LayerMask PlanetLayerMask;
    public GameObject PlanetDetailsObj;
    public float maxNumSteps;
    public float tabFadeSpeed;
    private const float DEFAULT_TAB_ALPHA = 80f / 255f;
    public float intensity = 1.5f;
    public float velMultiplier;

    [Header("UI Elements")]
    public TMP_Text planetText;
    public GameObject velArrow;
    public TMP_Text velMagnitudeTxt;
    public Image playPauseBtn;
    public Slider numStepsSlider;
    public GameObject colorPicker;
    public Image focusCheckbox;
    public Image predictToggleCheckbox;
    public Image predictRelativeCheckbox;
    public Image updatePredictionCheckbox;
    GameController gc;

    private void Start()
    {
        gc = FindObjectOfType<GameController>();
        PlanetDetailsObj.SetActive(false);
        UIHelper.UpdateBodyDropdowns();
        TabController.OnMinimise += ToggleTab;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnClick();

        if (selectedObj)
        {
            DisplayVelocityVector();
            if (!colorPicker.activeInHierarchy)
                colorPicker.SetActive(true);
        }
        else
        {
            if (colorPicker.activeInHierarchy)
                colorPicker.SetActive(false);
        }
    }

    void OnClick()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics.Raycast(mousePos, Camera.main.transform.forward, out var ray, float.MaxValue, PlanetLayerMask, QueryTriggerInteraction.Ignore);

        if (hit)
        {
            selectedObj = ray.transform.gameObject;
            planetText.text = selectedObj.name;
            PlanetDetailsObj.SetActive(true);

            var tc = PlanetDetailsObj.transform.parent.GetChild(0).GetComponent<TabController>();
            if (tc.isMinimised)
                ToggleTab(tc);

            StartCoroutine(OnPlanetClick(ray));
        }
        else
        {
            if (UIHelper.IsPointerOverUIElement())
                return;

            PlanetDetailsObj.SetActive(false);
            selectedObj = null;
        }
    }

    // Display the velocity with an arrow
    // Make arrow point in direction
    // Magnitude displayed as text
    void DisplayVelocityVector()
    {
        Vector3 velocity = selectedObj.GetComponent<CelestialBody>().velocity;
        float angle = Vector3.SignedAngle(Vector3.forward, velocity, -Vector3.up);
        velArrow.transform.eulerAngles = new Vector3(velArrow.transform.eulerAngles.x, velArrow.transform.eulerAngles.y, (angle + 90));
        velMagnitudeTxt.text = velocity.magnitude.ToString();
    }

    public void TogglePause()
    {
        if (gc.isPaused)
        {
            gc.isPaused = false;
            playPauseBtn.sprite = UIHelper.GetPausePNG();
        }
        else
        {
            gc.isPaused = true;
            playPauseBtn.sprite = UIHelper.GetPlayPNG();
        }
    }

    public void OnStepSliderChange()
    {
        gc.numSteps = (int)(numStepsSlider.value * maxNumSteps);

        if (gc.isPaused && gc.updatePredictions && gc.init)
            gc.PredictPositions();
    }

    // Testing
    public void OnColorChange(Color color)
    {
        selectedObj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color * selectedObj.GetComponent<CelestialBody>().intensity);
    }

    public void OnColorSubmit(Color color)
    {
        selectedObj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", color * selectedObj.GetComponent<CelestialBody>().intensity);
    }

    public void OnColorCancel()
    {
        // TODO: Implement reverting to old color if cancel is pressed
    }

    void ToggleFocus()
    {
        gc.focus = !gc.focus;

        if (gc.focus)
        {
            focusCheckbox.sprite = UIHelper.GetCheckBoxCheckedPNG();
            FocusObject();
        }
        else
            focusCheckbox.sprite = UIHelper.GetCheckBoxEmptyPNG();
    }

    public void FocusObject()
    {
        var dropdown = GameObject.Find("Focus Dropdown").GetComponent<TMP_Dropdown>();
        gc.focusObject = GameObject.Find(dropdown.captionText.text);
    }

    public void TogglePredict()
    {
        LineDrawer.DrawLine = !LineDrawer.DrawLine;

        if (LineDrawer.DrawLine)
        {
            predictToggleCheckbox.sprite = UIHelper.GetCheckBoxCheckedPNG();
            gc.Predict();
        }
        else
            predictToggleCheckbox.sprite = UIHelper.GetCheckBoxEmptyPNG();
    }

    public void ToggleRelativePredict()
    {
        gc.relativeTo = !gc.relativeTo;

        if (gc.relativeTo)
        {
            predictRelativeCheckbox.sprite = UIHelper.GetCheckBoxCheckedPNG();
            RelativeTo();
        }
        else
            predictRelativeCheckbox.sprite = UIHelper.GetCheckBoxEmptyPNG();

        gc.Predict();
    }

    public void RelativeTo()
    {
        var dropdown = GameObject.Find("Predict Dropdown").GetComponent<TMP_Dropdown>();
        gc.relativeBody = GameObject.Find(dropdown.captionText.text);

        if (gc.isPaused)
            gc.PredictPositions();
    }

    public void ToggleUpdatePrediction()
    {
        gc.updatePredictions = !gc.updatePredictions;

        if (gc.updatePredictions)
            updatePredictionCheckbox.sprite = UIHelper.GetCheckBoxCheckedPNG();
        else
            updatePredictionCheckbox.sprite = UIHelper.GetCheckBoxEmptyPNG();
    }

    public void ToggleTab(TabController tab)
    {
        var tabObj = tab.transform.parent.GetChild(1);

        if (tab.isMinimised)
            MaximiseTab(tabObj.gameObject);
        else
            MinimiseTab(tabObj.gameObject);
    }

    public void MinimiseTab(GameObject tab)
    {
        // Set inactive all children
        foreach (Transform child in tab.transform)
            child.gameObject.SetActive(false);

        // Fade the tab to invisible
        StartCoroutine(FadeTab(tab));
    }

    IEnumerator FadeTab(GameObject tab)
    {
        // Get Image Component
        var img = tab.GetComponent<RawImage>();
        var color = img.color;

        // Fade out
        while (color.a > 0)
        {
            color.a -= tabFadeSpeed;
            img.color = color;
            yield return null;
        }
    }

    public void MaximiseTab(GameObject tab)
    {
        // Fade the tab to visible
        StartCoroutine(UnFadeTab(tab));

        // Set active all children
        foreach (Transform child in tab.transform)
            child.gameObject.SetActive(true);
    }

    IEnumerator UnFadeTab(GameObject tab)
    {
        // Get Image Component
        var img = tab.GetComponent<RawImage>();
        var color = img.color;

        // Fade in
        while (color.a < DEFAULT_TAB_ALPHA)
        {
            color.a += tabFadeSpeed;
            img.color = color;
            yield return null;
        }
    }

    public IEnumerator OnPlanetClick(RaycastHit hit)
    {
        Debug.Log("Clicked On Planet");
        // Get Starting Mouse Position on Screen
        var startPos = Input.mousePosition;
        var celestialBody = hit.transform.GetComponent<CelestialBody>();

        if (!(gc.isPaused))
            yield break;

        while (Input.GetMouseButton(0))
        {
            Debug.Log("Holding");
            var mousePos = Input.mousePosition;

            var diff = (mousePos - startPos);
            var temp = diff.y;
            diff.y = diff.z;
            diff.z = temp;

            celestialBody.velocity += diff * velMultiplier;
            gc.Predict();

            yield return null;
        }
    }
}
