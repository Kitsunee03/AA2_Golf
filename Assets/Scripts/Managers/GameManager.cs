// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform levelParent;
    private GameObject currentLevel;
    private int currentLevelIndex = -1;
    private BallController ball;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ball = FindObjectOfType<BallController>();
            return;
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        LoadLevel();
    }

    private void Update()
    {
        Cheats();
    }

    public void LoadLevel(bool p_nextLevel = true)
    {
        if (p_nextLevel) { currentLevelIndex++; }
        else { currentLevelIndex--; }
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelPrefabs.Length -1);

        if (currentLevelIndex < levelPrefabs.Length)
        {
            if (currentLevel != null) { Destroy(currentLevel); }

            currentLevel = Instantiate(levelPrefabs[currentLevelIndex], levelParent);
            ball.ResetBall();
        }
        else
        {
            Debug.Log("All levels completed!");

            // End Game UI
        }
    }

    private void Cheats()
    {
        if (Input.GetKeyDown(KeyCode.E)) { LoadLevel(); }
        else if (Input.GetKeyDown(KeyCode.Q)) { LoadLevel(false); }

        if (Input.GetKeyDown(KeyCode.R)) { ball.ResetBall(); }
    }
}