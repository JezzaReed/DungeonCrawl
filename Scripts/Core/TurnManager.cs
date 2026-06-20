namespace NewGameProject;

public enum TurnState { PlayerTurn, EnemyTurn, GameOver }

public class TurnManager
{
    public TurnState State { get; private set; } = TurnState.PlayerTurn;

    public void EndPlayerTurn() => State = TurnState.EnemyTurn;
    public void EndEnemyTurn()  => State = TurnState.PlayerTurn;
    public void SetGameOver()   => State = TurnState.GameOver;
}
