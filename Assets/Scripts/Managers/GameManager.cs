// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform levelParent;
    private GameObject currentLevel;
    private int currentLevelIndex = 0;
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
        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        if (currentLevelIndex < levelPrefabs.Length)
        {
            if (currentLevel != null)
                Destroy(currentLevel);

            currentLevel = Instantiate(levelPrefabs[currentLevelIndex++], levelParent);
            ball.ResetBall();
        }
        else
        {
            Debug.Log("All levels completed!");
            // Aquí podrías mostrar UI de fin de juego
        }
    }
}