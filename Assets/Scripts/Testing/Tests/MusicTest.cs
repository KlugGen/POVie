using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DKK.Testing
{
    public class MusicTest : Test
    {      

        public IEnumerator CorCor()
        {
            "Coroutine additional".LogDev();
            yield return null;
        }
   
        public override IEnumerator CoroutineTest(bool random = false)
        {
            yield return base.CoroutineTest(random);  
        }
    }
}