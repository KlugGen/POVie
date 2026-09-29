using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetsDatabase
{
    public class AssetsDatabase : Singleton<AssetsDatabase>
    {
        public Sprite plusButton_icon, minusButton_icon;
        public Colors colors;

        public Sprites sprites;
    }

    [System.Serializable]
    public class Colors
    {
        public Color dragableElementColorEnabled,
            dragableElementColorDisabled,
            dragableColorHoverOnCloseArea;
    }

    [System.Serializable]
    public class Sprites
    {
        public Sprite anonymousSprite;
    }
}