using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class DynamicTriangle : Scene
{
    private VertexStream triangle;
    private RenderView _view;
    private RenderState _state;

    private Color colV1;
    private Color colV2;
    private Color colV3;

    private Color colV1Target;
    private Color colV2Target;
    private Color colV3Target;

    private float t;
    
    public override void Load()
    {
        colV1 = Color.Violet;
        colV2 = Color.Blue;
        colV3 = Color.Cyan;

        colV1Target = Color.Blue;
        colV2Target = Color.Cyan;
        colV3Target = Color.Violet;

        _view = Graphics.CreateView();

        _state = RenderState.Default;

        triangle = new VertexStream("triangle", VertexPCT.VertexLayout);
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
        
        Graphics.ApplyRenderState(_state);
        Graphics.ApplyRenderView(_view);

        t += (float)time.ElapsedGameTime.TotalSeconds;

        var interpFactor = Calc.Sin(t);

        var col1 = Color.Lerp(colV1, colV1Target, interpFactor);
        var col2 = Color.Lerp(colV2, colV2Target, interpFactor);
        var col3 = Color.Lerp(colV3, colV3Target, interpFactor);

        if (t > 3.125f)
        {
            t = 0.0f;
        }

        triangle.Begin();
        triangle.PushTriangle(
            new VertexPCT(0f, Game.WindowSize.Height, 0f, col1),
            new VertexPCT(Game.WindowSize.Width, Game.WindowSize.Height, 0f, col2),
            new VertexPCT(Game.WindowSize.Width / 2f, 0f, 0f, col3)
        );

        triangle.End();

        Graphics.Submit(triangle);
    }
}