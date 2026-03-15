using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BombBooster : MonoBehaviour
{
    [SerializeField] private Button bombButton;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image selectedUI; 
    private bool isBoosterActive;
    public bool IsBoosterActive=>isBoosterActive;
    private int count;
    void Awake()
    {
        isBoosterActive=false;
        count=1;
        SetText(count);
    }
    void Start()
    {
        bombButton.onClick.AddListener(() =>{
           if(count>0){
            isBoosterActive=!isBoosterActive;
            selectedUI.gameObject.SetActive(isBoosterActive);
            }
        });
    }
    public void UseBooster()
    {
        isBoosterActive=false;
        count--;
        selectedUI.gameObject.SetActive(isBoosterActive);
        SetText(count);
    }
    void SetText(int count)
    {
        countText.SetText(count.ToString());
    }
}
