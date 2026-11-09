using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image imagePrefab;

    public Transform heartsContainer;
    [SerializeField] Sprite heartFullSprite, heartEmptySprite;

    public TextMeshProUGUI timerText;

    public static UIManager instance;
    private void Awake()
    {
        instance = this;
    }
    public void AddHeart()
    {
        Image heartImage = Instantiate(imagePrefab, heartsContainer);
        heartImage.sprite = heartFullSprite;
    }

    public void UpdateHealth(int _newAmount)
    {
        int currentHearts = heartsContainer.childCount;

        for(int i = 0; i < _newAmount; i++)
        {
            heartsContainer.GetChild(i).gameObject.GetComponent<Image>().sprite = heartFullSprite;
        }

        for (int i = _newAmount; i < currentHearts; i++)
        {
            heartsContainer.GetChild(i).gameObject.GetComponent<Image>().sprite = heartEmptySprite;
        }
    }

    public void UpdateMaxHealth(int _newAmount)
    {
        foreach(Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        int currentHearts = 0;;
        int difference = _newAmount - currentHearts;

        if (difference > 0)
        {
            for (int i = 0; i < difference; i++)
            {
                AddHeart();
            }
        }
        else if (difference < 0)
        {
            Destroy(heartsContainer.GetChild(heartsContainer.childCount - 1).gameObject);
        }
    }

    public void SetTMProString(TextMeshProUGUI _textObject, string _string)
    {
        _textObject.text = _string;
    }

    public void ToggleElement(GameObject _obj, bool _active)
    {
        _obj.SetActive(_active);
    }
}
