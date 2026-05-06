using UnityEngine;

[CreateAssetMenu(fileName = "SlotMachineTower", menuName = "Scriptable Objects/SlotMachineTower")]
public class SlotMachineTower : ScriptableObject
{
    [Tooltip("Single Sprite Used For Displaying While Spinning")]public Sprite sprite;
    [Tooltip("Array containing how many are in each column")] public int[] countInColumn;
    public SlotOutcome[] outcomes;
    public int multiplier;
}
