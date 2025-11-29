using UnityEngine;
using TMPro;

public class PlayerAppearance : MonoBehaviour
{
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer turretRenderer;
    public TMP_Text nameText;


    void Start()
    {
        // We only apply appearance for the local player.
        // Remote players will get values later once networking sync is added.
    }

    public void ApplyAppearance(string name, int colorIndex)
    {
        nameText.text = name;

        if (colorIndex == 0)
        {
            bodyRenderer.color = Color.red;
            turretRenderer.color = Color.red;
        }
        else if(colorIndex == 1) 
        {
            bodyRenderer.color = Color.blue;
            turretRenderer.color = Color.blue;
        }else if(colorIndex == 2)
        {
            bodyRenderer.color = Color.green;
            turretRenderer.color= Color.green;
        }
        else
        {
            bodyRenderer.color = Color.white;
            turretRenderer.color = Color.white;
        }
    }

    
}
