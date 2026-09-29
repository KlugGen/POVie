using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterController : MonoBehaviour
{
    public string helpMessage;
    public List<ElementsPair> pairs = new List<ElementsPair>();    
    public int correctDropsExpected;
    public Color letterColor;

    private int correctDrops = 0;

    public bool specialLetter = false;

    [HideInInspector]
    public GameObject textOverlay;

    public bool CorrectDrop(SpotController spotController, bool withShowing = true)
    {        
        correctDrops++;
        ElementsPair ep = GetElementPairBySpotController(spotController);
        if(ep != null)
        {
            ep.done = true;
        }
        else
        {
            "element pair null".LogDev();
        }

        //PrintOneLeft();

        if (correctDrops >= correctDropsExpected)
        {
            pairs.ForEach(o => o.spotController.InitializeTarget(o.element));

            if (specialLetter)
            {
                textController.specialUnlocked = true;

                if(withShowing)
                    textController.ShowAllReadyLetters();
            }

            return true;
        }

        return false;
    }

    public bool HasSome() {
        return correctDrops == correctDropsExpected;
    }

    public void Reset()
    {
        correctDrops = 0;
        pairs.ForEach(o => o.done = false);
        HideLetter();
    }

    public void PrintOneLeft()
    {
        List<ElementsPair> pairsLeft = pairs.FindAll(o => o.done == false);
        if (pairsLeft.Count > 0)
            pairsLeft[0].spotController.spot.degrees.LogDev("First undone degrees: ");
    }

    public struct CompletnessLevel
    {
        public int correctDrops;
        public int allElements;
    }

    public CompletnessLevel CompletenessLevel()
    {
        return new CompletnessLevel() { allElements = correctDropsExpected, correctDrops = correctDrops };
    }

    //[HideInInspector]
    public TextOverlayController textController;

    public void ShowLetter()
    {
        //if (textController.specialUnlocked)
        // {
        TextDeconstructionController.Instance.sentenceText.gameObject.SetActive(true);
            textOverlay.SetActive(true);
            pairs.ForEach(o => o.spotController.InitializeTarget(o.element));
            pairs.ForEach(o => o.spotController.EnableAnimation());
       // }
    }
    public void HideLetter()
    {
        //"hide letters".LogDev();
        //pairs.ForEach(o => o.spotController.InitializeTarget(o.element));
        //GetComponent<DKK.SimpleAnimation>().ResetAnimation();
    }

    public GameObject GetMyGameObject(SpotController spot) {
        foreach(ElementsPair ep in pairs)
        {
            if (ep.spotController == spot)
                return ep.element;
        }

        return null;
    }

    public ElementsPair GetElementPairBySpotController(SpotController spotController)
    {
        return pairs.Find( o => o.spotController == spotController);
    }
}

[System.Serializable]
public class ElementsPair
{
    public bool done = false;
    public SpotController spotController;
    public GameObject element;
    public int degrees = 0;
}
