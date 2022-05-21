using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class HelloQuad : Scene
{
    private QuadVertexStream quadStream;
    private RenderView _view;
    private RenderState _state;
    private Texture2D _texture;


    public override void Load()
    {
        _texture = Content.Get<Texture2D>("commander_keen");

        _view = Graphics.CreateView();

        _state = RenderState.Default;

        quadStream = new QuadVertexStream("quad", VertexPCT.VertexLayout);

        var quad = new Quad(_texture);

        quad.SetXYWH(Game.WindowSize.Width/2f , Game.WindowSize.Height / 2f, _texture.Width * 2, _texture.Height * 2, 0.5f, 0.5f);

        quad.SetColors(Color.Red, Color.Red, Color.BlueViolet, Color.BlueViolet);

        quadStream.PushQuad(ref quad);
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

        Graphics.Submit(quadStream, shader: null, texture: _texture);
    }
}