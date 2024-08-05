using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class ClassColors
{
    public Color color;
    public AxieClass @class;
}

public class ProjectileColor : MonoBehaviour
{
    public List<ClassColors> ClassColors = new List<ClassColors>();
    public ParticleSystem[] particleSystems;

    public void SetColor(AxieClass @class)
    {
        particleSystems.ToList().ForEach(x => { x.startColor = ClassColors.FirstOrDefault(y => y.@class == @class).color; });
    }

}
