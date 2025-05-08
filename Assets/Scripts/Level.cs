using UnityEngine;

public class Level : MonoBehaviour
{
    [Header("Level attributes")]
    [SerializeField] private string levelName;
    [SerializeField] private bool isGhostLevel;

    #region Accessors
    public string LevelName { get { return levelName; } set { levelName = value; } }
    public bool IsGhostLevel { get { return isGhostLevel; } set { isGhostLevel = value; } }
    #endregion
}