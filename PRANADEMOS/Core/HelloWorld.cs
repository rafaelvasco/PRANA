using PRANA;

namespace PRANADEMOS;

public class HelloWorld : Scene
{
    public HelloWorld()
    {
        //Game.TargetFrameRate = 144;
        //Game.IsFixedTimeStep = false;
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
    }

    public override void Update(GameTime time)
    {
        if (Input.KeyPressed(Key.Escape))
        {
            Game.Exit();
        }
    }

    public override void Draw(GameTime time)
    {
        
    }
}