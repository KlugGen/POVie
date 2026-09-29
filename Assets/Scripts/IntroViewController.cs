using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroViewController : Singleton<IntroViewController>
{
    public Loader l1, l2, l3, l4;

    public Loader GetActiveLoader() {
        return l1;
    }
}
