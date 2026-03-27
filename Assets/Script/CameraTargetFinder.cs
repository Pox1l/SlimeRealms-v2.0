using UnityEngine;
using Cinemachine;

public class CameraTargetFinder : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();

        // PÙVODNÍ KÓD ZAKOMENTOVÁN:
        // TryFindPlayer();

        TryFindTargetGroup();
    }

    void Update()
    {
        // Pokud z nìjakého dùvodu cíl zmizí a znovu se objeví
        if (vcam.Follow == null)
        {
            // PÙVODNÍ KÓD ZAKOMENTOVÁN:
            // TryFindPlayer();

            TryFindTargetGroup();
        }
    }

    /* PÙVODNÍ METODA ZAKOMENTOVÁNA
    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            vcam.Follow = player.transform;
            vcam.LookAt = player.transform;
        }
    }
    */

    // NOVÁ METODA
    private void TryFindTargetGroup()
    {
        // Hledá objekt s tagem "targetCamera"
        GameObject targetGroup = GameObject.FindGameObjectWithTag("targetCamera");

        if (targetGroup != null)
        {
            // Pøiøadí nalezenou Target Group do Follow
            vcam.Follow = targetGroup.transform;

            // LookAt se pro 2D u Cinemachine bìžnì nevyplòuje, ale pokud ho potøebuješ, odkomentuj øádek níže:
            // vcam.LookAt = targetGroup.transform; 
        }
    }
}
