﻿using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [SerializeField] Dummy Tower1;
    [SerializeField] Dummy Tower2;
    [SerializeField] TowerGhost ghost_Tower1;
    [SerializeField] TowerGhost ghost_Tower2;

    //Look up
    Dictionary<TowerTypes, Dummy> towerLookup = new Dictionary<TowerTypes, Dummy>();
    Dictionary<TowerTypes, TowerGhost> ghostLookup = new Dictionary<TowerTypes, TowerGhost>();

    //References
    UIRendererManager ui;
    Transform camera;
    Settings settings;
    EventSystem eventSystem;

    //Status
    TowerTypes towerMode;
    Transform platformTransform;
    RaycastHit hit;
    TowerGhost currentGhost;
    bool canReceiveMouseClick = true;

    //Cache
    Plane invisiblePlane = new Plane(Vector3.up, new Vector3(0, 1, 0));

    public bool IsInPlacementMode { get; private set; }


    void Awake()
    {
        Instance = this;
        camera = Camera.main.transform;
        towerLookup = new Dictionary<TowerTypes, Dummy>()
        {
            {TowerTypes.Tower1, Tower1 },
            {TowerTypes.Tower2, Tower2 },
        };

        ghostLookup = new Dictionary<TowerTypes, TowerGhost>()
        {
            {TowerTypes.Tower1, ghost_Tower1 },
            {TowerTypes.Tower2, ghost_Tower2 },
        };

        eventSystem = EventSystem.current;
        
    }

    void Start()
    {
        ui = UIRendererManager.Instance;
        settings = Settings.Instance;
    }

    void Update()
    {
        if (IsInPlacementMode)
        {
            PlacementUpdate();
            if (PlayerPressesExitKey)
            {
                ExitPlacementMode();
            }
        }
    }

    #region Public
    public void EnterMode_Tower1() => EnterPlacementMode(TowerTypes.Tower1);
    public void EnterMode_Tower2() => EnterPlacementMode(TowerTypes.Tower2);
    #endregion

    #region Private
    void ExitPlacementMode()
    {
        //Ghost
        SetCurrentGhostVisibility(false);

        //UI
        ui.ExitTowerPlacementMode();

        //Status
        IsInPlacementMode = false;
        platformTransform = null;
        
    }

    void EnterPlacementMode(TowerTypes mode)
    {
        StartCoroutine(DelayMouseClickDetection());
        //UI
        ui.TowerPlacementMode(mode);

        //Ghost 
        SetCurrentGhostVisibility(false);
        currentGhost = ghostLookup[mode];
        SetCurrentGhostVisibility(true);

        //Status
        IsInPlacementMode = true;
        towerMode = mode;
    }

    void PlaceTower()
    {
        Dummy tower = Instantiate(towerLookup[towerMode], platformTransform.position, Quaternion.identity);
        platformTransform.GetComponent<Platform>().PlaceTower(tower);
    }
    #endregion

    void PlacementUpdate()
    {
        //Mostly updates ghost position. Also for exiting mode after placing tower.
        if (HitsAnEmptyPlatform())
        {
            platformTransform = hit.transform;
            currentGhost.SetPosition(platformTransform.position);
            SetGhostPlacementAvailability(true);

            if (ClickedMouseToPlaceTower)
            {
                PlaceTower();
                ExitPlacementMode();
            }
        }
        else if (HitsAnyCollider)
        {
            platformTransform = null;
            currentGhost.SetPosition(hit.point);
            SetGhostPlacementAvailability(false);
        }
        else
        {
            platformTransform = null;
            currentGhost.SetPosition(MousePositionOnInvisiblePlane());
            SetGhostPlacementAvailability(false);
        }
    }

    #region Minor methods and helper properties
    bool PlayerPressesExitKey => (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1));
    bool HitsPlatform => Physics.Raycast(CameraToMouseRay, out hit, 100f, settings.PlatformLayer);
    bool HitsAnyCollider => Physics.Raycast(CameraToMouseRay, out hit, 100f);
    bool ClickedMouseToPlaceTower => canReceiveMouseClick && !IsMouseOverUI && Input.GetMouseButtonDown(0);
    Ray CameraToMouseRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    bool IsMouseOverUI => eventSystem.IsPointerOverGameObject();
    bool HitsAnEmptyPlatform()
    {
        if (HitsPlatform)
        {
            if (!hit.transform.GetComponent<Platform>().HasTower)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator DelayMouseClickDetection ()
    {
        //After clicking on a UI button, do not allow detecting mouse click on the 
        //...same frame
        canReceiveMouseClick = false;
        yield return null;
        canReceiveMouseClick = true;
    }


    void SetCurrentGhostVisibility(bool isVisible)
    {
        if (currentGhost != null)
        {
            currentGhost.SetVisibility(isVisible);
        }
    }

    Vector3 MousePositionOnInvisiblePlane()
    {
        Ray ray = CameraToMouseRay;
        float distance;
        invisiblePlane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    void SetGhostPlacementAvailability(bool canPlace)
    {
        if (currentGhost != null)
        {
            currentGhost.SetPlacementAvailability(canPlace);
        }
    }
    #endregion

    //void OnGUI()
    //{
    //    GUI.Label(new Rect(20, 20, 200,  20), "mode: " + PlacementMode);
    //}
}