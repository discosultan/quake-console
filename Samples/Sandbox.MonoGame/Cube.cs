using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox
{
    public class Cube
    {        
        public const int NumIndices = 36;
        public const int NumVertices = 8;

        private readonly GraphicsDevice _device;
        private readonly BasicEffect effect;

        private VertexBuffer vertices;
        private IndexBuffer indices;
        private Matrix world, view, projection;

        public Cube(GraphicsDevice device)
        {
            _device = device;
            effect = new BasicEffect(device) { VertexColorEnabled = true };
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, _device.Viewport.AspectRatio, 1, 1000);
            CreateCubeVertexBuffer();
            CreateCubeIndexBuffer();
        }

        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; }

        public Vector3 RotationSpeed { get; set; } = new Vector3(0, MathHelper.PiOver2, 0);

        public void Update(float deltaSeconds)
        {
            Rotation = new Vector3(
                MathHelper.WrapAngle(Rotation.X + RotationSpeed.X*deltaSeconds),
                MathHelper.WrapAngle(Rotation.Y + RotationSpeed.Y*deltaSeconds),
                MathHelper.WrapAngle(Rotation.Z + RotationSpeed.Z*deltaSeconds));

            world = Matrix.CreateScale(Scale)*
                    Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)*
                    Matrix.CreateTranslation(Position);
        }

        public void Draw()
        {
            _device.SetVertexBuffer(vertices);
            _device.Indices = indices;

            effect.View = view;
            effect.Projection = projection;
            effect.World = world;
            effect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumVertices, 0, NumIndices / 3);
        }

        void CreateCubeVertexBuffer()
        {
            var cubeVertices = new VertexPositionColor[NumVertices];

            cubeVertices[0].Position = new Vector3(-1, -1, -1);
            cubeVertices[1].Position = new Vector3(-1, -1, 1);
            cubeVertices[2].Position = new Vector3(1, -1, 1);
            cubeVertices[3].Position = new Vector3(1, -1, -1);
            cubeVertices[4].Position = new Vector3(-1, 1, -1);
            cubeVertices[5].Position = new Vector3(-1, 1, 1);
            cubeVertices[6].Position = new Vector3(1, 1, 1);
            cubeVertices[7].Position = new Vector3(1, 1, -1);

            cubeVertices[0].Color = Color.Black;
            cubeVertices[1].Color = Color.Red;
            cubeVertices[2].Color = Color.Yellow;
            cubeVertices[3].Color = Color.Green;
            cubeVertices[4].Color = Color.Blue;
            cubeVertices[5].Color = Color.Magenta;
            cubeVertices[6].Color = Color.White;
            cubeVertices[7].Color = Color.Cyan;

            vertices = new VertexBuffer(_device, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            vertices.SetData(cubeVertices);
        }

        void CreateCubeIndexBuffer()
        {
            var cubeIndices = new ushort[NumIndices];

            //bottom face
            cubeIndices[0] = 0;
            cubeIndices[1] = 2;
            cubeIndices[2] = 3;
            cubeIndices[3] = 0;
            cubeIndices[4] = 1;
            cubeIndices[5] = 2;

            //top face
            cubeIndices[6] = 4;
            cubeIndices[7] = 6;
            cubeIndices[8] = 5;
            cubeIndices[9] = 4;
            cubeIndices[10] = 7;
            cubeIndices[11] = 6;

            //front face
            cubeIndices[12] = 5;
            cubeIndices[13] = 2;
            cubeIndices[14] = 1;
            cubeIndices[15] = 5;
            cubeIndices[16] = 6;
            cubeIndices[17] = 2;

            //back face
            cubeIndices[18] = 0;
            cubeIndices[19] = 7;
            cubeIndices[20] = 4;
            cubeIndices[21] = 0;
            cubeIndices[22] = 3;
            cubeIndices[23] = 7;

            //left face
            cubeIndices[24] = 0;
            cubeIndices[25] = 4;
            cubeIndices[26] = 1;
            cubeIndices[27] = 1;
            cubeIndices[28] = 4;
            cubeIndices[29] = 5;

            //right face
            cubeIndices[30] = 2;
            cubeIndices[31] = 6;
            cubeIndices[32] = 3;
            cubeIndices[33] = 3;
            cubeIndices[34] = 6;
            cubeIndices[35] = 7;

            indices = new IndexBuffer(_device, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
            indices.SetData(cubeIndices);
        }
    }
}
