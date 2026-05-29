using UnityEngine;
using Photon.Pun;
using TMPro;

public class GetCode : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PhotonNetwork.InRoom)
            codeText.text = "Code: " + PhotonNetwork.CurrentRoom.Name;
        else
            codeText.text = "Não estás numa sala";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
