namespace PRANA;

public abstract class Scene
{
    public virtual void Load() {}

    public virtual void Unload() {}

    public abstract void Update(GameTime time);

    public abstract void Draw(GameTime time);

    public virtual void DrawImGui(GameTime time) {}
}

public class EmptyScene : Scene
{
    public override void Update(GameTime time)
    {
    }

    public override void Draw(GameTime time)
    {
    }
}
