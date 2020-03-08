using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTkTutorial.Models
{
    public class Pyramid : Cube
    {
        Vector3 Color = new Vector3(0f, 0f, 0f);
        public Pyramid(Vector3 color) : base()
        {
            VertCount = 8;
            IndiceCount = 36;
            ColorDataCount = 8;
            Color = color;
        }
        public override Vector3[] GetVerts()
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(0f, 0.5f,  0f),
                new Vector3(-0f, 0.5f,  0f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(0f, 0.5f,  0f),
                new Vector3(-0f, 0.5f,  0f),
            };
        }
        public override Vector3[] GetColorData()
        {
            return new Vector3[] {
            Color,
            Color,
            Color,
            Color,
            Color,
            Color,
            Color,
            Color
        };
        }
    }
}
