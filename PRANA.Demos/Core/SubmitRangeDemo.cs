using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class SubmitRangeDemo : Scene
{
    private VertexStream _vertices;
    private RenderView _view;
    private RenderState _state;


    public override void Load()
    {
        _view = Graphics.CreateView();

        _state = RenderState.Default;

        _vertices = new VertexStream("triangleAndQuad", VertexPCT.VertexLayout, isStatic: true);

        _vertices.Begin();

        var w = Game.WindowSize.Width;
        var h = Game.WindowSize.Height;

        _vertices.PushTriangle(
            new VertexPCT(w/2f, h/4f, 0f, Color.Red),
            new VertexPCT(w/2f - w/8f, h/2f, 0f, Color.Green),
            new VertexPCT(w/2f + w/8f, h/2f, 0f, Color.Blue)
        );

        var quad = new Quad(new RectangleF(w/2f - w/8f, h/2 + 50, w/4f, 200f));
        quad.SetColors(Color.Blue, Color.Green, Color.Yellow, Color.Red);

        _vertices.PushQuad(ref quad);

        _vertices.End();

        Graphics.MSAALevel = MSAALevel.Four;
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

        Graphics.SubmitRange(_vertices, shader: null, texture: null, startingVertexIndex:0, vertexCount: 3);
        Graphics.SubmitRange(_vertices, shader: null, texture: null, startingVertexIndex: 3,  vertexCount: 6);
    }
}