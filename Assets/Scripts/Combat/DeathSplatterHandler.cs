using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private void OnEnable()
    {
        Health.OnDeath += SpawnDeathVFX;
        Health.OnDeath += SpawnDeathSplatterPrefab;
    }

    private void OnDisable()
    {
        Health.OnDeath -= SpawnDeathVFX;
        Health.OnDeath -= SpawnDeathSplatterPrefab;
    }

    private void SpawnDeathSplatterPrefab(Health sender)
    {
        GameObject newSplatterPrefab = Instantiate(sender.SplatterPrefab, sender.transform.position, sender.transform.rotation);
        SpriteRenderer spriteRenderer = newSplatterPrefab.GetComponent<SpriteRenderer>();
        spriteRenderer.color = RandomizeColor(spriteRenderer.color);
        spriteRenderer.transform.SetParent(this.transform);
    }

    private Color RandomizeColor(Color color)
    {
        float randomR = Random.Range(color.r / 2, color.r);
        float randomG = Random.Range(color.g / 2, color.g);
        float randomB = Random.Range(color.b / 2, color.b);
        return new Color(randomR, randomG, randomB);
    }

    private void SpawnDeathVFX(Health sender)
    {
        GameObject deathVFX = Instantiate(sender.DeathVFX, sender.transform.position, sender.transform.rotation);
        ParticleSystem.MainModule ps = deathVFX.GetComponent<ParticleSystem>().main;
        ps.startColor = RandomizeColor(ps.startColor.color);
    }
}
