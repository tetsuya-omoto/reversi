using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    [SerializeField] Material material = null;

    Material topMaterial = null;
    Material backMaterial = null;

    [SerializeField] MeshRenderer topCylinder = null;
    [SerializeField] MeshRenderer backCylinder = null;
    public void SetState(StageManager.eStoneState state){
        bool isActive = (state != StageManager.eStoneState.EMPTY);
        {
            topCylinder.gameObject.SetActive(isActive);
            backCylinder.gameObject.SetActive(isActive);
        }
        SetColor(state == StageManager.eStoneState.WHITE);

    }
    public void SetColor(bool isWHITE)
    {
        if (topMaterial == null)
        {
            topMaterial = GameObject.Instantiate<Material>(material);
            backMaterial = GameObject.Instantiate<Material>(material);
            topCylinder.material = topMaterial;
            backCylinder.material = backMaterial;
        }
        topMaterial.color = isWHITE ? Color.white : Color.black;
        backMaterial.color = isWHITE ? Color.black : Color.white;
    }
}
