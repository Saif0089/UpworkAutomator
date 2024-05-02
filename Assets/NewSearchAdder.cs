using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewSearchAdder : MonoBehaviour
{
    public TMP_InputField SearchTerm;
    public TMP_InputField WebhookURL;
    public MyRSSReader myRSSReader;
    public Button addButton;

    public void AddNew()
    {
        SearchTerm.text = SearchTerm.text.Replace(" ", "+");
        myRSSReader.AddNewSearching(SearchTerm.text, WebhookURL.text);
    }
}
