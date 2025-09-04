using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;



[CreateAssetMenu(fileName = "Default EffectDeck",menuName = "GameCore/EffectDeck")]
public class SoEffectDeck : ScriptableObject
{

    [InlineEditor]
    [LabelText("特效so合集")]
    public List<SO_Effects> EffectsList;

    public SO_Effects GetEffect(string key)
    {

        foreach (var item in EffectsList)
        {
            if (item.key == key)
            {
                return item;

            }
        }
        
        return null;
    }


}
