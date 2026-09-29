using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DKK;

public class KaroCutPass : MonoBehaviour
{
    public Material prefMaterial;
    public Image mainImage;
    public List<Image> karoImages = new List<Image>();

    private List<KaroObj> karoObjects = new List<KaroObj>();

    private Rect mainImgRect;

    public class KaroObj
    {
        private KaroCutPass ctrl;

        public RectTransform rt;
        public Rect rect;
        private GridLayoutGroup _grid;
        public GridLayoutGroup grid
        {
            get
            {
                if (_grid == null)
                {
                    _grid = rt.parent.GetComponent<GridLayoutGroup>();
                }
                return _grid;
            }
            set
            {
                _grid = value;
            }
        }
        public Image img;
        public Material material;

        public KaroObj(Image image, KaroCutPass ctrl)
        {
            this.ctrl = ctrl;
            img = image;
            material = new Material(ctrl.prefMaterial);
            img.material = material;
            img.material.mainTexture = ctrl.mainImage.mainTexture;
            rt = img.rectTransform;
            rect = rt.rect;
            //if (rect.width == 0)
            //{
            //    Debug.Log(grid.cellSize.x);
            //}
        }

        public void Clear()
        {
            Destroy(material);
        }

        public void Refresh()
        {
            float aspect = (float)ctrl.mainImage.mainTexture.width / (float)ctrl.mainImage.mainTexture.height;
            Vector2 scale;
            float xd;
            float yd;
            if (ctrl.mainImgRect.width / ctrl.mainImgRect.height > ctrl.mainImage.mainTexture.width / ctrl.mainImage.mainTexture.height)
            {
                float a = rt.rect.height;

                // if (a == 0)
                //     LayoutRebuilder.ForceRebuildLayoutImmediate(grid.GetComponent<RectTransform>());

                if (a == 0)
                {
                    a = grid.cellSize.y;
                }
                float x = a / (ctrl.mainImgRect.height * aspect);
                float y = a / ctrl.mainImgRect.height;
                scale = new Vector2(x, y);
                xd = 0.5f - scale.x / 2;
                yd = 0.5f - scale.y / 2;
            }
            else
            {
                float a = rt.rect.width;

                if (a == 0)
                {
                    Debug.Log(rt.name);
                    a = grid.cellSize.x;
                    //Debug.Log(grid.cellSize.x);
                }
                float x = a / ctrl.mainImgRect.width;
                float y = a / (ctrl.mainImgRect.width / aspect);
                scale = new Vector2(x, y);
                xd = 0.5f - scale.x / 2;
                yd = 0.5f - scale.y / 2;
            }

            Vector2 vec = UICalc.ImagePixel(rt.position, ctrl.mainImage);
            Vector2 diff = new Vector2((vec.x - (float)ctrl.mainImage.mainTexture.width / 2) / ctrl.mainImage.mainTexture.width + xd, (vec.y - (float)ctrl.mainImage.mainTexture.height / 2) / ctrl.mainImage.mainTexture.height + yd);

            material.SetVector("_offset", diff);
            material.SetVector("_scale", scale);
            material.SetFloat("_rotate", Mathf.Deg2Rad * rt.eulerAngles.z);
        }
    }

    IEnumerator Start()
    {
        enabled = false;
        yield return new WaitForEndOfFrame();
        enabled = true;
        mainImgRect = mainImage.rectTransform.rect;
        foreach (Image img in karoImages)
        {
            karoObjects.Add(new KaroObj(img, this));
        }
    }

    // UPDATE TYLKO DLA DYNAMICZNYCH KAFELKÓW, JEŚLI NIE MA TO WYŁĄCZYĆ !
    void Update()
    {
        Refresh();
    }

    void Refresh()
    {
        foreach (KaroObj ko in karoObjects)
        {
            ko.Refresh();
        }
    }

    public void Clear()
    {
        foreach (KaroObj ko in karoObjects)
        {
            ko.Clear();
        }
        karoObjects.Clear();
    }
}
