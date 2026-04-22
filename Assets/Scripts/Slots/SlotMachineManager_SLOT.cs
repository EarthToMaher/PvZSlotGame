using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlotMachineManager_SLOT : MonoBehaviour
{
    public SlotMachineTower[] selectedTowers;
    public int numOfColumns;
    private int count;
    private SlotMachineTower[][] finalResults = new SlotMachineTower[5][];
    public int visibleIcons;
    public AnimationCurve spinTimeCurve;
    public static event Action OnSpinStart;
    public static event Action FinishSpin;
    public enum MachineStates
    {
        IDLe,
        SPINNING,
        WAITING,
        PAYING
    }

    private MachineStates state = MachineStates.IDLe;

    [Header("References")]
    public RectTransform columnPrefab;
    public Transform columnsParent;

    public class RowData
    {
        public bool[] data;
    }

    public RowData[] test;

    public Transform[] columnParents;

    //private List<List<SlotIcon>> reelTapes = new();
    //private List<int> reelIndex = new();
    private List<RectTransform> columnRoots = new();

    private ReelTape_SLOT[] reelTapes;

    private int fullTapeLength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        finalResults[0] = new SlotMachineTower[5];
        finalResults[1] = new SlotMachineTower[5];
        finalResults[2] = new SlotMachineTower[5];
        finalResults[3] = new SlotMachineTower[5];
        finalResults[4] = new SlotMachineTower[5];
        reelTapes = new ReelTape_SLOT[numOfColumns];
        for(int i = 0; i < numOfColumns; i++)
        {
            Debug.Log("Ran for loop for creation");
            RectTransform column = Instantiate(columnPrefab,columnParents[i]);
            column.gameObject.transform.SetParent(columnParents[i]);
            ReelTape_SLOT rt = column.gameObject.AddComponent<ReelTape_SLOT>();
            rt.SetSlotInfo(selectedTowers,column,i,visibleIcons);
            rt.CreateTape();
            reelTapes[i] = rt;
        }
    }


    [ContextMenu("Start Spin")]
    public void StartSpin()
    {
        if(state==MachineStates.IDLe)
        {
            Debug.Log("Started Spin Logic");
            OnSpinStart?.Invoke();
            state = MachineStates.SPINNING;
            StartCoroutine(SpinTimer());
        }

    }

    public IEnumerator<WaitForSeconds> SpinTimer()
    {
        yield return new WaitForSeconds(spinTimeCurve.Evaluate(UnityEngine.Random.Range(0,1f)));
        Debug.LogWarning("ending spin");
        state = MachineStates.IDLe;

        StartCoroutine(StopSpinningLogic(0));
    }

    public IEnumerator StopSpinningLogic(int index)
    {
        reelTapes[index].SetFinishSpin();
        yield return new WaitForSeconds(1f);
        index++;
        if (index<reelTapes.Length) StartCoroutine(StopSpinningLogic(index));
    }

    public void AddToResults(SlotMachineTower[] results)
    {
        finalResults[count] = results;
        count++;
        if (count == numOfColumns)
        {
            string result = "Final- ";
            for(int y = 0; y < 5; y++)
            {
                count=0;
                for(int x = 0; x < numOfColumns; x++)
                {
                    result = result + finalResults[x][y].sprite.name;
                }
                result = result + " : ";
            }
            //Debug.LogWarning(result);
            CheckPayouts();
        }
    }

    public void CheckPayouts()
    {
        foreach(SlotMachineTower tower in selectedTowers)
        {
            foreach(SlotOutcome outcome in tower.outcomes)
            {
                CheckPayline(tower, finalResults, outcome);
            }
        }
    }

    List<SlotMachineTower> GetPaylineSymbols(SlotMachineTower[][] grid, SlotOutcome outcome, int offset)
    {
        List<SlotMachineTower> symbols = new List<SlotMachineTower>(); //Temp List of Symbols to return
        for (int col = 0; col < outcome.outcome.Length; col++) //Iterates through each character of the pattern string. Always 5
        {
            for (int row = 0; row < outcome.outcome[col].Length-1; row++) //Iterates through each element of the array
            {
                if (outcome.outcome[col][row] == '1') 
                {
                    symbols.Add(grid[row][col+offset]); //If 1, add symbol to it.
                }
            }
        }

        return symbols;
    }


    void CheckPayline(SlotMachineTower towerType, SlotMachineTower[][] grid, SlotOutcome outcome)
    {
        for(int i = 0; i < numOfColumns + 1 - outcome.outcome.Length; i++)
        {
            bool notFound = false;
            List<SlotMachineTower> towerList = GetPaylineSymbols(grid, outcome, i); //Get our Symbol list
            SlotMachineTower firstTower = towerList[0]; //Get the first tower in our symbol list
            foreach(SlotMachineTower tower in towerList) //For each tower in our symbol list
            {
                if(firstTower.name != towerType.name || firstTower.name != tower.name) notFound=true; //If the name of the first tower is different than the type we are checking or the current one is different, false.
            }
            if(notFound) continue; //If we failed to find it, continue through the for loop
            DrawPayline(towerType,outcome,i);
            
        }


    }

    void DrawPayline(SlotMachineTower tower, SlotOutcome outcome, int offset)
    {
        DebugPayline(tower, outcome, offset);
    }

    void DebugPayline(SlotMachineTower tower, SlotOutcome outcome, int offset)
    {
        Debug.LogWarning("Payline Found: " + outcome.name + " Tower: " + tower.sprite.name + " Starting at Line: " + (offset+outcome.outcome.Length));
    }
}
