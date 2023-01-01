public static class CommandEx
{
    public static Command Create(this Command cmd) => (Command)Activator.CreateInstance(cmd.GetType())!;
}