using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainLoader : Singleton<MainLoader>
{
    [NaughtyAttributes.ReorderableList]
    public List<LoaderSet> loaders = new List<LoaderSet>();
   
    public Image loaderImage;

    public int activeLoaderIndex = 0;

    public float frameRate = 1f;
    // Start is called before the first frame update

    [NaughtyAttributes.Button("Enable")]
    public void Enable()
    {
        gameObject.SetActive(true);
        //Debug.Log(loaderImage.rectTransform.sizeDelta);

        //loaderImage.rectTransform.sizeDelta = Vector2.one*loaders[activeLoaderIndex].size;
        //loaderImage.sprite = loaders[activeLoaderIndex].sprites[0];
        //arrayCounter = 0;

        //StartCoroutine("Animation");
    }

    [NaughtyAttributes.Button("Disable")]
    public void Disable()
    {
        gameObject.SetActive(false);
        StopCoroutine("Animation");
    }

    int arrayCounter = 0;

    IEnumerator Animation()
    {
        while (true)
        {
            loaderImage.sprite = loaders[activeLoaderIndex].sprites[arrayCounter];

            yield return new WaitForSeconds(loaders[activeLoaderIndex].speed);

            if (arrayCounter < loaders[activeLoaderIndex].sprites.Count - 1)
                arrayCounter++;
            else
                arrayCounter = 0;
        }
    }
}

[System.Serializable]
public class LoaderSet
{
    public List<Sprite> sprites = new List<Sprite>();
    public float size = 250f;
    public float speed = .2f;

}
