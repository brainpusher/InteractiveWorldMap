using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using System.Linq;

public class MainScript : MonoBehaviour
{
    [SerializeField] private TextAsset jsonFile;
    
    [SerializeField] private GameObject mapMarkerPrefab;
    [SerializeField] private GameObject horizontalTextLinePrefab;

    [SerializeField] private GameObject Content;
    //TODO: отправить код, связанный с панелями в какой-нибудь PanelHandler(PanelManager)
    [SerializeField] private GameObject countriesInfoPanel;
    [SerializeField] private GameObject countriesCountPanel;
    [SerializeField] private GameObject countriesListPanel;
    //TODO: сделать то же самое с кнопками
    [SerializeField] private Button clearListButton;
    [SerializeField] private Button openListButton;
    
    //TODO:конечно же это все тоже должно быть не здесь
    //false -> "descending", true -> ascending
    private bool m_AreaSortingOrder = false;
    private bool m_PopulationSortingOrder = false;
    private bool m_GdpSortingOrder = false;
    
    private Countries m_CountriesInJson;
    private GameObject m_LastClickedMapMarkerPrefab = null;

    private GameObject m_modelPrefab = null;
    private GameObject m_MarkPrefab = null;
    private CanvasManager m_CanvasManager;
    private Stack<GameObject> m_ClickedCountries;
    private Stack<GameObject> m_DisabledMarkers;
    private List<GameObject> m_InstantiatedLinesWithText;
    private Country lastClickedCountry = null;
    //private List
    private Ray ray;
    private RaycastHit hit;
    private bool m_SelectionModeActivated = false;
    float temps = 0;
    void Awake()
    {
        m_CanvasManager = GameObject.FindObjectOfType<CanvasManager>();
    }

    void Start()
    {
        m_CountriesInJson = JsonUtility.FromJson<Countries>(jsonFile.text);
        
        m_ClickedCountries = new Stack<GameObject>();
        m_DisabledMarkers = new Stack<GameObject>();
        m_InstantiatedLinesWithText = new List<GameObject>();
        
        countriesInfoPanel.SetActive(false);
        countriesCountPanel.SetActive(false);
        countriesListPanel.SetActive(false);
        
        clearListButton = clearListButton.GetComponent<Button>();
        clearListButton.enabled = false;

        openListButton = openListButton.GetComponent<Button>();
        openListButton.enabled = false;

        GameObject textLineGameObject;
        
        foreach (Country country in m_CountriesInJson.countries)
        {
            Instantiate(mapMarkerPrefab, new Vector3(country.latitude, 1, -country.longitude),Quaternion.identity);
            textLineGameObject = Instantiate(horizontalTextLinePrefab,Content.transform);// создаем строчку из префаба
            // идем по всем тестовым полям из префаба и заполняем инфой
            foreach (Text text in textLineGameObject.GetComponentsInChildren<Text>())
            {
                switch (text.name)
                {
                    case "countryNameText":
                        text.text = country.name;
                        break;
                    case "areaText":
                        text.text = country.area.ToString();
                        break;
                    case "populationText":
                        text.text = country.population.ToString();
                        break;
                    case "gdpText":
                        text.text = country.gdp.ToString();
                        break;
                }
            }
            textLineGameObject.SetActive(false);//делаем ее неактивной
            m_InstantiatedLinesWithText.Add(textLineGameObject);
            Debug.Log(textLineGameObject.name);
        }
    }

