using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] levelPrefabs;
    [SerializeField] private Transform levelParent;
    private GameObject currentLevel;
    private int currentLevelIndex = 0;
    private BallController ball;

    private void Awake()
    {
        ball = FindObjectOfType<BallController>();
    }

    public void LoadNextLevel()
    {
        if (currentLevelIndex < levelPrefabs.Length)
        {
            if (currentLevel != null) { Destroy(currentLevel); }

            currentLevel = Instantiate(levelPrefabs[currentLevelIndex++], levelParent);
            ball.ResetBall();
        }
        else
        {
            // game end
            Debug.Log("All levels completed!");
        }
    }
}