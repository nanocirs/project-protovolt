using Godot;

public partial class MainMenuManager : Node {

    public enum MenuPage {
        Main = 0,
        Lobby = 1,
        Cars = 2,
    }

    private MainMenu mainMenu = null;
    private LobbyMenu lobbyMenu = null;
    private CarMenuLoader carsMenu = null;

    private bool isMainMenuValid = true;

    public override void _Ready() {

        mainMenu = GetNodeOrNull<MainMenu>("Main");
        lobbyMenu = GetNodeOrNull<LobbyMenu>("Lobby");
        carsMenu = GetNodeOrNull<CarMenuLoader>("Cars");

        if (!IsMainMenuValid()) {
            return;
        }

        SetMenuPage(MenuPage.Main);
        
    }

    public void SetMenuPage(MenuPage page) {

        if (!isMainMenuValid) {
            return;
        }

        mainMenu.Hide();
        lobbyMenu.Hide();
        carsMenu.Hide();

        switch (page) {
            case MenuPage.Main:
                mainMenu.Show();
                break;
            case MenuPage.Lobby:
                lobbyMenu.Show();
                break;
            case MenuPage.Cars:
                carsMenu.Load();
                break;
            default:
                mainMenu.Show();
                break;
        }

    }

    private bool IsMainMenuValid() {

        if (mainMenu == null) {
            isMainMenuValid = false;
            GD.PrintErr("EntryMenu not assigned in Main Menu.");
        }

        if (lobbyMenu == null) {
            isMainMenuValid = false;
            GD.PrintErr("LobbyMenu not assigned in Main Menu.");
        }

        if (carsMenu == null) {
            isMainMenuValid = false;
            GD.PrintErr("CarMenuLoader not assigned in Main Menu.");
        }

        return isMainMenuValid;

    }

}
