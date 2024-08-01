using Godot;

public partial class PlayerController : CharacterBody3D
{
    Vector3 _moveDir;
    Vector3 _rotation;
    Vector2 _mousePositionOld;
    private bool _isJumping;

    public override void _Process(double delta)
    {
        var position = GetViewport().GetMousePosition();
        var diff = position - _mousePositionOld;
        _mousePositionOld = position;

        _rotation = new Vector3(diff.Y, diff.X, 0).Normalized();

        if (Input.IsActionPressed("move_forward"))
        {
            _moveDir.Z = -1;
        }

        if (Input.IsActionPressed("move_back"))
        {
            _moveDir.Z = 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            _moveDir.X = -1;
        }

        if (Input.IsActionPressed("move_right"))
        {
            _moveDir.X = 1;
        }

        if (Input.IsActionJustReleased("move_forward"))
        {
            _moveDir.Z = 0;
        }

        if (Input.IsActionJustReleased("move_back"))
        {
            _moveDir.Z = 0;
        }

        if (Input.IsActionJustReleased("move_left"))
        {
            _moveDir.X = 0;
        }

        if (Input.IsActionJustReleased("move_right"))
        {
            _moveDir.X = 0;
        }

        if (Input.IsActionJustPressed("jump"))
        {
            _isJumping = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
        {
            Velocity = new Vector3(Velocity.X, Velocity.Y - 9.8f * (float)delta, Velocity.Z);
        }
        else if (_isJumping)
        {
            Velocity = Velocity with { Y = 10f };
            _isJumping = false;
        }
        else
        {
            Velocity = _moveDir.Normalized() * 500f * (float)delta;
        }

        GD.Print(Velocity);

        MoveAndSlide();
    }
}