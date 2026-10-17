using UnityEngine;

public class HeroPosition : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public NPCHero SpawnedHero { get; private set; }

    public bool TrySpawn(NPCHero hero)
    {
        if (hero == null || hero.Character == null || IsOccupied || transform.childCount > 0)
            return false;

        var heroObject = Instantiate(
            hero.Character,
            transform.position,
            transform.rotation,
            transform);

        var rescueNpc = heroObject.GetComponent<RescueNpc>();
        if (rescueNpc != null)
        {
            rescueNpc.SetRuntime(new VillageNpcRuntime(hero));
        }

        SpawnedHero = hero;
        IsOccupied = true;
        return true;
    }
}
