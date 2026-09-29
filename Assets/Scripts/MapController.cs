using Elements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MapController : MonoBehaviour, IScrollHandler, IPointerClickHandler
{
    public RectTransform ParentsRectTransform;
    public ScrollRect ScrollRectOfParent;
    public float MapPanAfterZoomDelay = 0.1f;
    public float ZoomAfterFocus = 0.5f;
    public bool ZoomStarted = false;

    private RectTransform rectTransform;
    private Vector2 cursorOnMapPositionFraction = new Vector2();
    private Vector3 initialScale;
    private Coroutine turnOnMapPanCoroutine;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float maxZoom = 10f;

    public void Enable()
    {
        UserController.Instance.MaxUnlockedGroup();
        SetMarkersColors();
        Initialize();       
    }

    public void SetMarkersColors()
    {
        foreach (Countries c in Elements.ElementsDatabase.Instance.countries)
        {
            if (c.mapLandscape != null)
            {
                CountryLocation cl = c.mapLandscape.GetComponent<CountryLocation>();
                SetColorForMarker(cl, c);
            }

            if (c.mapPortrait != null)
            {
                CountryLocation cl = c.mapPortrait.GetComponent<CountryLocation>();
                SetColorForMarker(cl, c);
            }
        }

        CheckUnlocking();
    }       

    private void CheckUnlocking()
    {
        //"Checking unlocking".LogDev();

        ElementsDatabase.Instance.countries.ForEach(delegate (Countries c)
        {          
            if (c != null)
            {
                c.DisableMarkers();
            }
        });

        int maxUnlockedId = -2;

        int unlockedGroupsCounter = 0;
        List<string> enabledCountryMarkers = new List<string>();

        bool blockadeFound = false;

        foreach (CountryGroup cg in ElementsDatabase.Instance.groups.OrderBy(x => x.id))
        {            
            //cg.groupName.LogDev("Checking for group: ");
            List<Cities> cities = ElementsDatabase.Instance.GetCitiesByGroup(cg.id);          

            cities.ForEach(delegate(Cities c) {
                //c.name.LogDev("Checking city: ");
                if(c.country != null)
                {
                    if (!enabledCountryMarkers.Contains(c.country.name))
                    {
                        //c.country.name.LogDev("Enabling marker for: ");
                        c.country.EnableMarkers();
                        enabledCountryMarkers.Add(c.country.name);
                    }
                    //else
                    //{
                    //    "country null".LogDev();
                    //}
                }
                //else
                //{
                //    "country null".LogDev();
                //}
            });
        
            bool undoneFound = false;
           
            cities.ForEach(delegate (Cities c)
            {
                if (!c.IsDone() && c.IsReady())
                {
                    undoneFound = true;
                }
            });

            maxUnlockedId = cg.id;

            if (cg.cooloff && !cg.serializedFields.coolOffDone)
            {
                if (cg.IsCoolOffSetUp() && !cg.HasCoolOff())
                {
                    "COOLOFF DONE!!!".Log();
                    cg.serializedFields.coolOffDone = true;
                    FirebaseDatabaseController.Instance.SaveGroups();
                }
                else
                {
                    blockadeFound = true;

                    if (string.IsNullOrEmpty(cg.serializedFields.coolOffLeft))
                    {
                        cg.serializedFields.coolOffLeft = ServerTimeController.FormatDateTime(ServerTimeController.GetTime().AddMinutes(cg.cooloffTime));
                        FirebaseDatabaseController.Instance.SaveGroups();
                    }
                }
            }
            if (undoneFound || blockadeFound)
            {
                //cg.HasCoolOff().Log("HAS COOLOFF: ");
                //cg.GetCooloffLeft().Log("COOLOFF: ");
                break;
            }

            unlockedGroupsCounter++;
        }

        if (blockadeFound)
        {
            UserController.Instance.GetStats().AddFlag("blockade", true);
            "Blockade!!".Log();
        }
        else
        {
            UserController.Instance.GetStats().RemoveFlag("blockade", true);
        }

        UserController.Instance.maxUnlockdedGroup = maxUnlockedId;
        string language = UserController.Instance.GetLanguage();

        StartCoroutine(ActualizeProgressBarsDelayed(maxUnlockedId, unlockedGroupsCounter));
        //ActualizeProgressBars(maxUnlockedId, unlockedGroupsCounter);

        /// Reset Malta marker        
        ElementsDatabase.Instance.GetCountryByName("Malta").GetMapMarker().GetComponent<CountryLocation>().ResetScale();
        ElementsDatabase.Instance.GetCountryByName("Malta").GetMapMarker().GetComponent<Animator>().enabled = maxUnlockedId == -2;
        
        int maxUnlockedId_saved = UserController.Instance.GetStats().GetInt("maxUnlockedId_saved" + language, -3);
        //maxUnlockedId.LogDev("Max unlocked id: ");
        //maxUnlockedId_saved.LogDev("Max unlocked id saved: ");
        if (maxUnlockedId > maxUnlockedId_saved)
        {
            //"new max unlocked set".LogDev();
            Countries c = ElementsDatabase.Instance.countries.Find(o => o.GetMyGroup().id == maxUnlockedId);

            //c.name.LogDev("New max found: ");

            if (c != null)
            {
                if (maxUnlockedId != -1)
                {
                    "adding unlocked marker".LogDev();
                    UserController.Instance.GetStats().AddString("unlockedCountry" + language, c.name);
                   // c.name.LogDev("Set as new unlcoked: ");
                }
                else
                {
                    //"max unlcoked == 1".LogDev();
                }
            }
            else
            {
                //"country null".LogDev();
            }

            UserController.Instance.GetStats().AddInt("tutorialShow" + language, maxUnlockedId);
            UserController.Instance.GetStats().AddInt("maxUnlockedId_saved" + language, maxUnlockedId);
            UserController.Instance.GetStats().AddInt("displayUnlocked" + language, maxUnlockedId);
            UserController.Instance.GetStats().AddInt("starButtonUnchecked" + language, 1);
        }
    }

    private IEnumerator ActualizeProgressBarsDelayed(int maxUnlockedId, int unlockedGroupsCounter)
    {
        yield return new WaitForEndOfFrame();
        ActualizeProgressBars(maxUnlockedId, unlockedGroupsCounter);
    }

    private void ActualizeProgressBars(int maxUnlockedId, int unlockedGroupsCounter)
    {     
        CountryGroup maxGroup = ElementsDatabase.Instance.groups.Find(o => o.id == maxUnlockedId);

        int allCities = Elements.ElementsDatabase.Instance.allCities.Count;
        int doneCities = Elements.ElementsDatabase.Instance.GetAllDoneCities().Count;

        if (maxGroup != null)
        {
            List<Cities> cities = ElementsDatabase.Instance.GetCitiesByGroup(maxUnlockedId);
            int maxValue = cities.Count;
            int doneValue = cities.FindAll(o => o.IsDone()).Count;
           
            MapViewController.Instance.progressNumber.ActualizeValue(doneCities, allCities);

            bool animateLast = false;

            if (UserController.Instance.GetStats().HasFlag("showNewFinishedCity"))
            {
                UserController.Instance.GetStats().RemoveFlag("showNewFinishedCity");
                animateLast = true;             
            }

            MapViewController.Instance.progressComponents.Initialize(maxValue);
            MapViewController.Instance.progressComponents.ActualizeValue(doneValue, maxValue, animateLast);
        }
    }

    private void SetColorForMarker(CountryLocation countryLocation, Countries country)
    {
        countryLocation.Initliaze(country);
    }

    public void Disable()
    {
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        ScaleToEnvelopParent();
        MoveMap(new Vector2(0.5f, 0.5f));
        initialScale = transform.localScale;
    }

    private void Update()
    {
        HandleMobileZoom();
    }

    private void ScaleToEnvelopParent()
    {
        float newScale = GetMinPossibleScale();

        transform.localScale = new Vector3(newScale, newScale, newScale);
        initialScale = transform.localScale;
    }

    public void OnScroll(PointerEventData eventData)
    {
        SetCursorOnMapPosition(Input.mousePosition);
        ZoomHandleScale(eventData.scrollDelta.y, zoomSpeed);
        MoveMapAfterScroll(Input.mousePosition);

        MapViewController.Instance.UpdateMarkersScale();
    }

    private void HandleMobileZoom()
    {
        if (Input.touchCount == 2)
        {
            ZoomStarted = true;
            ScrollRectOfParent.StopMovement();
            ScrollRectOfParent.horizontal = false;
            ScrollRectOfParent.vertical = false;

            if (turnOnMapPanCoroutine != null)
                StopCoroutine(turnOnMapPanCoroutine);

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;

            Vector2 pointBetweenTouches = GetCenteredTouch(touchZero.position, touchOne.position);

            SetCursorOnMapPosition(pointBetweenTouches);
            ZoomHandleScale(difference, zoomSpeed * 0.1f);
            MoveMapAfterScroll(pointBetweenTouches);

            MapViewController.Instance.UpdateMarkersScale();
        }
        else if (ZoomStarted)
        {
            ZoomStarted = false;
            //TODO żeby to było PO ściągnięciu dwóch palców!!!
            turnOnMapPanCoroutine = StartCoroutine(TurnOnMapPan());
        }
    }

    private IEnumerator TurnOnMapPan()
    {
        yield return new WaitForSeconds(MapPanAfterZoomDelay);

        ScrollRectOfParent.horizontal = true;
        ScrollRectOfParent.vertical = true;
    }

    private Vector2 GetCenteredTouch(Vector2 position1, Vector2 position2)
    {
        float x = (position1.x + position2.x) / 2f;
        float y = (position1.y + position2.y) / 2f;

        return new Vector2(x, y);
    }

    private void SetCursorOnMapPosition(Vector2 cursorPosition)
    {
        cursorOnMapPositionFraction = new Vector2
        {
            x = (cursorPosition.x - transform.position.x) / GetMapCurrentWidth(),
            y = (cursorPosition.y - transform.position.y) / GetMapCurrentHeight()
        };
    }

    private void MoveMapAfterScroll(Vector2 cursorPosition)
    {
        Vector2 cursorPositionOnMapInPixels = new Vector2
        {
            x = cursorOnMapPositionFraction.x * GetMapCurrentWidth(),
            y = cursorOnMapPositionFraction.y * GetMapCurrentHeight()
        };

        Vector3 newMapPosition = new Vector3
        {
            x = cursorPosition.x - cursorPositionOnMapInPixels.x,
            y = cursorPosition.y - cursorPositionOnMapInPixels.y,
            z = 0f
        };

        transform.position = newMapPosition;
    }

    private float GetMapCurrentWidth() => rectTransform.rect.width * transform.localScale.x;

    private float GetMapCurrentHeight() => rectTransform.rect.height * transform.localScale.y;

    private void ZoomHandleScale(float scrollValue, float speed)
    {
        Vector3 delta = Vector3.one * (scrollValue * speed);
        Vector3 desiredScale = transform.localScale + delta;
        transform.localScale = ClampDesiredScale(desiredScale);
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(initialScale, desiredScale);
        desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);

        return desiredScale;
    }

    public void Focus(GameObject target)
    {
        if (target is null)
            return;

        //ZoomMap(ZoomAfterFocus);
        MoveMap(target);
    }

    public void Focus(float x, float y)
    {
        Vector2 fraction = new Vector2(x, y);

        if (fraction.x > 1f || fraction.y > 1f)
        {
            Debug.LogError("MapController.Focus(): Argument out of bounds!");
            return;
        }

        //ZoomMap(ZoomAfterFocus);
        MoveMap(fraction);
    }

    public void ZoomMap(float scale)
    {
        if (scale >= GetMinPossibleScale())
            transform.localScale = new Vector3(scale, scale, scale);
        else Debug.Log("MapController.ZoomMap(): Given scale is incorrect.");

        //"Zooming".Log();
        MapViewController.Instance.UpdateMarkersScale();
    }

    private void MoveMap(GameObject target)
    {
        Vector2 percPosOfChosenLabel = GetPercPositionOfChosenLabel(target);
        float pointX = rectTransform.rect.width * percPosOfChosenLabel.x * transform.localScale.x;
        float pointY = rectTransform.rect.height * percPosOfChosenLabel.y * transform.localScale.y;

        transform.localPosition = new Vector3
        {
            x = -pointX,
            y = -pointY,
            z = transform.position.z
        };
    }

    private void MoveMap(Vector2 fractionToZoomOn)
    {
        float pointX = rectTransform.rect.width * fractionToZoomOn.x * transform.localScale.x;
        float pointY = rectTransform.rect.height * fractionToZoomOn.y * transform.localScale.y;

        transform.localPosition = new Vector3
        {
            x = -pointX,
            y = -pointY,
            z = transform.position.z
        };
    }

    private Vector2 GetPercPositionOfChosenLabel(GameObject target)
    {
        RectTransform targetRectTransform = target.GetComponent<RectTransform>();

        float xOfAll = rectTransform.rect.width / 2f + targetRectTransform.anchoredPosition.x;
        float yOfAll = rectTransform.rect.height / 2f + targetRectTransform.anchoredPosition.y;

        return new Vector2
        {
            x = xOfAll / rectTransform.rect.width,
            y = yOfAll / rectTransform.rect.height
        };
    }

    private float GetMinPossibleScale()
    {
        float mapWidthHeightRatio = rectTransform.rect.width / rectTransform.rect.height;
        float screenWidthHeightRatio = Screen.safeArea.width / Screen.safeArea.height;

        if (mapWidthHeightRatio > screenWidthHeightRatio)
            return ParentsRectTransform.rect.height / rectTransform.rect.height;
        else
            return ParentsRectTransform.rect.width / rectTransform.rect.width;
    }

    private float lastClickTime = 0f;

    public void OnPointerClick(PointerEventData eventData)
    {
        //"Pointer click handle".Log();
        float diff = Time.realtimeSinceStartup - lastClickTime;
        //diff.Log("Dff: ");

        if (diff < .3f)
        {
            SetCursorOnMapPosition(eventData.position);
            ZoomHandleScale(3f, .05f);
            MoveMapAfterScroll(eventData.position);

            MapViewController.Instance.UpdateMarkersScale();
            //ZoomMap(transform.localScale.x+.1f);
        }

        lastClickTime = Time.realtimeSinceStartup;
    }
}