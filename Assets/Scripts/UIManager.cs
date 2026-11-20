using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Image imagePrefab;
    public GameObject bossUI;
    public Transform heartsContainer;
    [SerializeField] Sprite heartFullSprite, heartEmptySprite;
    [SerializeField] Slider bossBarSlider;
    [SerializeField] public TextMeshProUGUI bossBarText, bossNameText, fpsCounterText;

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

    public void ScreenShake(float _duration, float _shakeAmount, Vector3 _originalPosition)
    {
        StartCoroutine(ScreenShakeCoroutine(_duration, _shakeAmount, _originalPosition));
    }

    IEnumerator ScreenShakeCoroutine(float _duration, float _shakeAmount, Vector3 _originalPosition)
    {
        float time = 0;

        while (time < _duration)
        {
            Vector3 randomShake =  Random.insideUnitSphere * _shakeAmount;

            Camera.main.transform.position += new Vector3(randomShake.x, randomShake.y, 0);
            time += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = _originalPosition;
        yield return null;
    }

    public void UpdateBossHealthBar(float _newHealthPercent, float _newHealthAmount, float _totalHealth)
    {
        bossBarSlider.value = _newHealthPercent / 100f;
        string newBarText;

        if (_newHealthAmount >= 1000)
        {
            _newHealthAmount /= 1000;
            newBarText = $"{_newHealthAmount}K";
        }
        else
        {
            newBarText = $"{_newHealthAmount}";
        }

        if (_totalHealth >= 1000)
        {
            _totalHealth /= 1000;
            newBarText += $"/{_totalHealth}K";
        }
        else
        {
            newBarText += $"/{_totalHealth}";
        }
        bossBarText.text = newBarText;
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
