//using System;

//namespace GameStateComponents {
//    public class GameStateTest {
//        public GameState gs;

//        public static void Main() {
//            double[] position = new double[2] { 5, 3 };
//            gs = GameState.Instance;
//            gs.addPlayer();
//            gs.addPlayer();
//            gs.addPlayer();
//            gs.addPlayer();
//            gs.addPlayer();

//            gs.updateHealth(0, 200);
//            gs.updatePosition(0, 20, 20);
//            gs.updateHealth(4, 50);
//            gs.updatePosition(3, position);

//            for (int i = 0; i < 5; i++) {
//                Console.WriteLine(gs.getHealth(i));
//                Console.WriteLine(gs.getPosition(i));
//            }
//        }
//    }
//}