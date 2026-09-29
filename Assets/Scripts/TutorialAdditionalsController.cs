using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialAdditionalsController : View
{
    static private TutorialAdditionalsController instance;

    static public TutorialAdditionalsController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(TutorialAdditionalsController))[0] as TutorialAdditionalsController;

            return instance;
        }
    }

    private void Awake()
    {        
        //SecondTutorial();
        tutorials = new List<GameObject>() { tutorialII, tutorialIII, tutorialIV, tutorialV, tutorialVI, tutorial_new, tutorial_new_2, tutorial_new_3, tutorial_new_4 };
    }


    public override void Enable()
    {
        onClickAction = null;
        base.Enable();       
    }

    private List<GameObject> tutorials = new List<GameObject>();

    public GameObject tutorialII, tutorialIII, tutorialIV, tutorialV, tutorialVI, tutorial_new, tutorial_new_2, tutorial_new_3, tutorial_new_4;

    public Text tutorialIV_text;
    public Text tutorialVI_text;

    public Text tutorialII_text;
    
    private UnityEngine.Events.UnityAction onClickAction = null;

    public void SecondTutorial()
    {
        UserController.Instance.GetStats().AddInt("tut2" + UserController.Instance.GetLanguage(), 1);
        Enable();
        tutorial_new.SetActive(true);
        //tutorialIII.SetActive(false);
        //tutorialII.SetActive(true);

        //onClickAction = delegate ()
        //{
        //    tutorialII.SetActive(false);
        //    tutorialIII.SetActive(true);

        //    onClickAction = delegate ()
        //    {
        //        Disable();
        //    };
        //};

        onClickAction = delegate ()
        {
            SecondTutorial_StepII();
        };
    }

    public void SecondTutorial_StepII()
    {
        tutorial_new.SetActive(false);
        tutorial_new_2.SetActive(true);

        onClickAction = delegate ()
        {
            tutorial_new_2.SetActive(false);
            tutorial_new_3.SetActive(true);
            ShakeDetector.Instance.enabled = true;
            ShakeDetector.Instance.gameObject.SetActive(true);
            tutorialII_text.text = TextTranslationModule.GetWord("Shake your device to remove objects");
            //onDragableBar.EnableWithoutArrows(TextTranslationModule.GetWord("Shake your device to remove objects"));
            ShakeDetector.Instance.OnShake = null;
            ShakeDetector.Instance.RegisterOnShake(delegate (ShakeData data)
            {
                ShakeDetector.Instance.gameObject.SetActive(false);

                tutorialII_text.text = TextTranslationModule.GetWord("Swipe left or right to remove sand");


                SwipeDetector.Instance.OnSwipe = null;
                SwipeDetector.Instance.RegisterOnSwipe(delegate (SwipeData data1)
                {
                    if (data1.SwipeTime > .2f)
                    {
                        SandDistractorController.Instance.Disable();
                        SwipeDetector.Instance.gameObject.SetActive(false);

                        tutorialII_text.text = TextTranslationModule.GetWord("Tap on drones to shoot them down");

                        DronesDistractorController.Instance.OnClicked = null;
                        DronesDistractorController.Instance.RegisterOnClicked(delegate ()
                        {
                            DronesDistractorController.Instance.Disable();

                            SecondTutorial_StepIII();

                        });
                        DronesDistractorController.Instance.EnableRandom();
                    }
                });

                SwipeDetector.Instance.gameObject.SetActive(true);
                SandDistractorController.Instance.Enable();
                //S8();
            });
        };
    }

    public void SecondTutorial_StepIII()
    {
        tutorial_new_3.SetActive(false);
        tutorial_new_4.SetActive(true);
        onClickAction = delegate ()
        {
            Disable();
        };
    }

    public void OnClick()
    {
        if(this.onClickAction != null)
        {
            this.onClickAction.Invoke();
        }
    }

    [NaughtyAttributes.Button("3rd tut")]
    public void ThirdTutorial()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Enter playmode first!");
            return;
        }

        UserController.Instance.GetStats().AddInt("tut3" + UserController.Instance.GetLanguage(), 1);
        Enable();
       //tutorialIV_text.text = string.Format(TextTranslationModule.GetWord("You can use your accumulated {0} to get hints where angle drop areas are by clicking on it"), "\u2605");
        tutorialIV.SetActive(true);

        onClickAction = delegate ()
        {
            tutorialIV.SetActive(false);
            tutorialV.SetActive(true);

            onClickAction = delegate ()
            {
                Disable();
            };
        };
    }

    [NaughtyAttributes.Button("Fourth")]
    public void FourthTutorial()
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("Enter playmode first!");
            return;
        }
       
        Enable();
        tutorialVI.SetActive(true);

        //tutorialVI_text.text = string.Format(TextTranslationModule.GetWord("{0} ticket price is shown next to city."), "\u2605");
        //tutorialVI_text.text += "\n\n";
        //tutorialVI_text.text += string.Format(TextTranslationModule.GetWord("If you have enough stars, ticket price will be automatically deducted from your {0} balance."), "\u2605");
        //tutorialVI_text.text += "\n\n";
        //tutorialVI_text.text += string.Format(TextTranslationModule.GetWord("If your balance is not enough, you can buy or wait for the {0} bonus."), "\u2605");

        tutorialVI_text.text = string.Format(TextTranslationModule.GetWord("Ticket price, shown next to the city, will be automatically deducted from your {0} balance"), "\u2605");
        

onClickAction = delegate ()
        {
            Disable();
        };
    }



    public override void Disable()
    {
        //"D".Log();
        base.Disable();
        onClickAction = null;
        tutorials.ForEach(delegate(GameObject o) {
            if (o != null)
                o.SetActive(false);
        });

        ShakeDetector.Instance.enabled = false;
        ShakeDetector.Instance.gameObject.SetActive(false);
        ShakeDetector.Instance.OnShake = null;


        SandDistractorController.Instance.Disable();
        SwipeDetector.Instance.gameObject.SetActive(false);
        SwipeDetector.Instance.OnSwipe = null;

        DronesDistractorController.Instance.Disable();
        DronesDistractorController.Instance.OnClicked = null;
        DronesDistractorController.Instance.OnComplete = null;
    }
}
