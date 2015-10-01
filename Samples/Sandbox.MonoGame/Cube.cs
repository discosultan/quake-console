using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sandbox
{
    public class Cube
    {        
        public const int NumIndices = 36;
        public const int NumVertices = 8;

        private readonly GraphicsDevice _device;
        private readonly BasicEffect _effect;
        
        public VertexPositionColor[] Vertices { get; private set; }
        private DynamicVertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Matrix _world;        

        public Cube(GraphicsDevice device, BasicEffect effect)
        {
            _device = device;
            _effect = effect;
            _world = Matrix.Identity;
            CreateCubeVertexBuffer();
            CreateCubeIndexBuffer();
        }

        public Vector3 Position;
        public Vector3 Scale = Vector3.One;
        public Vector3 Rotation;
        
        public Vector3 RotationSpeed = new Vector3(0, MathHelper.PiOver2, 0);

        public void Update(float deltaSeconds)
        {
            Rotation = new Vector3(
                MathHelper.WrapAngle(Rotation.X + RotationSpeed.X*deltaSeconds),
                MathHelper.WrapAngle(Rotation.Y + RotationSpeed.Y*deltaSeconds),
                MathHelper.WrapAngle(Rotation.Z + RotationSpeed.Z*deltaSeconds));

            _world = Matrix.CreateScale(Scale)*
                    Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)*
                    Matrix.CreateTranslation(Position);
        }

        public void Draw()
        {            
            _vertexBuffer.SetData(Vertices);
            _device.SetVertexBuffer(_vertexBuffer);
            _device.Indices = _indexBuffer;
            
            _effect.World = _world;
            _effect.CurrentTechnique.Passes[0].Apply();

            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumVertices, 0, NumIndices / 3);
        }

        void CreateCubeVertexBuffer()
        {
            Vertices = new VertexPositionColor[NumVertices];
            
            Vertices[0].Position = new Vector3(-1, -1, -1);
            Vertices[1].Position = new Vector3(-1, -1, 1);
            Vertices[2].Position = new Vector3(1, -1, 1);
            Vertices[3].Position = new Vector3(1, -1, -1);
            Vertices[4].Position = new Vector3(-1, 1, -1);
            Vertices[5].Position = new Vector3(-1, 1, 1);
            Vertices[6].Position = new Vector3(1, 1, 1);
            Vertices[7].Position = new Vector3(1, 1, -1);

            Vertices[0].Color = Color.Black;
            Vertices[1].Color = Color.Red;
            Vertices[2].Color = Color.Yellow;
            Vertices[3].Color = Color.Green;
            Vertices[4].Color = Color.Blue;
            Vertices[5].Color = Color.Magenta;
            Vertices[6].Color = Color.White;
            Vertices[7].Color = Color.Cyan;

            _vertexBuffer = new DynamicVertexBuffer(_device, VertexPositionColor.VertexDeclaration, 8, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(Vertices);
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

            _indexBuffer = new IndexBuffer(_device, IndexElementSize.SixteenBits, 36, BufferUsage.WriteOnly);
            _indexBuffer.SetData(cubeIndices);
        }
    }
}
