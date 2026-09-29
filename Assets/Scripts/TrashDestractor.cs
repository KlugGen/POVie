using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashDestractor : Singleton<TrashDestractor>
{
    public List<RectTransform> trashes = new List<RectTransform>();
    public List<FormationController> formations = new List<FormationController>();

    public void Enable()
    {
        gameObject.SetActive(true);
        EnableWithFormation(formations[Random.Range(0,formations.Count)]);
    }

    public void EnableWithFormation(FormationController formation)
    {
        if (formation.spots.Count < trashes.Count)
        {
            Debug.Log("No places");
            return;
        }

        List<GameObject> tempSpots = new List<GameObject>(formation.spots);

        foreach (RectTransform t in trashes)
        {
            int index = Random.Range(0, tempSpots.Count - 1);
            GameObject spot = tempSpots[index];
            tempSpots.RemoveAt(index);

            RectTransform r = spot.GetComponent<RectTransform>();
            if (r != null)
            {
                Vector3 pos = spot.transform.localPosition;
                pos.y -= r.rect.height / 2;
                t.localPosition = pos;
            }
            else
                t.localPosition = spot.transform.localPosition;

            //t.position = spot.GetComponent<RectTransform>().position;
            t.Rotate(new Vector3(0f, 0f, 1f), Random.Range(10f, 200f));
        }       
    }

    public void ProperShake()
    {

    }
}
