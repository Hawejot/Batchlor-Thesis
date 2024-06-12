using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollViewButtonController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Button scrollUpButton;
    public Button scrollDownButton;
    public Sprite visibleSpriteUp;
    public Sprite visibleSpriteDown;
    public float scrollSpeed = 0.1f; // Adjust this value to control the speed of scrolling


    public void Scroll(int direction)
    {
        Debug.Log("Scroll called with direction: " + direction);
        float startNormalizedPosition = scrollRect.verticalNormalizedPosition;
           
        if (direction == 1) {

            float newPosition = startNormalizedPosition + scrollSpeed;

            scrollRect.verticalNormalizedPosition = newPosition;
  

        }else if (direction == -1) {
            float newPosition = startNormalizedPosition - scrollSpeed;

            scrollRect.verticalNormalizedPosition = newPosition;

        }
        else
        {
            Debug.Log("Wrong function call SmoothScroll");
        }
    }
}
