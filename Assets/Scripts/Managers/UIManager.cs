using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private GameManager gm;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelNameText;

    [Header("Cheats")]
    [SerializeField] private TextMeshProUGUI cheatsToggleLabel;
    [SerializeField] private GameObject cheatButtons;
    [SerializeField] private GameObject cheatControlsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        gm = GameManager.Instance;
    }

    public void UpdateLevelName(string p_name)
    {
        if (levelNameText != null) { levelNameText.text = p_name; }
    }

    // used by buttons
    public void OnLevelChangeButtonPressed(bool p_nextLevel) { gm.LoadLevel(p_nextLevel); }
    public void OnCheatsTogglePressed()
    {
        gm.ToggleCheats();
        if (cheatButtons != null) { cheatButtons.SetActive(gm.CheatsEnabled); }
        if (cheatControlsText != null) { cheatControlsText.SetActive(gm.CheatsEnabled); }
        if (cheatsToggleLabel != null) { cheatsToggleLabel.text = gm.CheatsEnabled ? "Cheats enabled" : "Cheats disabled"; }
    }
}