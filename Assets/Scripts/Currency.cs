using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Currency : MonoBehaviour
{
    public int currCurrency = 1000;
    public int spinAmount = 10;
    public TextMeshProUGUI currencyText;
    public void Start()
    {
        UpdateUI(currCurrency);
    }
    public void SpinSlot()
    {
        if(FindFirstObjectByType<SlotMachineManager_SLOT>().state == SlotMachineManager_SLOT.MachineStates.SPINNING) return;
        if(currCurrency<spinAmount) SceneManager.LoadScene(0);
        else
        {

            StartCoroutine(UpdateCurrencyText(currCurrency,currCurrency-spinAmount,-1));
            currCurrency -= spinAmount;
            FindFirstObjectByType<SlotMachineManager_SLOT>().StartSpin();
        }
    }

    public void UpdateUI(int amt)
    {
        currencyText.text ="" + amt;
    }

    public void AddCurrency(int amt)
    {
        StartCoroutine(UpdateCurrencyText(currCurrency,currCurrency+amt,1));
        currCurrency += amt;
        
    }

    public IEnumerator UpdateCurrencyText(int startingCurrency, int endingCurrency, int scaling)
    {
        while (startingCurrency != endingCurrency)
        {
            startingCurrency += scaling;
            UpdateUI(startingCurrency);
            yield return new WaitForEndOfFrame();
        }
    }
}
