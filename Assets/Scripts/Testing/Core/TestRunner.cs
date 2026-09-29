using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DKK.Testing
{
    [ExecuteInEditMode]
    public class TestRunner : MonoBehaviour
    {
        static private TestRunner instance;

        static public TestRunner Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.FindObjectsOfTypeAll(typeof(TestRunner))[0] as TestRunner;

                return instance;
            }
        }

        public bool randomTests = false;

        public List<Test> tests = new List<Test>();

        [NaughtyAttributes.Button("Run test")]
        public void Run()
        {            
            StartCoroutine("Test");
        }

        private IEnumerator Test()
        {
            "= Starting tests".LogDev();
            foreach (Test t in tests)
            {
                if (!t.readyToGo)
                    continue;

                t.BeforeAction();

                t.testName.LogDev("== Starting test: ");
                for (int i = 0; i < t.iterations; i++)
                {
                    if(t.iterations > 1)
                        i.LogDev("== Iteration no: ");
                    yield return new WaitForEndOfFrame();                  
                    yield return t.CoroutineTest(randomTests);                  
                }

                t.AfterAction();

            }

            "Ending tests".LogDev();
            yield return null;
        }
    }
}
