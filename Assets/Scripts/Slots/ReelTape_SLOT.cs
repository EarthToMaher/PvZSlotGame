
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;
public class ReelTape_SLOT : MonoBehaviour
{
        private SlotMachineTower[] reelTapeIcons;
        private Queue<SlotMachineTower> reelTapeOrder;
        private int reelNum;
        private int numInColumn;
        private RectTransform column;
        public ReelStates state = ReelStates.DONe;
        private float slowdownSpipnSpeed;
        public RectTransform[] finalOutcomes = new RectTransform[5];

        private Queue<RectTransform> reelTapeTransforms;
        int index = 0;

        public enum ReelStates
    {
        SPINNING,
        STOPPING,
        WAITING,
        DONe
    }

        public void SetSlotInfo(SlotMachineTower[] icons, RectTransform canvas, int columnNum, int columnSize)
        {
           reelTapeIcons = icons;
           column = canvas;
           reelNum = columnNum;
           numInColumn = columnSize;
        }

    public void OnDisable()
    {
                    SlotMachineManager_SLOT.OnSpinStart -= StartSpin;
            //SlotMachineManager_SLOT.FinishSpin -= SetFinishSpin;
    }


    public class SlotIconInfo
    {
        public Sprite sprite;
        public SlotMachineTower towerInfo;
    }

    public void CreateTape(){
            Debug.Log("Ran Create");
            
            //Create our reel with an order
            
            reelTapeTransforms = new();
            reelTapeOrder = new();
            //Create a temp list of the sprites so we can populate our reelTapeOrder
            //List<Sprite> reelTapeSprites = new();
            List<SlotMachineTower> reelTapeTowers = new();

            foreach(SlotMachineTower icon in reelTapeIcons){
                for(int i = 0; i < icon.countInColumn[reelNum]; i++){
                    //reelTapeSprites.Add(icon.sprite);
                    reelTapeTowers.Add(icon);
                }
            }

            while(reelTapeTowers.Count > 0){
                int index = UnityEngine.Random.Range(0,reelTapeTowers.Count);
                reelTapeOrder.Enqueue(reelTapeTowers[index]);
                reelTapeTowers.RemoveAt(index);
            }
            Visualize();
            SlotMachineManager_SLOT.OnSpinStart += StartSpin;
            //SlotMachineManager_SLOT.FinishSpin += SetFinishSpin;
        }

