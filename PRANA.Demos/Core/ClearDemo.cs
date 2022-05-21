using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class ClearDemo : Scene
{
    private RenderView _view;
    private RenderState _state;


    public override void Load()
    {
        _view = Graphics.CreateView();

        _view.ClearColor = Color.Red;

        _state = RenderState.Default;
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

        _view.ClearColor.G += 5;

        if (_view.ClearColor.G > 254)
        {
            _view.ClearColor.G = 0;
        }
    }

    public override void Draw(GameTime time)
    {
        Graphics.ApplyRenderView(_view);
        Graphics.ApplyRenderState(_state);

    }
}