using UnityEngine;
using System.Net;
using System;
using System.IO;
using System.Text;
using UnityEngine.Networking;

[Serializable]
public class Score
{
    public string nombre;
    public int id;
    public int videogames_id;
    public int score;
    public string created_at;
    public string updated_at;
    public string nameGame;
    [SerializeField] private string endPoint_api;

    public void ConsultarTop3()
    {
        string uri = endPoint_api + "score/best/" + nameGame;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
            }
        }
    }

    public Score[] ConsultarTop10()
    {
        string ednPoint_top = endPoint_api + "score/best/galaga";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ednPoint_top);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();
        Debug.Log(jsonResponse + "<<<<<<<<<<<");
        Score[] score = JsonHelper.FromJson<Score>(jsonResponse);
        return score;
    }
    public void GuardarScore()
    {
        WebRequest request = WebRequest.Create(endPoint_api + "guardarData/galaga");
        request.Method = "POST";
        string postData = JsonUtility.ToJson(this);
        Debug.Log(">>>>>>>>>>>>" + postData);
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;

        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();
        WebResponse response = request.GetResponse();
        using (dataStream = response.GetResponseStream())
        {
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
        }

        // Close the response.
        response.Close();
    }
}