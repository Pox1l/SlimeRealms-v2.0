using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class TargetGroupBuilder : MonoBehaviour
{
    private CinemachineTargetGroup targetGroup;
    private bool targetsFound = false;

    void Start()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        TryFindTargets();
    }

    void Update()
    {
        // Neustále hledá, dokud nenajde oba objekty (užiteèné pøi pøechodu scén)
        if (!targetsFound)
        {
            TryFindTargets();
        }
    }

    private void TryFindTargets()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject lookAhead = GameObject.FindGameObjectWithTag("LookAheadTarget");

        if (player != null && lookAhead != null)
        {
            // Vyèistíme pole pro pøípad, že tam už nìco je
            targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];

            // Pøidáme Hráèe (Váha 3, Polomìr 0)
            targetGroup.AddMember(player.transform, 3f, 0f);

            // Pøidáme LookAheadTarget (Váha 1, Polomìr 0)
            targetGroup.AddMember(lookAhead.transform, 1f, 0f);

            targetsFound = true;
        }
    }
}