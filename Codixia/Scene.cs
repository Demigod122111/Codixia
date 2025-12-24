namespace Codixia;

/// <summary>
/// Represents a scene in the game.
/// </summary>
public abstract class Scene
{
    protected Scene() { }

    public abstract void Render();
    public abstract void Update(float dt);
}
