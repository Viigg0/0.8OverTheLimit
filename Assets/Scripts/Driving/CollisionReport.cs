using UnityEngine;

/// <summary>
/// Detects collisions and generates a context-sensitive damage/injury description
/// based on what the car hit and the driver's current BAC level.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CollisionReport : MonoBehaviour
{
    // ── Object tag categories ─────────────────────────────────────────────────
    // Name-keyword based — no tags required.
    public enum CollisionCategory
    {
        Lamp,       // lampposts
        Car,        // parked/oncoming cars
        Building,   // walls, storefronts, facades
        Tree,       // trees
        Other,      // anything else with a collider
    }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private CarMovement carMovement;

    [Header("Impact threshold (m/s)")]
    [Tooltip("Minimum relative impact speed to trigger a report.")]
    [SerializeField] private float minImpactSpeed = 1f;

    [Header("Events")]
    [Tooltip("Fires when a reportable collision occurs. Wire this to your UI.")]
    public CollisionEvent onCollision;

    // ── Public data passed to listeners ───────────────────────────────────────
    [System.Serializable]
    public class CollisionEvent : UnityEngine.Events.UnityEvent<CollisionResult> { }

    public struct CollisionResult
    {
        public CollisionCategory category;
        public float impactSpeed;
        public float bac;
        public string title;
        public string description;
        public Severity severity;
    }

    public enum Severity { Minor, Moderate, Serious, Critical }

    // ── BAC tier helpers ──────────────────────────────────────────────────────
    private static string BACTier(float bac)
    {
        if (bac < 0.04f) return "sober";
        if (bac < 0.08f) return "slightly impaired";
        if (bac < 0.15f) return "heavily impaired";
        return "severely impaired";
    }

    private Severity SpeedSeverity(float speed)
    {
        if (speed < 5f)  return Severity.Minor;
        if (speed < 15f) return Severity.Moderate;
        if (speed < 25f) return Severity.Serious;
        return Severity.Critical;
    }

    // ── Collision detection ───────────────────────────────────────────────────
    private void OnCollisionEnter(Collision collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < minImpactSpeed) return;

        float bac = carMovement != null ? carMovement.CurrentBAC : 0f;
        CollisionCategory cat = Categorize(collision.gameObject);
        CollisionResult result = BuildResult(cat, impactSpeed, bac, collision.gameObject.name);

        onCollision?.Invoke(result);

        // Also log to console so you can see it without a UI wired up yet.
        Debug.Log($"[CollisionReport] {result.title} | BAC {bac:F3} | {impactSpeed:F1} m/s\n{result.description}");
    }

    // ── Category detection ────────────────────────────────────────────────────
    // Tags to add in Unity's Tag Manager: "Lamp", "Car", "Building"
    // Assign them on the root GameObject (or any parent) — child colliders are covered.
    private CollisionCategory Categorize(GameObject go)
    {
        // Walk up the hierarchy so child mesh colliders inherit the parent's tag.
        Transform t = go.transform;
        while (t != null)
        {
            switch (t.tag)
            {
                case "Lamp":     return CollisionCategory.Lamp;
                case "Car":      return CollisionCategory.Car;
                case "Building": return CollisionCategory.Building;
                case "Tree":     return CollisionCategory.Tree;
            }
            t = t.parent;
        }
        return CollisionCategory.Other;
    }

    // ── Description builder ───────────────────────────────────────────────────
    private CollisionResult BuildResult(CollisionCategory cat, float speed, float bac, string objectName)
    {
        Severity sev = SpeedSeverity(speed);
        string tier = BACTier(bac);
        string bacNote = bac >= 0.08f
            ? $" You were {tier} (BAC {bac:F3}), making this a criminal offence."
            : bac >= 0.04f
                ? $" Your BAC of {bac:F3} means you were {tier}."
                : "";

        string title;
        string desc;

        switch (cat)
        {
            case CollisionCategory.Lamp:
                title = "Lamppost Hit";
                desc = sev switch
                {
                    Severity.Minor    => $"You clipped a lamppost. It's dented but still standing.{bacNote}",
                    Severity.Moderate => $"You knocked a lamppost over. There's a dent in your bumper and the street is now darker.{bacNote}",
                    Severity.Serious  => $"You sheared a lamppost clean off its base at speed. The car has serious front-end damage.{bacNote}",
                    _                 => $"You demolished a lamppost at high speed. The front of your car is destroyed.{bacNote}",
                };
                break;

            case CollisionCategory.Car:
                title = sev >= Severity.Serious ? "Serious Vehicle Collision" : "Vehicle Collision";
                desc = sev switch
                {
                    Severity.Minor    => $"A minor fender bender with a parked car. Scratches on both vehicles.{bacNote}",
                    Severity.Moderate => $"You hit a parked car hard enough to set off its alarm and crumple both bumpers.{bacNote}",
                    Severity.Serious  => $"A serious crash into another car. Both vehicles have major damage; anyone inside would be badly hurt.{bacNote}",
                    _                 => $"A catastrophic collision with another vehicle. It is totalled, and so are you.{bacNote}",
                };
                break;

            case CollisionCategory.Building:
                title = "Building Collision";
                desc = sev switch
                {
                    Severity.Minor    => $"You scraped along a building wall. Paint transfer and a bent panel.{bacNote}",
                    Severity.Moderate => $"You drove into a building facade. The front end is caved in and bystanders inside are shaken.{bacNote}",
                    Severity.Serious  => $"You crashed through a building wall. Serious structural damage and likely injuries inside.{bacNote}",
                    _                 => $"You demolished part of a building at high speed. This is a mass casualty event.{bacNote}",
                };
                break;

            case CollisionCategory.Tree:
                title = "Tree Collision";
                desc = sev switch
                {
                    Severity.Minor    => $"You clipped a tree. A scratch along the side and a few leaves on your windscreen.{bacNote}",
                    Severity.Moderate => $"You hit a tree hard enough to snap branches and buckle your hood.{bacNote}",
                    Severity.Serious  => $"You crashed into a tree at speed. The front end is destroyed and the airbags deployed.{bacNote}",
                    _                 => $"You drove full speed into a tree. The car wrapped around it — unsurvivable in real life.{bacNote}",
                };
                break;

            default:
                title = "Collision";
                desc = sev switch
                {
                    Severity.Minor    => $"A minor impact with an obstacle.{bacNote}",
                    Severity.Moderate => $"You hit something solid. The car has taken visible damage.{bacNote}",
                    Severity.Serious  => $"A serious crash. Significant damage and possible injuries.{bacNote}",
                    _                 => $"A catastrophic impact at full speed.{bacNote}",
                };
                break;
        }

        return new CollisionResult
        {
            category    = cat,
            impactSpeed = speed,
            bac         = bac,
            title       = title,
            description = desc,
            severity    = sev,
        };
    }
}
