using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
public class GameManager_ : MonoBehaviour
{
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreen;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Canvas mainUICanvas;
    public static GameManager_ Instance;
    public State State=>state;
    private State state;
    private int currentLevel;
    private List<int> levels= new List<int>
    {
        14,
        15,
        16,
        18,
        19,
        20,
        25,
        30,
        35

    };
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        currentLevel=0;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeState(State.PLAYING);
        gridManager.RegenerateLevel(levels[currentLevel]);
        nextLevelButton.onClick.AddListener(()=>{
            mainUICanvas.gameObject.SetActive(true);
            WinScreen.SetActive(false);
            currentLevel++;
            currentLevel=Mathf.Clamp(currentLevel,0,levels.Count);
            gridManager.RegenerateLevel(levels[currentLevel]);
        });
        retryButton.onClick.AddListener(()=>{
            mainUICanvas.gameObject.SetActive(true);
            LoseScreen.SetActive(false);
            gridManager.RegenerateLevel(levels[currentLevel]);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeState(State state)
    {
        this.state=state;
    }
    public void WinGame()
    {
        state=State.END;
       StartCoroutine(DelayShowPopUp(WinScreen,SoundType.Win));
    }
    public void LoseGame()
    {
        state=State.END;
      StartCoroutine(DelayShowPopUp(LoseScreen,SoundType.Win));
    }
    public IEnumerator DelayShowPopUp(GameObject canvas, SoundType type)
    {
        yield return new WaitForSeconds(1f);
        mainUICanvas.gameObject.SetActive(false);
        SoundManager.Instance.PlaySound(type);
        canvas.SetActive(true);
        
    }
}
public enum State
{
    START=0,
    PLAYING=5,
    END=10,

}
