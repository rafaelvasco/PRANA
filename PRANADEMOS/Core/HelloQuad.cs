using PRANA;

namespace PRANADEMOS;

public class HelloQuad : Scene
{
    private QuadVertexStream quadStream;
    private RenderView _view;
    private RenderState _state;


    public override void Load()
    {
        _view = Graphics.CreateDefaultView();

        _state = RenderState.Default;

        quadStream = new QuadVertexStream("quad", VertexPCT.VertexLayout);

        var quad = new Quad(new RectangleF(100, 100, Game.WindowSize.Width - 200, Game.WindowSize.Height - 200));

        var quad2 = new Quad(new RectangleF(150, 150, 200, 200));

        quad.SetColors(Color.Red, Color.Blue, Color.Violet, Color.Pink);

        quadStream.PushQuad(ref quad);
        quadStream.PushQuad(ref quad2);
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
        Graphics.ApplyRenderView(_view);
        Graphics.ApplyRenderState(_state);

        Graphics.Draw(quadStream);
    }
}