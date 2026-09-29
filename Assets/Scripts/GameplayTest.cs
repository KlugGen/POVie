using Elements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DKK.Testing
{
    public class GameplayTest : Test
    {
        public Button hashtagButton;
        public Button gameplayBackButton;
        public Button gameplayBackConfirmButton;
        public GameObject citiesListContainer;

        public override IEnumerator CoroutineTest(bool random = false)
        {            
            var cities = ElementsDatabase.Instance.GetPlayableCities();

            

            foreach (Cities c in cities)
            {
                c.name.Log("Processing:" ,true);
                if (c.country != null)
                {
                    if(c.country.mapPortrait != null)
                    {
                        if (c.country.mapPortrait.activeInHierarchy)
                        {
                            Transform mapButtonTransform = c.country.mapPortrait.transform;
                            Transform button =  mapButtonTransform.Find("Button");
                            
                            if(button!= null)
                            {
                                Button uiButton = button.GetComponent<Button>();
                                if(uiButton != null)
                                {
                                    c.name.Log("Start Testing: ",true);

                                    // click map marker
                                    uiButton.onClick.Invoke();
                                    yield return new WaitForEndOfFrame();

                                    // choose first city from list and play
                                   Button b =  TestUnit.GetFirstButtonFromParent(citiesListContainer);
                                    if (b == null)
                                        continue;

                                    b.onClick.Invoke();
                                    yield return new WaitForEndOfFrame();
                                    yield return new WaitForEndOfFrame();

                                    // skip hashtag screen
                                    hashtagButton.onClick.Invoke();
                                    yield return new WaitForEndOfFrame();
                                    yield return new WaitForEndOfFrame();

                                    while (!GamePlayController.Instance.generalGameplayEnabled)
                                    {
                                        yield return null;
                                    }


                                    // exit from the gameplay
                                    gameplayBackButton.onClick.Invoke();
                                    yield return new WaitForEndOfFrame();
                                    yield return new WaitForEndOfFrame();

                                    // confirm exit from the gameplay
                                    gameplayBackConfirmButton.onClick.Invoke();
                                    yield return new WaitForEndOfFrame();
                                    yield return new WaitForEndOfFrame();

                                }
                            }                          
                        }
                    }
                }
            }

            yield break;
        }
    }
}