    private GameObject LoadPrefabFromFile(string filename)
    {
        GameObject loadedObject = Resources.Load("Prefabs/" + filename) as GameObject;
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file found - please check the configuration");
        }
        return loadedObject;
    }
    void Update()
    {
        int countOfSelectedCountries = 0;
        // if left button pressed
        if ( Input.GetMouseButtonDown (0) )
        {
            temps = Time.time ;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        //if short click
        if (Input.GetMouseButtonUp(0) && (Time.time - temps) < 0.3 && !m_SelectionModeActivated){
            // the object identified by hit.transform was clicked
            if (Physics.Raycast(ray, out hit)){
                foreach (Country country in m_CountriesInJson.countries)
                {
                    if (country.latitude == hit.transform.position.x && -country.longitude == hit.transform.position.z)
                    {
                        if (hit.collider.gameObject.CompareTag("Marker"))
                        {
                            m_MarkPrefab = hit.collider.gameObject;
                            m_MarkPrefab.SetActive(false);
                            m_DisabledMarkers.Push(m_MarkPrefab);

                            if (m_modelPrefab != null)
                            {
                                Destroy(m_modelPrefab);
                            }
                            
                            m_CanvasManager.DisplayInfoAboutCountry(country);
                            lastClickedCountry = country;
                            if (m_LastClickedMapMarkerPrefab == null)
                            {
                                m_LastClickedMapMarkerPrefab = hit.collider.gameObject;
                            }
                            else
                            {
                                if (!m_MarkPrefab.Equals(m_LastClickedMapMarkerPrefab))
                                {
                                    m_LastClickedMapMarkerPrefab.SetActive(true);
                                }

                                m_DisabledMarkers.Pop();
                                m_LastClickedMapMarkerPrefab = hit.collider.gameObject;
                                m_DisabledMarkers.Push(m_LastClickedMapMarkerPrefab);
                            }

                            if (m_modelPrefab == null)
                            {
                                m_modelPrefab = Instantiate(LoadPrefabFromFile(country.capital),new Vector3(country.latitude, 2, -country.longitude),Quaternion.identity);
                                m_CanvasManager.InstantiateTextPrefab(m_modelPrefab.transform.position, country.name);
                            }
                            else
                            {
                                Destroy(m_modelPrefab);
                                m_CanvasManager.DestroyPrefab();
                                m_modelPrefab = Instantiate(LoadPrefabFromFile(country.capital),new Vector3(country.latitude, 2, -country.longitude),Quaternion.identity);
                                m_CanvasManager.InstantiateTextPrefab(m_modelPrefab.transform.position, country.name);
                            }
                            countriesInfoPanel.SetActive(true);
                        }
                        
                        break;
                    }
                }
            }
        } 
        else if (Input.GetMouseButtonUp(0) && (Time.time - temps) > 0.3)
        {
            m_SelectionModeActivated = true;
            
            countriesInfoPanel.SetActive(false);
            countriesCountPanel.SetActive(true);
            
            openListButton.enabled = true;
            clearListButton.enabled = true;

            if (m_modelPrefab != null)
            {
                m_ClickedCountries.Push(m_modelPrefab);
                m_CanvasManager.InstantiateImagePrefab(m_modelPrefab.transform.position);
                m_ClickedCountries.Push(m_CanvasManager.getInstantiatedTextPrefab());
                m_ClickedCountries.Push(m_CanvasManager.getInstantiatedImagePrefab());
                countOfSelectedCountries++;
                m_CanvasManager.DisplayCountOfSelectedCountries(countOfSelectedCountries);

                if (lastClickedCountry != null)
                {
                    FindAndActivateTextLine(lastClickedCountry);
                }
            }
            if (Physics.Raycast(ray, out hit))
            {
                foreach (Country country in m_CountriesInJson.countries)
                {
                    if (country.latitude == hit.transform.position.x && -country.longitude == hit.transform.position.z)
                    {
                        if (hit.collider.gameObject.CompareTag("Marker"))
                        {
                            FindAndActivateTextLine(country);
                            m_MarkPrefab = hit.collider.gameObject;
                            if (m_LastClickedMapMarkerPrefab != null)
                            {
                                m_LastClickedMapMarkerPrefab.SetActive(false);
                            }

                            m_MarkPrefab.SetActive(false);
                            m_DisabledMarkers.Push(m_MarkPrefab);
                            
                            m_modelPrefab = Instantiate(LoadPrefabFromFile(country.capital),new Vector3(country.latitude, 2, -country.longitude),Quaternion.identity);
                            m_CanvasManager.InstantiateTextPrefab(m_modelPrefab.transform.position, country.name);
                            m_CanvasManager.InstantiateImagePrefab(m_modelPrefab.transform.position);
                            
                            m_ClickedCountries.Push(m_modelPrefab);
                            m_ClickedCountries.Push(m_CanvasManager.getInstantiatedTextPrefab());
                            m_ClickedCountries.Push(m_CanvasManager.getInstantiatedImagePrefab());
                            countOfSelectedCountries++;
                            m_CanvasManager.DisplayCountOfSelectedCountries(countOfSelectedCountries);
                        }
                    }
                }
            }
        }
    }

    private void FindAndActivateTextLine(Country country)
    {
        for (int i = 0; i < m_InstantiatedLinesWithText.Count; i++)
        {
            //we get all text component from our Head prefab
            List<Text> textNameVariable = new List<Text>(m_InstantiatedLinesWithText[i].GetComponentsInChildren<Text>());
            //and search if the countryName = the counter selected with short click
            foreach (Text t in textNameVariable)
            {
                if (t.name.Equals("countryNameText") && t.text.Equals(country.name))
                {
                    // then we activate those object in our list
                    m_InstantiatedLinesWithText[i].SetActive(true);
                }
            }

        }
    }
    public void ReleaseCleaning()
    {
        countriesCountPanel.SetActive(false);
        m_SelectionModeActivated = false;
        
        while(m_DisabledMarkers.Count!=0)
        {
            m_DisabledMarkers.Pop().SetActive(true);
        }

        while(m_ClickedCountries.Count!=0)
        {
            Destroy(m_ClickedCountries.Pop());
        }
        clearListButton.enabled = false;
        openListButton.enabled = false;
    }

    public void OpenCountryListPanel()
    {
        if (countriesListPanel != null)
        {
            countriesListPanel.SetActive(true);
        }
    }

    public void CloseCountryListPanel()
    {
        countriesListPanel.SetActive(false);
    }

    public void ChangeAreaSortingOrder(Image imageToUpdate)
    {
        imageToUpdate.transform.Rotate(new Vector3(180,0,0));
        m_AreaSortingOrder = !m_AreaSortingOrder;
    }
    public void ChangePopulationSortingOrder(Image imageToUpdate)
    {
        imageToUpdate.transform.Rotate(new Vector3(180,0,0));
        m_PopulationSortingOrder = !m_PopulationSortingOrder;
    }
    public void ChangeGdpSortingOrder(Image imageToUpdate)
    {
        imageToUpdate.transform.Rotate(new Vector3(180,0,0));
        m_GdpSortingOrder = !m_GdpSortingOrder;
    }
}
