using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasManager : MonoBehaviour
{
    //Todo: group variables in editor
    [Header("Country Prefabs")]
    [SerializeField] private GameObject countryNamePrefab;
    [SerializeField] private GameObject checkMarkPrefab;
    [Header("Country Info Panel")]
    [SerializeField] private Text areaText;
    [SerializeField] private Text gdpText;
    [Header("Country Count Panel")]
    [SerializeField] private Text populationText;
    
    [SerializeField] private Text selectedInfoText;
    private Text m_CountryName;
    private Image m_CheckMarkImage;
    private GameObject m_CountryNamePrefab = null;
    private GameObject m_CheckMarkPrefab = null;
    public void InstantiateTextPrefab(Vector3 position, string countryName)
    {
        m_CountryNamePrefab = Instantiate(countryNamePrefab, GameObject.FindWithTag("CountryCanvas").transform);
        m_CountryName = m_CountryNamePrefab.GetComponent<Text>();
        m_CountryName.text = countryName;
        m_CountryName.transform.position = position + new Vector3(0,2,0);
    }
    
    public void InstantiateImagePrefab(Vector3 position)
    {
        m_CheckMarkPrefab = Instantiate(checkMarkPrefab, GameObject.FindWithTag("CountryCanvas").transform);
        m_CheckMarkImage = m_CheckMarkPrefab.GetComponent<Image>();
        m_CheckMarkImage.transform.position = position + new Vector3(-1,2,1);
    }

    public void DestroyPrefab()
    {
        if (m_CountryNamePrefab != null)
        {
            Destroy(m_CountryNamePrefab);
        }
    }

    public GameObject getInstantiatedTextPrefab()
    {
        if (m_CountryNamePrefab != null)
        {
            return m_CountryNamePrefab;
        }

        return null;
    }
    
    public GameObject getInstantiatedImagePrefab()
    {
        if (m_CheckMarkPrefab != null)
        {
            return m_CheckMarkPrefab;
        }

        return null;
    }

    public void DisplayInfoAboutCountry(Country country)
    {
        areaText.text = "Площадь " + country.area + "КМ2";
        gdpText.text = "ВВП " + country.gdp + "трлн.долл";
        populationText.text = "Население " + country.population;
    }

    public void DisplayCountOfSelectedCountries(int count)
    {
        selectedInfoText.text = "Выбрано стран:" + count;
    }

}
