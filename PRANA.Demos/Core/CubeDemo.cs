using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class CubeDemo : Scene
{
    private RenderView _view;
    private RenderState _state;
    private float _rotationX;
    private float _rotationY;

    private QuadVertexStream _cube;

    private Texture2D _texture;

    public override void Load()
    {
        _view = Graphics.CreateView();

        _texture = Content.Get<Texture2D>("commander_keen");

        _view.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            Calc.ToRadians(60.0f),
            (float)Game.WindowSize.Width/Game.WindowSize.Height,
            0.01f,
            10.0f

        );

        _view.Transform =
            Matrix.CreateLookAt(new Vector3(0f, 1.5f, 5.0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f));

        _view.ClearColor = new Color(0.25f, 0.5f, 0.75f, 1.0f);

        _state = RenderState.Default;

        _cube = new QuadVertexStream("cube", VertexPCT.VertexLayout);

        var quads = new Quad[]
        {
            new(
                -1.0f, -1.0f, -1.0f, Color.Red, 0.0f, 0.0f,
                1.0f, -1.0f, -1.0f, Color.Red, 1.0f, 0.0f,
                1.0f, 1.0f, -1.0f, Color.Red, 1.0f, 1.0f,
                -1.0f, 1.0f, -1.0f, Color.Red, 0.0f, 1.0f
            ),
            new(
                -1.0f, -1.0f, 1.0f, Color.Green, 0.0f, 0.0f,
                1.0f, -1.0f, 1.0f, Color.Green, 1.0f, 0.0f,
                1.0f, 1.0f, 1.0f, Color.Green, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f, Color.Green, 0.0f, 1.0f
            ),
            new(
                -1.0f, -1.0f, -1.0f, Color.Blue, 0.0f, 0.0f,
                -1.0f, 1.0f, -1.0f, Color.Blue, 1.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, Color.Blue, 1.0f, 1.0f,
                -1.0f, -1.0f, 1.0f, Color.Blue, 0.0f, 1.0f
            ),
            new(
                1.0f, -1.0f, -1.0f, Color.Yellow, 0.0f, 0.0f,
                1.0f, 1.0f, -1.0f, Color.Yellow, 1.0f, 0.0f,
                1.0f, 1.0f, 1.0f, Color.Yellow, 1.0f, 1.0f,
                1.0f, -1.0f, 1.0f, Color.Yellow, 0.0f, 1.0f
            ),
            new(
                -1.0f, -1.0f, -1.0f, Color.Cyan, 0.0f, 0.0f,
                -1.0f, -1.0f, 1.0f, Color.Cyan, 1.0f, 0.0f,
                1.0f, -1.0f, 1.0f, Color.Cyan, 1.0f, 1.0f,
                1.0f, -1.0f, -1.0f, Color.Cyan, 0.0f, 1.0f
            ),
            new(
                -1.0f, 1.0f, -1.0f, Color.Violet, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f, Color.Violet, 1.0f, 0.0f,
                1.0f, 1.0f, 1.0f, Color.Violet, 1.0f, 1.0f,
                1.0f, 1.0f, -1.0f, Color.Violet, 0.0f, 1.0f
            )
        };

        for (int i = 0; i < 6; i++)
        {
            _cube.PushQuad(ref quads[i]);
        }
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

        float dt = (float)time.ElapsedGameTime.TotalSeconds;

        _rotationX += 2.5f * dt;
        _rotationY += 1.0f * dt;

        var rotationXMatrix = Matrix.CreateRotationX(_rotationX);
        var rotationYMatrix = Matrix.CreateRotationY(_rotationY);

        var modelMatrix = rotationXMatrix * rotationYMatrix;

        Graphics.SetModelTransform(ref modelMatrix);

        Graphics.Submit(_cube, shader: null, texture: _texture);

    }
}