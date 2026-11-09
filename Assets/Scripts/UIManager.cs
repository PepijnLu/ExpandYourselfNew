using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image imagePrefab;

    [SerializeField] Transform heartsContainer;
    [SerializeField] Sprite heartFullSprite, heartEmptySprite;
    private void Awake()
    {
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
}