        public void Visualize()
        {
            Debug.Log("Ran visualize");
            int index = reelTapeOrder.Count;
            int currentSlot = numInColumn-1;
            while(index >0)
            {
                GameObject reelIcon = new GameObject();
                reelIcon.AddComponent<Image>();
                //reelIcon.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,1);
                //reelIcon.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,1);
                reelIcon.transform.SetParent(column,false);
                reelIcon.GetComponent<Image>().sprite = reelTapeOrder.Peek().sprite;
                reelTapeOrder.Enqueue(reelTapeOrder.Dequeue());
                reelTapeTransforms.Enqueue(reelIcon.GetComponent<RectTransform>());
                float yPos = currentSlot* -176;
                currentSlot--;
                if(currentSlot > -2) reelIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,yPos);
                else reelIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,176);
                index--;
            }
        }

        public void FinishSpin()
        {
            while (index < numInColumn) //While we are updating our icons we can visually see
            {
                RectTransform thisTransform = reelTapeTransforms.Peek(); //Check our current transform
                float newYPos = thisTransform.anchoredPosition.y - 60f; //Calculate our new position
                thisTransform.anchoredPosition = new Vector2(0,newYPos); //Update our RectTransform
                reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue()); //Move this to the back of the queue
                index++; //Update our index
            }
            while (index < reelTapeTransforms.Count) //Continue cycling our queue for the elements we cannot see
            {
                reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue());
                index++;
            }
            index = 0; //Reset index before we run it again
            if (reelTapeTransforms.Peek().anchoredPosition.y <= -880) //If our final element is below what can be seen
            {
                RectTransform ourTransform = reelTapeTransforms.Dequeue(); //Dequeue our last element (the one we cannot see)
                ourTransform.anchoredPosition = new Vector2(0,200); //Reset it to the top
                reelTapeTransforms.Enqueue(ourTransform); //Requeue it to the back of the queue

            }

            Stack<RectTransform> slotOutcomes = new(); //Set up our outcome Stack
                while(index<numInColumn) //Populate our stack with what we rolled
                {
                    RectTransform rect = reelTapeTransforms.Dequeue();
                    slotOutcomes.Push(rect);
                    reelTapeTransforms.Enqueue(rect);
                    index++;
                }
                while (index<reelTapeTransforms.Count) //Requeue everything
                    {
                        reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue());
                        index++;
                    }
                    int count = 1; //Debug Helper, Debugs which icon is on which line
                    SlotMachineTower[] towerOutcomes = new SlotMachineTower[5];
                    while(slotOutcomes.Count > 0) //Iterate until our stack is empty
                    {
                        finalOutcomes[count-1] = slotOutcomes.Pop();
                        Sprite sprite = finalOutcomes[count-1].GetComponent<Image>().sprite;
                        Debug.Log(count + ": " + sprite.name);
                        foreach(SlotMachineTower tower in reelTapeIcons)
                        {
                            if (sprite.name == tower.sprite.name)
                            {
                                towerOutcomes[count-1]=tower;
                                Debug.Log(tower.outcomes[0].name);
                                break;
                            }
                        }
                        count++;
                    }
                    FindFirstObjectByType<SlotMachineManager_SLOT>().AddToResults(towerOutcomes);

            state = ReelStates.WAITING; //Update our state to know we finished spinning
        }

    public void FixAlignment()
    {
        int count = 0;
        for (int i = 0; i < 5; i++)
        {
            float targetY = -200*i;
            if (targetY < finalOutcomes[i].anchoredPosition.y)
            {
                float newYPos = Mathf.Clamp(finalOutcomes[i].anchoredPosition.y + slowdownSpipnSpeed,Mathf.NegativeInfinity,-200*i); //Calculate our new position
                finalOutcomes[i].anchoredPosition = new Vector2(0,newYPos); //Update our RectTransform
                count++;
            }
            else if (targetY > finalOutcomes[i].anchoredPosition.y)
            {
                float newYPos = Mathf.Clamp(finalOutcomes[i].anchoredPosition.y - slowdownSpipnSpeed,-200*i,Mathf.Infinity); //Calculate our new position
                finalOutcomes[i].anchoredPosition = new Vector2(0,newYPos); //Update our RectTransform
                count++;
            }
        }
        slowdownSpipnSpeed-=slowdownSpipnSpeed*0.25f;
        if (count == 0)
        {
            slowdownSpipnSpeed = 60;
            state = ReelStates.DONe;
        }

    }

    public void StartSpin()
    {
        Debug.Log("Updated spinning state");
        SetState(ReelStates.SPINNING);
    }

    public void SetFinishSpin(){SetState(ReelStates.STOPPING);}

    public void SetState(ReelStates newState){ state = newState;}

    public void Spin()
    {
         while (index < 6)
            {
                RectTransform thisTransform = reelTapeTransforms.Peek();
                float newYPos = thisTransform.anchoredPosition.y - 60f;
                thisTransform.anchoredPosition = new Vector2(0,newYPos);
                reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue());
                index++;
            }
            while (index < reelTapeTransforms.Count)
            {
                reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue());
                index++;
            }
            index = 0;
            if (reelTapeTransforms.Peek().anchoredPosition.y <= -880)
            {
                RectTransform ourTransform = reelTapeTransforms.Dequeue();
                ourTransform.anchoredPosition = new Vector2(0,200);
                reelTapeTransforms.Enqueue(ourTransform);

            }
    }
        
    public void Update()
    {
        switch (state)
        {
            case ReelStates.SPINNING:
            {
                Spin();
                return;
            }
            case ReelStates.STOPPING:
            {
                FinishSpin();
                return;
            }
            case ReelStates.WAITING:
            {
                FixAlignment();
                return;
            }
        }
    }
            
}
