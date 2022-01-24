using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KartColourFactory
{
    public Color PlayerKartColour = Color.red;
    public Color[] AIKartColours;

    private readonly HashSet<Color> usedAIColours = new();

    public void PaintKarts(List<ArcadeKart> karts)
    {
        if (AIKartColours == null || AIKartColours.Length == 0)
        {
            AIKartColours = GenerateRandomColours(karts.Count);
        }

        for (int i = 0; i < karts.Count; i++)
        {
            if (karts[i].TryGetComponent(out UniversalInput _))
            {
                PaintPlayerKart(karts[i]);
            }
            else
            {
                PaintAIKart(karts[i]);
            }
        }
        usedAIColours.Clear();
    }

    public void PaintPlayerKart(ArcadeKart kart)
    {
        PaintKart(kart, PlayerKartColour);
    }

    public void PaintAIKart(ArcadeKart kart)
    {
        Color aiColour = PickAIColour();
        bool applied = PaintKart(kart, aiColour);
        if (applied)
        {
            usedAIColours.Add(aiColour);
        }
    }

    public bool PaintKart(ArcadeKart kart, Color colour)
    {
        int childCount = kart.gameObject.transform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            GameObject Child = kart.gameObject.transform.GetChild(i).gameObject;
            if(Child.TryGetComponent(out MeshRenderer renderer))
            {
                renderer.materials[0].color = colour;
                return true;
            }
        }
        return false;
    }

    private Color PickAIColour()
    {
        for (int i = 0; i < AIKartColours.Length; i++)
        {
            Color colour = AIKartColours[i];
            if (usedAIColours.Contains(colour))
            {
                continue;
            }
            else
            {
                return colour;
            }
        }
        // all colours used;
        uint safetyLimit = 100000000;
        uint runs = 0;
        while (true)
        {
            if(runs > safetyLimit)
            {
                Debug.LogError("Failed to Find Unquie Colour");
                break;
            }
            for (int i = 0; i < AIKartColours.Length; i++)
            {
                Color colourA = AIKartColours[i];
                Color colourB = AIKartColours[(i + 1) % AIKartColours.Length];
                Color colourC = Color.LerpUnclamped(colourA, colourB, Random.Range(0f, 1f));

                if (usedAIColours.Contains(colourC))
                {
                    continue;
                }
                else
                {
                    return colourC;
                }
            }
            runs++;
        }

        return Color.red;
    }

    private Color[] GenerateRandomColours(int kartCount)
    {
        Color[] colours = new Color[kartCount];
        for (int i = 0; i < kartCount; i++)
        {
            colours[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
        return colours;
    }
}
