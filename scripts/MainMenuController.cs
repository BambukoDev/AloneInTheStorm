using Godot;

public partial class MainMenuController : Control {
    Button play, settings, credits, quit;
    public override void _Ready() {
        play = GetNode<Button>("buttons/play");
        settings = GetNode<Button>("buttons/settings");
        credits = GetNode<Button>("buttons/credits");
        quit = GetNode<Button>("buttons/quit");
    }
}
