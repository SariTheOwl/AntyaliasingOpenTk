using OpenTK.Graphics.OpenGL;
using System;

namespace OpenTkTutorial
{
    public class UniformInfo
    {
        public String name = "";
        public int address = -1;
        public int size = 0;
        public ActiveUniformType type;
    }
}
