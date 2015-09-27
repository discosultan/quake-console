using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sandbox
{
    class CameraControllerComponent : GameComponent
    {
        private KeyboardState _previousKeyboardState;
        private KeyboardState _currentKeyboardState;

        private Vector3 _position = new Vector3(0, 5, 10);
        //private Quaternion _rotation = Quaternion.Identity;
        private Matrix _rotation = Matrix.CreateFromAxisAngle(Vector3.Right, -MathHelper.PiOver4 * 0.5f);

        private Vector2 _previousMousePos;        

        public CameraControllerComponent(Game game) : base(game)
        {            
        }

        public void LoadContent()
        {
            CalculateProjection();
            CalculateView();
        }

        public Keys MoveLeftKey { get; set; } = Keys.A;
        public Keys MoveRightKey { get; set; } = Keys.D;
        public Keys MoveUpKey { get; set; } = Keys.R;
        public Keys MoveDownKey { get; set; } = Keys.F;
        public Keys MoveForwardKey { get; set; } = Keys.W;
        public Keys MoveBackwardKey { get; set; } = Keys.S;

        public float MovementSpeed { get; set; } = 10.0f;
        public float RotationSpeed = 1.0f;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public override void Update(GameTime gameTime)
        {
            float deltaSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

            HandleKeyboard(deltaSeconds);
            HandleMouse(deltaSeconds);
            CalculateView();
        }

        private void HandleKeyboard(float deltaSeconds)
        {
            _currentKeyboardState = Keyboard.GetState();

            Vector3 movement = Vector3.Zero;
            if (IsKeyDown(MoveLeftKey))
                movement.X -= 1;
            if (IsKeyDown(MoveRightKey))
                movement.X += 1;
            if (IsKeyDown(MoveUpKey))
                movement.Y += 1;
            if (IsKeyDown(MoveDownKey))
                movement.Y -= 1;
            if (IsKeyDown(MoveForwardKey))
                movement.Z -= 1;
            if (IsKeyDown(MoveBackwardKey))
                movement.Z += 1;

            if (movement != Vector3.Zero)
            {
                movement.Normalize();
                movement = Vector3.TransformNormal(movement, _rotation);
                _position += movement * MovementSpeed * deltaSeconds;                
            }

            _previousKeyboardState = _currentKeyboardState;
        }

        private void HandleMouse(float deltaSeconds)
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
                        amount *= RotationSpeed*0.001f;
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

        private bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key); //&& _previousKeyboardState.IsKeyUp(key);
        }
    }
}
