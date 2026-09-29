//using NUnit.Framework;
using Elements;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace DKK.Testing
{
    public class PerspectivesTestUnit : Test
    {
        private List<Cities> cities = new List<Cities>();

        public override void BeforeAction()
        {
            base.BeforeAction();
            cities = ElementsDatabase.Instance.GetPlayableCities();
        }
        /// <summary>
        /// Check if all scales are equal to Vector3.one
        /// </summary>
        //[Test]
        public void ScaleTest()
        {
            foreach (Cities city in cities)
            {
                if (city.IsReady())
                {
                    //city.name.LogDev("Checking: ");
                    //Assert.AreEqual(true, city.GetController().CheckScales());
                }
            }
        }

        public void ShadowsForZoomed()
        {
            foreach(Cities c in cities)
            {
                bool zoomed = c.type == CitiesGameplayType.zooming;
                bool shad = c.showMimics;

                if(zoomed != shad)
                {
                    Debug.LogError(c.name);
                }

                if (shad != c.HasRandomizedDropSpots())
                {
                    Debug.LogError(c.name);
                }

                //Assert.AreEqual(zoomed, shad);                
            }
        }

        public void CheckAvatars()
        {
            foreach (Cities c in cities)
            {
                //Assert.AreNotEqual(null,c.avatar);
            }
        }

        public void CheckHashtags()
        {
            
        }

        public void CheckMagenta()
        {

        }

        public void CheckGroups()
        {
           
        }


        //public override IEnumerator CoroutineTest(bool random = false)
        //{
        //    yield return base.CoroutineTest(random);
        //}
    }
}