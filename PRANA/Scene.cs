namespace PRANA;

public abstract class Scene
{
    public abstract void Load();

    public abstract void Unload();

    public abstract void Update(GameTime time);

    public abstract void Draw(GameTime time);
}

public class EmptyScene : Scene
{
    public override void Load()
    {
    }

    public override void Unload()
    {
    }

    public override void Update(GameTime time)
    {
    }

    public override void Draw(GameTime time)
    {
    }
}
