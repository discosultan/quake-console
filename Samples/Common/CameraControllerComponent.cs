using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace QuakeConsole.Samples.Common
{
    public class CameraControllerComponent : GameComponent
    {
        private const float RotationScaleFactor = 1 / 1000f;

        private Vector3 _position = new Vector3(0, 5, 10);
        private Matrix _rotation = Matrix.CreateFromAxisAngle(Vector3.Right, -MathHelper.PiOver4 * 0.5f);

        private Vector2 _previousMousePos;        

        public CameraControllerComponent(Game game) : base(game)
        { }

        public override void Initialize()
        {
            CalculateProjection();
            CalculateView();
        }

        public Keys MoveLeftKey { get; set; } = Keys.A;
        public Keys MoveRightKey { get; set; } = Keys.D;
        public Keys MoveUpKey { get; set; } = Keys.E;
        public Keys MoveDownKey { get; set; } = Keys.Q;
        public Keys MoveForwardKey { get; set; } = Keys.W;
        public Keys MoveBackwardKey { get; set; } = Keys.S;

        public float MovementSpeed { get; set; } = 10.0f;
        public float RotationSpeed = 2.0f;

        public bool InvertMouseX { get; set; }
        public bool InvertMouseY { get; set; }

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public override void Update(GameTime gameTime)
        {
            float deltaSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

            HandleKeyboard(deltaSeconds);
            HandleMouse();
            CalculateView();
        }

        private void HandleKeyboard(float deltaSeconds)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            Vector3 movement = Vector3.Zero;
            if (keyboardState.IsKeyDown(MoveLeftKey))
                movement.X -= 1;
            if (keyboardState.IsKeyDown(MoveRightKey))
                movement.X += 1;
            if (keyboardState.IsKeyDown(MoveUpKey))
                movement.Y += 1;
            if (keyboardState.IsKeyDown(MoveDownKey))
                movement.Y -= 1;
            if (keyboardState.IsKeyDown(MoveForwardKey))
                movement.Z -= 1;
            if (keyboardState.IsKeyDown(MoveBackwardKey))
                movement.Z += 1;

            if (movement != Vector3.Zero)
            {
                movement.Normalize();
                movement = Vector3.TransformNormal(movement, _rotation);
                _position += movement * MovementSpeed * deltaSeconds;                
            }
        }

        private void HandleMouse()
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 mousePos = mouseState.Position.ToVector2();
                if (_previousMousePos != Vector2.Zero)
                {
                    Vector2 amount = mousePos - _previousMousePos;                    
                    if (amount != Vector2.Zero)
                    {
                        amount *= RotationSpeed * RotationScaleFactor;
                        if (!InvertMouseX) amount.X *= -1;
                        if (!InvertMouseY) amount.Y *= -1;
                        _rotation = Matrix.CreateFromYawPitchRoll(amount.X, amount.Y, 0) * _rotation;
                    }
                }
                _previousMousePos = mousePos;
            }
            else
            {
                _previousMousePos = Vector2.Zero;
            }
        }

        private void CalculateView()
        {                                                
            Vector3 target = _position + _rotation.Forward;
            View = Matrix.CreateLookAt(_position, target, _rotation.Up);
        }

        private void CalculateProjection()
        {            
            Viewport viewport = Game.GraphicsDevice.Viewport;
            float aspectRatio = viewport.Width / (float)viewport.Height;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 1000);
        }
    }
}
