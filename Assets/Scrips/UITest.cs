using UnityEngine; 
using UnityEngine.EventSystems; 
using UnityEngine.UI; 
using TMPro; // Add this namespace for TextMeshPro

public class UITest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler 
{
    [SerializeField] private TMP_Text textToShow; // Reference to your TextMeshPro text
    
    void Start()    
    {            
        // Hide the text at start if it's not null
        if (textToShow != null)
        {
            textToShow.gameObject.SetActive(false);
        }
    }     
    
    public void OnPointerDown(PointerEventData eventData)    
    {        
        // Pressed        
        GetComponent<Image>().color = Color.red;
        
        // Show the text when clicked
        if (textToShow != null)
        {
            textToShow.gameObject.SetActive(true);
        }
    }  
    
    public void OnPointerUp(PointerEventData eventData)    
    {        
        // Released        
        GetComponent<Image>().color = Color.white;
        
        // Optional: Hide the text when released
        // if (textToShow != null)
        // {
        //     textToShow.gameObject.SetActive(false);
        // }
    } 
}