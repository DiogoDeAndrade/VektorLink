using System;
using UC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void OnChangeWave(int wave);
    public event OnChangeWave onChangeWave;

    [SerializeField] private WaveDef[] waveDefs;

    int wave = 0;
    static public GameManager Instance => instance;
    
    static GameManager instance;
    
    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetGame();
    }

    public void ResetGame()
    {
        wave = 0;
    }

    public WaveDef GetWave()
    {
        if (wave < waveDefs.Length) return waveDefs[wave];

        return waveDefs[waveDefs.Length - 1];
    }

    public void NextWave()
    {
        wave++;
        onChangeWave?.Invoke(wave);
    }
}
