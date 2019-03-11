using System;

namespace GameStateComponents {
    public class GameStateTest {
        public static GameState gs;

        public static void Main() {
            double[] position = new double[2] { 5, 3 };
            gs = GameState.Instance;
            int player1 = gs.addPlayer();
            int player2 = gs.addPlayer();
            int player3 = gs.addPlayer();
            int player4 = gs.addPlayer();
            int player5 = gs.addPlayer();

            gs.updateHealth(player1, 200);
            gs.updatePosition(player1, 20, 20);
            gs.updateHealth(player5, 50);
            gs.updatePosition(player4, position);

            for (int i = 0; i < 5; i++) {
                Console.WriteLine(gs.getHealth(i));
                Console.WriteLine("[{0}]", string.Join(", ", gs.getPosition(i)));
            }
        }
    }
}