using UnityEngine;

public class ColourManager : MonoBehaviour
{
    public static ColourManager instance;

    public Color[] colours;

    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
