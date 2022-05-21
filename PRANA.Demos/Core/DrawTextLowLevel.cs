using System.Runtime.InteropServices;
using PRANA;
using PRANA.Common;

namespace PRANADEMOS;

public class DrawTextLowLevel : Scene
{
    private Font[] _fonts;
    private int _currentFontIndex;

    private QuadVertexStream _vertexStream;

    private RenderView _view;
    private RenderState _state;

    public override void Load()
    {
        _view = Graphics.CreateView();

        _state = RenderState.Default;

        _fonts = new Font[4];

        _fonts[0] = Content.Get<Font>("ChiKareGo2");

        _fonts[1] = Content.Get<Font>("EnterCommand");

        _fonts[2] = Content.Get<Font>("LinLibertine");
        
        _fonts[3] = Content.Get<Font>("MonoGram");

        _vertexStream = new QuadVertexStream("text", VertexPCT.VertexLayout);
    }

    private unsafe void DrawText(Font font, string text, Vector2 position, Color color)
    {
        var offset = Vector2.Zero;

        fixed (Glyph* glyphPtr = &MemoryMarshal.GetArrayDataReference(font.Glyphs))
        {
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                switch (c)
                {
                    case '\r':
                        continue;
                    case '\n':
                        offset.X = 0;
                        offset.Y = font.LineSpacing;
                        continue;
                }

                var currentGlyphIndex = font.GetGlyphIndexOrDefault(c);

                var pCurrentGlyph = glyphPtr + currentGlyphIndex;

                var p = offset;

                p.X += pCurrentGlyph->OffsetX;
                p += position;

                if (!pCurrentGlyph->TextureRect.IsEmpty)
                {
                    var quad = new Quad(font.Texture, pCurrentGlyph->TextureRect);
                    quad.SetColor(color);
                    quad.SetXYWH(p.X, p.Y, pCurrentGlyph->TextureRect.Width, pCurrentGlyph->TextureRect.Height, 0f, 0f);

                    _vertexStream.PushQuad(ref quad);
                }

                offset.X += pCurrentGlyph->Advance;
            }
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

        if (Input.KeyPressed(Key.Left))
        {
            _currentFontIndex--;

            if (_currentFontIndex < 0)
            {
                _currentFontIndex = 0;
            }
        }
        else if (Input.KeyPressed(Key.Right))
        {
            _currentFontIndex++;

            if (_currentFontIndex > _fonts.Length - 1)
            {
                _currentFontIndex = _fonts.Length - 1;
            }
        }
    }

    public override void Draw(GameTime time)
    {
        Graphics.ApplyRenderView(_view);
        Graphics.ApplyRenderState(_state);

        _vertexStream.Reset();

        DrawText(_fonts[_currentFontIndex], "abcdefghijklmnopqrstuvxyzw0123456789'\"!@#$%¨&*()-_=+[{]}~^.,<>;:/\\|",
            new Vector2(0, 0), Color.White);
        DrawText(_fonts[_currentFontIndex], "Extended: ãâáàíìúùûéèêõôóò", new Vector2(0, 50), Color.White);
        
        DrawText(_fonts[_currentFontIndex], "Font Texture:", new Vector2(0, 80), Color.White);

        var fontTextureQuad = new Quad(_fonts[_currentFontIndex].Texture);

        fontTextureQuad.SetXY(0, 150, 0f, 0f);

        _vertexStream.PushQuad(ref fontTextureQuad);

        Graphics.Submit(_vertexStream, shader: null, texture: _fonts[_currentFontIndex].Texture);
    }
}