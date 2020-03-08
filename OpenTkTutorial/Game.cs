using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTkTutorial.Menu;
using OpenTkTutorial.Models;
using OpenTkTutorial.Shaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace OpenTkTutorial
{
    class Game : GameWindow
    {
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";
        int ibo_elements;
        Vector3[] vertdata;
        Vector3[] coldata;
        List<Volume> objects = new List<Volume>();
        int[] indicedata;
        Camera cam = new Camera();
        Vector2 lastMousePos = new Vector2();
        float time = 0.0f;
        int numJitters = 25;

        // enum odpowiadajacy za tryb wlaczanego antyaliasingu - domyslnie antyaliasing jes wylaczony
        eAntyAliasingMode mode = eAntyAliasingMode.Off;

        //enum odpowiadajacy za tryb widoku 
        eInputType inputType = eInputType.Keyboard;

        double[,] jitterTable = new double[8, 2] {
    {0.5625, 0.4375},
    {0.0625, 0.9375},
    {0.3125, 0.6875},
    {0.6875, 0.8124},
    {0.8125, 0.1875},
    {0.9375, 0.5625},
    {0.4375, 0.0625},
    {0.1875, 0.3125}
};
        public Game() : base(1200, 720, new GraphicsMode(32, 24, 0, 8))
        {
            GL.Disable(EnableCap.Multisample);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            initProgram();
            Title = "Brak Antyaliasingu";
            GL.ClearColor(Color.LightSkyBlue);
            GL.PointSize(5f);
        }
        void initProgram()
        {
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            GL.GenBuffers(1, out ibo_elements);

            shaders.Add("default", new ShaderProgram("Shaders/vs.glsl", "Shaders/fs.glsl", true));
            activeShader = "default";
            ObjVolume obj1 = ObjVolume.LoadFromFile("Objects/cow.obj");
            objects.Add(obj1);

            Cube c1 = new Cube(new Vector3(0.8f, 0.3f, 0.5f));
            Cube c2 = new Cube(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f));
            Cube c3 = new Cube(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f));
            Floor f1 = new Floor(new Vector3(0f, 0.7f, 0f));
            objects.Add(c1);
            objects.Add(c2);
            objects.Add(c3);
            objects.Add(f1);

            Pyramid p1 = new Pyramid(new Vector3(0.8f, 0.8f, 0f));
            objects.Add(p1);

            cam.Position += new Vector3(0f, 0f, 3f);

            CursorVisible = false;
            CursorGrabbed = true;
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            ProcessInput();

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();


            int vertcount = 0;

            foreach (Volume v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                vertcount += v.VertCount;
                texcoords.AddRange(v.GetTextureCoords());
            }

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();

            time += (float)e.Time;
            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }
            objects[0].Position = new Vector3(0.3f, -0.5f, -3.0f);
            objects[0].Rotation = new Vector3(0, 0, 0);
            objects[0].Scale = new Vector3(1f, 1f, 1f);

            objects[1].Position = new Vector3(13f, 0f, -12.0f);
            objects[1].Rotation = new Vector3(0, 0.5f, 0.5f);
            objects[1].Scale = new Vector3(8f, 8f, 8f);

            objects[2].Position = new Vector3(2f, -3f, -6.0f);
            objects[2].Rotation = new Vector3(0.5f, 0.5f, 0.5f);
            objects[2].Scale = new Vector3(1.5f, 1f, 1f);

            objects[3].Position = new Vector3(-3f, 2f, -9.0f);
            objects[3].Rotation = new Vector3(0.3f, 0.7f, 0);
            objects[3].Scale = new Vector3(1f, 1f, 1f);

            objects[4].Position = new Vector3(-1f, -4.2f, -2f);
            objects[4].Rotation = new Vector3(0, 0, 0);
            objects[4].Scale = new Vector3(30f, 30f, 30f);

            objects[5].Position = new Vector3(-15f, -0.5f, -20.0f);
            objects[5].Rotation = new Vector3(0, 0, 0);
            objects[5].Scale = new Vector3(40f, 40f, 40f);


            foreach (Volume v in objects)
            {
                v.CalculateModelMatrix();
                v.ViewProjectionMatrix = cam.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(1.3f, ClientSize.Width / (float)ClientSize.Height, 1.0f, 40.0f);
                v.ModelViewProjectionMatrix = v.ModelMatrix * v.ViewProjectionMatrix;
            }
            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
        }

        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);

            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);

            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadMatrix(ref projection);
        }
        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.AccumBufferBit);

            Matrix4 aamat = Matrix4.Identity;

            shaders[activeShader].EnableVertexAttribArrays();
            switch (mode)
            {
                case eAntyAliasingMode.Off:
                case eAntyAliasingMode.Multisample:
                    {
                        OffRender(aamat);
                        break;
                    }
                case eAntyAliasingMode.Accum_Buffer_3:
                    {
                        AccumRender(aamat);
                        break;
                    }
            }

            shaders[activeShader].DisableVertexAttribArrays();

            GL.Flush();

            SwapBuffers();
        }

        public void ProcessInput()
        {
            switch (inputType)
            {
                case eInputType.Keyboard:
                    KeyboardInput();
                    break;
                case eInputType.Menu:
                    MenuInput();
                    break;
            }
        }

        protected void OffRender(Matrix4 aamat)
        {
            int indiceat = 0;
            foreach (Volume v in objects)
            {

                Matrix4 mvp = v.ModelViewProjectionMatrix * aamat;
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref mvp);

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }
        }

        protected void AccumRender(Matrix4 aamat)
        {
            int jitter;
            for (jitter = 0; jitter < numJitters; jitter++)
            {
                Vector3 vector3 = new Vector3((float)(jitter % 4) / (4 * Width), (float)(jitter / 4) / (4 * Height), 0.0f);
                if (jitter % 2 == 0)
                {
                    Matrix4.CreateTranslation(ref vector3, out aamat);
                }
                else
                {
                    vector3 *= -1.0f;
                    Matrix4.CreateTranslation(ref vector3, out aamat);
                }
                OffRender(aamat);
                GL.Accum(AccumOp.Accum, 1.0f / (float)numJitters);
            }
            GL.Accum(AccumOp.Return, 1.0f);
        }

        protected void KeyboardInput()
        {
            if (Keyboard.GetState().IsKeyDown(Key.W))
            {
                cam.Move(0f, 0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.S))
            {
                cam.Move(0f, -0.1f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                cam.Move(-0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                cam.Move(0.1f, 0f, 0f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Q))
            {
                cam.Move(0f, 0f, 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.E))
            {
                cam.Move(0f, 0f, -0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Number1))
            {
                Title = "No Antyaliasing";
                GL.Disable(EnableCap.Multisample);
                mode = eAntyAliasingMode.Off;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Number2))
            {
                Title = "Bufor akumalacyjny 5 x 5";
                GL.Disable(EnableCap.Multisample);
                numJitters = 25;
                mode = eAntyAliasingMode.Accum_Buffer_3;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Number3))
            {
                Title = "Bufor akumalacyjny 7 X 7";
                GL.Disable(EnableCap.Multisample);
                numJitters = 49;
                mode = eAntyAliasingMode.Accum_Buffer_3;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Number4))
            {
                GL.Enable(EnableCap.Multisample);
                GL.Hint(HintTarget.MultisampleFilterHintNv, HintMode.Nicest);
                Title = "Multisample";
                mode = eAntyAliasingMode.Multisample;
            }

            if (Keyboard.GetState().IsKeyDown(Key.Number5))
            {
                CursorGrabbed = false;
                CursorVisible = true;

                cam.Position = new Vector3(0f, 0f, 3f);
                cam.Orientation = new Vector3((float)Math.PI, 0f, 0f);
                var view = cam.accCamera();
                for (int i = 1; i <= 5; i++)
                {
                    objects.Add(new MenuBox(new Vector3(((i * 0.67f) % 1.0f), i * 0.68f, (i * 0.47f) % 1.0f)));
                    objects.Last().Position = (new Vector3(view.X - 1.45f + i * 0.20f, view.Y - 0.65f, view.Z - 0.1f));
                }
                inputType = eInputType.Menu;
            }
            if (Focused)
            {
                Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                lastMousePos += delta;

                cam.AddRotation(delta.X, delta.Y);
                lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }

            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
        protected void MenuInput()
        {
            MouseState mouseState = Mouse.GetCursorState();
            Point mousePos = PointToClient(new Point(mouseState.X, mouseState.Y));
            var mouseClick = mouseState.IsButtonDown(MouseButton.Left);

            if (mouseClick && mousePos.Y >= 7 * (Height / 8))
            {
                if (mousePos.X <= (Width / 13))
                {
                    Title = "No Antyaliasing";
                    GL.Disable(EnableCap.Multisample);
                    mode = eAntyAliasingMode.Off;
                }
                else if (mousePos.X >= (Width / 13) && mousePos.X <= 2 * (Width / 13))
                {
                    Title = "Bufor akumalacyjny 5 x 5";
                    GL.Disable(EnableCap.Multisample);
                    numJitters = 25;
                    mode = eAntyAliasingMode.Accum_Buffer_3;
                }
                else if (mousePos.X >= 2 * (Width / 13) && mousePos.X <= 3 * (Width / 13))
                {
                    Title = "Bufor akumalacyjny 7 X 7";
                    GL.Disable(EnableCap.Multisample);
                    numJitters = 49;
                    mode = eAntyAliasingMode.Accum_Buffer_3;
                }
                else if (mousePos.X >= 3 * (Width / 13) && mousePos.X <= 4 * (Width / 13))
                {
                    GL.Enable(EnableCap.Multisample);
                    GL.Hint(HintTarget.MultisampleFilterHintNv, HintMode.Nicest);
                    Title = "Multisample";
                    mode = eAntyAliasingMode.Multisample;
                }
                else if (mousePos.X >= 4 * (Width / 13) && mousePos.X <= 5 * (Width / 13))
                {
                    CursorGrabbed = true;
                    CursorVisible = false;


                    objects.RemoveRange(objects.Count - 5, 5);

                    inputType = eInputType.Keyboard;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                Exit();
            }
        }
    }
}
