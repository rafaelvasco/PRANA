using ImGuiNET;
using PRANA;
using NumVec4 = System.Numerics.Vector4;
using NumVec3 = System.Numerics.Vector3;
using NumVec2 = System.Numerics.Vector2;


namespace PRANADEMOS;

public class ImGuiDemo : Scene
{
    private NumVec3 _clearColor = new(114f / 255f, 144f / 255f, 154f / 255f);
    private RenderView _guiView;
    private bool _showTestWindow;
    private bool _showAnotherWindow;
    private readonly byte[] _textBuffer = new byte[100];
    private float f;
    private Texture2D _logo;
    private IntPtr _logoImguiHandle;

    public override void Load()
    {
        _logo = Content.Get<Texture2D>("Logo");
        _logoImguiHandle = ImGuiController.BindTexture(_logo);
        _guiView = Graphics.CreateView();
    }

    public override void Update(GameTime time)
    {
    }

    public override void Draw(GameTime time)
    {
    }

    public override void DrawImGui(GameTime time)
    {
        _guiView.ClearColor = _clearColor;

        Graphics.ApplyRenderState(RenderState.Default);

        Graphics.ApplyRenderView(_guiView);

        ImGui.Text("Hello World!");
        ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty);
        ImGui.ColorEdit3("clear color", ref _clearColor);
        if (ImGui.Button("Test Window")) _showTestWindow = !_showTestWindow;
        if (ImGui.Button("Another Window")) _showAnotherWindow = !_showAnotherWindow;
        ImGui.Text(
            $"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");

        ImGui.InputText("Text input", _textBuffer, 100);
        ImGui.Text("Texture sample");
        ImGui.Image(_logoImguiHandle, new NumVec2(256, 256), NumVec2.Zero, NumVec2.One, NumVec4.One, NumVec4.One);

        if (_showAnotherWindow)
        {
            ImGui.SetNextWindowSize(new NumVec2(200, 100), ImGuiCond.FirstUseEver);
            ImGui.Begin("Another Window", ref _showAnotherWindow);
            ImGui.Text("Hello");
            ImGui.End();
        }

        if (_showTestWindow)
        {
            ImGui.SetNextWindowPos(new NumVec2(650, 20), ImGuiCond.FirstUseEver);
            ImGui.ShowDemoWindow(ref _showTestWindow);
        }
    }
}