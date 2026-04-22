using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineManager : MonoBehaviour
{
    //Create 5 Different Reel Tapes
    //Have each Reel Tape track the Icons in a Queue
    //Have the position be determined by spot in Queue
    //Add an offset until queue updates
    
    private class ReelTape
    {
        private SlotIcon[] reelTapeIcons;
        private Queue<Sprite> reelTapeOrder;
        private RectTransform column;
        public bool spinning = false;

        private Queue<RectTransform> reelTapeTransforms;
        int index = 0;

        bool finished = false;

        public ReelTape(SlotIcon[] icons, RectTransform canvas)
        {
           reelTapeIcons = icons;
           column = canvas;
        }

        public void CreateTape(){
            
            //Create our reel with an order
            
            reelTapeTransforms = new();
            reelTapeOrder = new();
            //Create a temp list of the sprites so we can populate our reelTapeOrder
            List<Sprite> reelTapeSprites = new();

            foreach(SlotIcon icon in reelTapeIcons){
                for(int i = 0; i < icon.countInColumn; i++){
                    reelTapeSprites.Add(icon.sprite);
                }
            }

            while(reelTapeSprites.Count > 0){
                int index = Random.Range(0,reelTapeSprites.Count);

                reelTapeOrder.Enqueue(reelTapeSprites[index]);
                reelTapeSprites.RemoveAt(index);
            }

            //while (reelTapeOrder.Count > 0)
            //{
                //Debug.Log(reelTapeOrder.Dequeue());
            //}
            Visualize();
        }

        public void Visualize()
        {
            int index = reelTapeOrder.Count;
            int currentSlot = 4;
            while(index >0)
            {
                GameObject reelIcon = new GameObject();
                reelIcon.AddComponent<Image>();
                //reelIcon.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,1);
                //reelIcon.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,1);
                reelIcon.transform.SetParent(column,false);
                reelIcon.GetComponent<Image>().sprite = reelTapeOrder.Peek();
                reelTapeOrder.Enqueue(reelTapeOrder.Dequeue());
                reelTapeTransforms.Enqueue(reelIcon.GetComponent<RectTransform>());
                float yPos = currentSlot* -176;
                currentSlot--;
                if(currentSlot > -2) reelIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,yPos);
                else reelIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,176);
                index--;
            }
            spinning = true;
        }

        public void Spin()
        {
            
            //Move x amount per frame
            //When y amount has been moved update queue
            //Do this for x time
            //Wait for last cycle to end for visual clarity
        }

        public void Update()
        {
            if(reelTapeOrder.Count<=0) return;
            if (!spinning&&!finished)
            {
             while (index < 6)
            {
                //Something wrong with this line ISSUe: We are referencing the sprite and not the image, we need to make sure we have a Queue of the GOs or Images, not the Sprites
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
                finished=true;
                RectTransform ourTransform = reelTapeTransforms.Dequeue();
                ourTransform.anchoredPosition = new Vector2(0,176);
                reelTapeTransforms.Enqueue(ourTransform);
                Stack<RectTransform> slotOutcomes = new();
                while(index<5)
                {
                    RectTransform rect = reelTapeTransforms.Dequeue();
                    slotOutcomes.Push(rect);
                    reelTapeTransforms.Enqueue(rect);
                    index++;
                }
                while (index<reelTapeTransforms.Count)
                    {
                        reelTapeTransforms.Enqueue(reelTapeTransforms.Dequeue());
                        index++;
                    }
                    int count = 1;
                    while(slotOutcomes.Count > 0)
                    {
                        Debug.Log(count + ": " + slotOutcomes.Pop().GetComponent<Image>().sprite.name);
                        count++;
                    }

            }
            }
            else if(spinning){
            while (index < 6)
            {
                //Something wrong with this line ISSUe: We are referencing the sprite and not the image, we need to make sure we have a Queue of the GOs or Images, not the Sprites
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
                ourTransform.anchoredPosition = new Vector2(0,176);
                reelTapeTransforms.Enqueue(ourTransform);

            }
            if (Random.Range(0, 1000)==9)
            {
                spinning = false;
            }}
        }

        //public IEnumerator SpinCoroutine()
        //{
            
        //}
        
    }
    [Header("Slot Setup")]
    public SlotIcon[] symbolDefinitions;      // Scriptable objects containing sprite + count
    public int columnCount = 5;
    public int visibleSymbols = 5;            // what the player sees per column
    public float spinTime = 2f;
    public float deceleration = 3f;

    [Header("References")]
    public RectTransform columnPrefab;
    public Transform columnsParent;

    //private List<List<SlotIcon>> reelTapes = new();
    //private List<int> reelIndex = new();
    private List<RectTransform> columnRoots = new();

    private ReelTape[] reelTapes;

    private int fullTapeLength;

    private ReelTape reelTape;

    private void Start() {
        RectTransform column = Instantiate(columnPrefab,columnsParent);
        column.gameObject.transform.SetParent(columnsParent);
        reelTape = new ReelTape(symbolDefinitions, column);
        reelTape.CreateTape();
    }

    private void Update()
    {
        reelTape.Update();
        
    }



}