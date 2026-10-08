using System.Collections.Generic;
using UnityEngine;

public class MapSection : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> compatibleSections = new ();
    public List<GameObject> CompatibleSections => compatibleSections;
}
