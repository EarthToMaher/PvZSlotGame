using UnityEngine;

[CreateAssetMenu(fileName = "SlotIcon", menuName = "Slot Machine/Icon")]
public class SlotIcon : ScriptableObject
{
    public Sprite sprite;
    public int countInColumn = 1; // How many times this symbol appears in each column
}
