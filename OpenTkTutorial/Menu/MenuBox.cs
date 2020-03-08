using OpenTK;
using OpenTkTutorial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTkTutorial.Menu
{
    public class MenuBox : Cube
    {
        Vector3 Color = new Vector3(0f, 0f, 0f);
        Vector3[] Size;
        public MenuBox(Vector3 color) : base()
        {
            VertCount = 8;
            IndiceCount = 36;
            ColorDataCount = 8;
            Color = color;
            Size = new Vector3[] {
                new Vector3(-0.5f/4, -0.5f/4,  0),
                new Vector3(0.25f/4, -0.5f/4,  0),
                new Vector3(0.25f/4, 0.25f/4,  0),
                new Vector3(-0.5f/4, 0.25f/4,  0),
                new Vector3(-0.5f/4, -0.5f/4,  0),
                new Vector3(0.25f/4, -0.5f/4,  0),
                new Vector3(0.25f/4, 0.25f/4,  0),
                new Vector3(-0.5f/4, 0.25f/4,  0),
            };
        }
        public override Vector3[] GetVerts()
        {
            return Size;
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
