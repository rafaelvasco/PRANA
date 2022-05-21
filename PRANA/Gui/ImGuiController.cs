using ImGuiNET;

namespace PRANA
{
    public static class ImGuiController
    {
        private static ImGuiRenderer _imguiRenderer;

        private static int _scrollWheelValue;

        private static readonly List<int> _keys = new();

        private static Dictionary<ushort, IntPtr> _textureMap;

        private static IntPtr _imguiContext;

        internal static void Init()
        {
            if (_imguiRenderer != null)
            {
                return;
            }

            _imguiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(_imguiContext);

            _imguiRenderer = new ImGuiRenderer();

            SetupInput();

            _textureMap = new Dictionary<ushort, IntPtr>();
        }

        internal static void Shutdown()
        {
            ImGui.DestroyContext(_imguiContext);
        }

        internal static void BeginGui(GameTime time)
        {
            UpdateGuiState(time);

            ImGui.NewFrame();
        }

        internal static void PresentGui()
        {
            _imguiRenderer.Present();
        }

        public static IntPtr BindTexture(Texture2D texture)
        {
            IntPtr imguiHandle = _imguiRenderer.BindTexture(texture);

            _textureMap.Add(texture.Handle.idx, imguiHandle);

            return imguiHandle;
        }

        public static void UnBindTexture(Texture2D texture)
        {
            if (_textureMap.TryGetValue(texture.Handle.idx, out var imguiTexHandle))
            {
                _imguiRenderer.UnbindTexture(imguiTexHandle);
            }
        }

        private static void UpdateGuiState(GameTime gameTime)
        {
            var io = ImGui.GetIO();

            ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < _keys.Count; i++)
            {
                io.KeysDown[_keys[i]] = Input.KeyDown((Key)_keys[i]);
            }

            io.KeyShift = Input.KeyDown(Key.LeftShift) || Input.KeyDown(Key.RightShift);
            io.KeyCtrl = Input.KeyDown(Key.LeftControl) || Input.KeyDown(Key.RightControl);
            io.KeyAlt = Input.KeyDown(Key.LeftAlt) || Input.KeyDown(Key.RightAlt);
            io.KeySuper = Input.KeyDown(Key.LeftWindows) || Input.KeyDown(Key.RightWindows);

            io.DisplaySize = new System.Numerics.Vector2(Graphics.BackBufferWidth, Graphics.BackBufferHeight);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            io.MousePos = new System.Numerics.Vector2(Input.Mouse.X, Input.Mouse.Y);

            io.MouseDown[0] = Input.Mouse.Left;
            io.MouseDown[1] = Input.Mouse.Right;
            io.MouseDown[2] = Input.Mouse.Middle;

            var scrollDelta = Input.Mouse.ScrollWheelValue - _scrollWheelValue;
            io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            _scrollWheelValue = Input.Mouse.ScrollWheelValue;
        }

        private static void SetupInput()
        {
            var io = ImGui.GetIO();

            _keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab);
            _keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left);
            _keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right);
            _keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up);
            _keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down);
            _keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp);
            _keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home);
            _keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Key.End);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Back);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Space] = (int)Key.Space);
            _keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Key.A);
            _keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Key.C);
            _keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Key.V);
            _keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Key.X);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y);
            _keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z);

            Input.OnTextInput += args =>
            {
                if (args.Character == '\t')
                {
                    return;
                }

                io.AddInputCharacter(args.Character);
            };

            ImGui.GetIO().Fonts.AddFontDefault();
        }
    }
}
